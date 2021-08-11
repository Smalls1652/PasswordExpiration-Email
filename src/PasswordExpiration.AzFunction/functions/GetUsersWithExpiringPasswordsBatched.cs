using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace PasswordExpiration.AzFunction
{
    using Lib;
    using Lib.Core;
    using Lib.Core.Graph;
    using Lib.Models.Core;
    using Lib.Models.Graph.Core;

    using Helpers;
    using Helpers.Services;

    public class GetUsersWithExpiringPasswordsBatched
    {
        private readonly IGraphClientService graphClientSvc;
        public GetUsersWithExpiringPasswordsBatched(IGraphClientService _graphClientSvc)
        {
            graphClientSvc = _graphClientSvc;
        }

        [Function("GetUsersWithExpiringPasswordsBatched")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetUsersWithExpiringPasswordsBatched/{domainName}")] HttpRequestData req, string domainName, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("GetUsersWithExpiringPasswords");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            // Check to see if 'maxAge' was provided in the query.
            string maxAgeQuery = HttpUtility.ParseQueryString(req.Url.Query).Get("maxAge");
            int maxAge;
            switch (string.IsNullOrEmpty(maxAgeQuery))
            {
                // If 'maxAge' is null (Not provided), then set the max age to the default.
                case true:
                    maxAge = 56;
                    break;

                // Otherwise, set the 'maxAge' to what the user provided.    
                default:
                    maxAge = Convert.ToInt32(maxAgeQuery);
                    break;
            }

            // Create the 'UserTools' object from the GraphClientService.
            UserTools graphUserTools = new UserTools(graphClientSvc);

            /*
                The following section is to prep for running the tasks by:
                    - Creating a task throttler (using SemaphoreSlim) to limit the amount of concurrent threads used to 10.
                    - Creating a 'ConcurrentBag' for the tasks to store each user data in.
                    - Creating a 'List' for storing the created tasks in.
            */
            SemaphoreSlim taskThrottler = new SemaphoreSlim(10);
            ConcurrentBag<UserPasswordExpirationDetails> usersWithExpiringPasswordsBag = new ConcurrentBag<UserPasswordExpirationDetails>();
            List<Task> taskList = new List<Task>();

            // Run a for loop to increment from 65 to 90 (The UTF32 decimal codes for the letters A-Z) to create the tasks to get users.
            for (int i = 65; i <= 90; i++)
            {
                // If we reach the max number of concurrent tasks, then wait to create the rest of the tasks.
                await taskThrottler.WaitAsync();

                // Convert the current loop integer (i) value into the corresponding UTF32 character.
                string lastNameStartsWith = Char.ConvertFromUtf32(i);
                logger.LogInformation($"Creating task for getting users, under the '{domainName}' domain and their last name starts with '{lastNameStartsWith}', with expiring passwords.");

                // Create the task to get the users.
                taskList.Add(
                    Task.Run(
                        () =>
                        {
                            try
                            {
                                // Get all of the users matching the search criteria.
                                List<UserPasswordExpirationDetails> userPasswordExpirationDetails = ExpiringPasswordFinder.GetUsersWithExpiringPasswords(
                                    graphUserTools,
                                    domainName,
                                    lastNameStartsWith,
                                    TimeSpan.FromDays(maxAge),
                                    TimeSpan.FromDays(10)
                                );

                                // Add each found user into the 'ConcurrentBag' of all the users.
                                foreach (UserPasswordExpirationDetails user in userPasswordExpirationDetails)
                                {
                                    usersWithExpiringPasswordsBag.Add(user);
                                }
                            }
                            finally
                            {
                                // When the task is finished, release the thread for the next task to utilize.
                                taskThrottler.Release();
                            }
                        }
                    )
                );
            }

            // Wait for the remaining tasks to finish.
            logger.LogInformation("Waiting for all remaining tasks to complete.");
            Task.WaitAll(taskList.ToArray());

            // Dispose of all of the tasks that were created.
            logger.LogInformation("Cleaning up resources.");
            foreach (Task task in taskList)
                task.Dispose();

            // Convert the 'ConcurrentBag' to a 'List'. Then clear all of the values out of the 'ConcurrentBag'.
            List<UserPasswordExpirationDetails> usersWithExpiringPasswords = new List<UserPasswordExpirationDetails>(
                usersWithExpiringPasswordsBag
            );
            usersWithExpiringPasswordsBag.Clear();

            // Sort all of the items alphabetically in the 'List' by each user's last name.
            usersWithExpiringPasswords.Sort((item1, item2) => string.Compare(item1.User.Surname, item2.User.Surname));

            // Generate the response to send back to the client with the parsed user information.
            logger.LogInformation("Returning data to client.");
            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            response.WriteString(JsonConverter.ConvertToJson<List<UserPasswordExpirationDetails>>(usersWithExpiringPasswords));
            return response;
        }
    }
}
