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

            string maxAgeQuery = HttpUtility.ParseQueryString(req.Url.Query).Get("maxAge");
            int maxAge;

            switch (string.IsNullOrEmpty(maxAgeQuery))
            {
                case true:
                    maxAge = 56;
                    break;

                default:
                    maxAge = Convert.ToInt32(maxAgeQuery);
                    break;
            }

            logger.LogInformation($"Password max age: {maxAge} days");

            /*
            ApiScopesConfig scopesConfig = new ApiScopesConfig()
            {
                Scopes = new string[]{
                    "https://graph.microsoft.com/.default"
                }
            };

            logger.LogInformation("Creating Graph Client.");
            GraphClient graphClient = new GraphClient(
                baseUri: new Uri("https://graph.microsoft.com/beta/"),
                clientId: appId,
                tenantId: azureAdTenantId,
                clientSecret: appSecret,
                apiScopes: scopesConfig
            );
            */

            //logger.LogInformation($"Getting users, with the '{domainName}' domain, with expiring passwords.");
            //GraphClient graphClient = executionContext.Items["graphClient"] as GraphClient;

            UserTools graphUserTools = new UserTools(graphClientSvc);

            SemaphoreSlim taskThrottler = new SemaphoreSlim(10);

            ConcurrentBag<UserPasswordExpirationDetails> usersWithExpiringPasswordsBag = new ConcurrentBag<UserPasswordExpirationDetails>();
            List<Task> taskList = new List<Task>();
            for (int i = 65; i <= 90; i++)
            {
                await taskThrottler.WaitAsync();

                string lastNameStartsWith = Char.ConvertFromUtf32(i);
                logger.LogInformation($"Creating task for getting users, under the '{domainName}' domain and their last name starts with '{lastNameStartsWith}', with expiring passwords.");

                taskList.Add(
                    Task.Run(
                        () =>
                        {
                            try
                            {
                                List<UserPasswordExpirationDetails> userPasswordExpirationDetails = ExpiringPasswordFinder.GetUsersWithExpiringPasswords(
                                    graphUserTools,
                                    domainName,
                                    lastNameStartsWith,
                                    TimeSpan.FromDays(maxAge),
                                    TimeSpan.FromDays(10)
                                );

                                foreach (UserPasswordExpirationDetails user in userPasswordExpirationDetails)
                                {
                                    usersWithExpiringPasswordsBag.Add(user);
                                }
                            }
                            finally
                            {
                                taskThrottler.Release();
                            }
                        }
                    )
                );
            }

            logger.LogInformation("Waiting for all remaining tasks to complete.");
            Task.WaitAll(taskList.ToArray());

            logger.LogInformation("Cleaning up resources.");
            foreach (Task task in taskList)
                task.Dispose();

            List<UserPasswordExpirationDetails> usersWithExpiringPasswords = new List<UserPasswordExpirationDetails>();
            foreach (UserPasswordExpirationDetails item in usersWithExpiringPasswordsBag)
            {
                usersWithExpiringPasswords.Add(item);
            }
            usersWithExpiringPasswords.Sort((item1, item2) => string.Compare(item1.User.Surname, item2.User.Surname));

            logger.LogInformation("Returning data to client.");
            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            response.WriteString(JsonConverter.ConvertToJson<List<UserPasswordExpirationDetails>>(usersWithExpiringPasswords));

            return response;
        }
    }
}
