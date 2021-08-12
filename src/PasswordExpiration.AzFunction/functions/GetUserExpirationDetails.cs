using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace PasswordExpiration.AzFunction
{
    using Lib.Core;
    using Lib.Core.Graph;
    using Lib.Models.Core;
    using Lib.Models.Graph.Core;
    using Lib.Models.Graph.Users;

    using Helpers;
    using Helpers.Services;
    using Models.Configs;

    /// <summary>
    /// Get a specific user's password expiration details.
    /// </summary>
    public class GetUserExpirationDetails
    {
        private readonly IFunctionsConfigService functionsConfigSvc;
        private readonly IGraphClientService graphClientSvc;
        public GetUserExpirationDetails(IFunctionsConfigService _functionsConfigSvc, IGraphClientService _graphClientSvc)
        {
            functionsConfigSvc = _functionsConfigSvc;
            graphClientSvc = _graphClientSvc;
        }

        /// <summary>
        /// Get a specific user's password expiration details.
        /// </summary>
        /// <param name="req">The HTTPRequestData from the client.</param>
        /// <param name="userPrincipalName">The user principal name supplied in the client's request.</param>
        /// <param name="executionContext">The Azure Functions FunctionContext.</param>
        /// <returns></returns>
        [Function("GetUserExpirationDetails")]
        public HttpResponseData Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetUserExpirationDetails/{userPrincipalName}/{configId?}")]HttpRequestData req,
            string userPrincipalName,
            string configId,
            FunctionContext executionContext
        )
        {
            var logger = executionContext.GetLogger("GetUserExpirationDetails");
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

            // Search for the user and parse their password expiration details.
            User foundUser = graphUserTools.GetUser(userPrincipalName);
            UserPasswordExpirationDetails userPasswordExpirationDetails = new UserPasswordExpirationDetails(
                foundUser,
                TimeSpan.FromDays(userSearchConfig.MaxPasswordAge),
                TimeSpan.FromDays(10)
            );

            // Generate the response to send back to the client with the parsed user information.
            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            response.WriteString(JsonConverter.ConvertToJson<UserPasswordExpirationDetails>(userPasswordExpirationDetails));
            return response;
        }
    }
}
