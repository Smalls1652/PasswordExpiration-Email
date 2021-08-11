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
    using Lib;
    using Lib.Core;
    using Lib.Core.Graph;
    using Lib.Models.Core;
    using Lib.Models.Graph.Core;
    using Lib.Models.Graph.Users;

    using Helpers;
    using Helpers.Services;
    using Models.PostBody;

    public class GetUsersWithExpiringPasswords
    {
        private readonly IGraphClientService graphClientSvc;
        public GetUsersWithExpiringPasswords(IGraphClientService _graphClientSvc)
        {
            graphClientSvc = _graphClientSvc;
        }

        [Function("GetUsersWithExpiringPasswords")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetUsersWithExpiringPasswords/{domainName}")] HttpRequestData req, string domainName, FunctionContext executionContext)
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

            string lastNameStartsWithQuery = HttpUtility.ParseQueryString(req.Url.Query).Get("lastNameStartsWith");
            string lastNameStartsWith = null;

            switch (string.IsNullOrEmpty(lastNameStartsWithQuery))
            {
                case true:
                    lastNameStartsWith = null;
                    break;
                    
                default:
                    lastNameStartsWith = lastNameStartsWithQuery;
                    break;
            }

            logger.LogInformation($"Getting users, with the '{domainName}' domain, with expiring passwords.");
            UserTools graphUserTools = new UserTools(graphClientSvc);
            List<UserPasswordExpirationDetails> usersWithExpiringPasswords = ExpiringPasswordFinder.GetUsersWithExpiringPasswords(graphUserTools, domainName, lastNameStartsWith, TimeSpan.FromDays(maxAge), TimeSpan.FromDays(10));

            logger.LogInformation("Returning data to client.");
            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            response.WriteString(JsonConverter.ConvertToJson<List<UserPasswordExpirationDetails>>(usersWithExpiringPasswords));

            return response;
        }
}
}
