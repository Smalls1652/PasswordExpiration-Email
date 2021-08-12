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
    using Models.Configs;
    using Helpers.Services;

    public class GetUsersWithExpiringPasswords
    {
        private readonly IFunctionsConfigService functionsConfigSvc;
        private readonly IGraphClientService graphClientSvc;
        public GetUsersWithExpiringPasswords(IFunctionsConfigService _functionsConfigSvc, IGraphClientService _graphClientSvc)
        {
            functionsConfigSvc = _functionsConfigSvc;
            graphClientSvc = _graphClientSvc;
        }

        [Function("GetUsersWithExpiringPassword")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetUsersWithExpiringPasswords/{configId}")] HttpRequestData req,
            string configId,
            FunctionContext executionContext
        )
        {
            var logger = executionContext.GetLogger("GetUsersWithExpiringPasswords");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            UserSearchConfigItem userSearchConfig;
            switch (string.IsNullOrEmpty(configId))
            {
                case true:
                    userSearchConfig = functionsConfigSvc.GetUserSearchConfig("65ef0352-9545-42f8-b70f-6dd21fa8df6b");
                    break;

                default:
                    userSearchConfig = functionsConfigSvc.GetUserSearchConfig(configId);
                    break;
            }

            logger.LogInformation($"User Search Config to be used: {userSearchConfig.Name} ({userSearchConfig.Id})");

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
                logger.LogInformation($"Creating task for getting users, under the '{userSearchConfig.DomainName}' domain and their last name starts with '{lastNameStartsWith}', with expiring passwords.");

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
                                    userSearchConfig.DomainName,
                                    userSearchConfig.OuPath,
                                    lastNameStartsWith,
                                    TimeSpan.FromDays(userSearchConfig.MaxPasswordAge),
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
