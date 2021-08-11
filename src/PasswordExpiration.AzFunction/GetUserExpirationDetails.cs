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

    public class GetUserExpirationDetails
    {
        private readonly IGraphClientService graphClientSvc;
        public GetUserExpirationDetails(IGraphClientService _graphClientSvc)
        {
            graphClientSvc = _graphClientSvc;
        }

        [Function("GetUserExpirationDetails")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetUserExpirationDetails/{userPrincipalName}")] HttpRequestData req, string userPrincipalName, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("GetUserExpirationDetails");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            string maxAgeQuery = HttpUtility.ParseQueryString(req.Url.Query).Get("maxAge");
            int maxAge;
            logger.LogInformation($"maxAge: {maxAgeQuery}");

            switch (string.IsNullOrEmpty(maxAgeQuery))
            {
                case true:
                    maxAge = 56;
                    break;
                    
                default:
                    maxAge = Convert.ToInt32(maxAgeQuery);
                    break;
            }

            UserTools graphUserTools = new UserTools(graphClientSvc);

            User foundUser = graphUserTools.GetUser(userPrincipalName);
            UserPasswordExpirationDetails userPasswordExpirationDetails = new UserPasswordExpirationDetails(foundUser, TimeSpan.FromDays(maxAge), TimeSpan.FromDays(10));

            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            response.WriteString(JsonConverter.ConvertToJson<UserPasswordExpirationDetails>(userPasswordExpirationDetails));

            return response;
        }
}
}
