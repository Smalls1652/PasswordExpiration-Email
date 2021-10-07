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
    using Lib.Core;
    using Lib.Core.Graph;
    using Lib.Models.Core;

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

        [Function("GetUsersWithExpiringPasswords")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetUsersWithExpiringPasswords/{configId}")] HttpRequestData req,
            string configId,
            FunctionContext executionContext
        )
        {
            var logger = executionContext.GetLogger("GetUsersWithExpiringPasswords");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            UserSearchConfigItem userSearchConfig = SharedFunctionMethods.GetUserSearchConfigItem(functionsConfigSvc, configId);

            logger.LogInformation($"User Search Config to be used: {userSearchConfig.Name} ({userSearchConfig.Id})");

            // Create the 'UserTools' object from the GraphClientService.
            UserTools graphUserTools = new UserTools(graphClientSvc);

            List<UserPasswordExpirationDetails> usersWithExpiringPasswords = await SharedFunctionMethods.GetUserDetailsByLastNames(
                graphUserTools,
                userSearchConfig,
                logger
            );


            // Generate the response to send back to the client with the parsed user information.
            logger.LogInformation("Returning data to client.");
            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            response.WriteString(JsonConverter.ConvertToJson<List<UserPasswordExpirationDetails>>(usersWithExpiringPasswords));
            return response;
        }
    }
}
