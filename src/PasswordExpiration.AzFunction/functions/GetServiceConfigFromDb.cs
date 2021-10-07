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

    using Helpers.Services;
    using Models.Configs;

    public class GetServiceConfigsFromDb
    {
        private readonly ICosmosDbService cosmosDbService;
        public GetServiceConfigsFromDb(ICosmosDbService _cosmosDbService)
        {
            cosmosDbService = _cosmosDbService;
        }


        [Function("GetServiceConfigsFromDb")]
        public HttpResponseData Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "serviceConfigs")] HttpRequestData req,
            FunctionContext executionContext
        )
        {
            ILogger logger = executionContext.GetLogger("GetServiceConfigsFromDb");
            logger.LogInformation("Getting configs from DB.");

            List<UserSearchConfigItem> searchConfigs = cosmosDbService.GetUserSearchConfigs(logger);
            string searchConfigsJson = JsonConverter.ConvertToJson<List<UserSearchConfigItem>>(searchConfigs);

            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            response.WriteString(searchConfigsJson);

            return response;
        }
    }
}