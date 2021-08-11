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
    using Models.PostBody;

    /// <summary>
    /// Get a specific user's password expiration details.
    /// </summary>
    public class GetUserExpirationDetails
    {
        private readonly IGraphClientService graphClientSvc;
        public GetUserExpirationDetails(IGraphClientService _graphClientSvc)
        {
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
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetUserExpirationDetails/{userPrincipalName}")] HttpRequestData req, string userPrincipalName, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("GetUserExpirationDetails");
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

            // Search for the user and parse their password expiration details.
            User foundUser = graphUserTools.GetUser(userPrincipalName);
            UserPasswordExpirationDetails userPasswordExpirationDetails = new UserPasswordExpirationDetails(foundUser, TimeSpan.FromDays(maxAge), TimeSpan.FromDays(10));

            // Generate the response to send back to the client with the parsed user information.
            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            response.WriteString(JsonConverter.ConvertToJson<UserPasswordExpirationDetails>(userPasswordExpirationDetails));
            return response;
        }
}
}
