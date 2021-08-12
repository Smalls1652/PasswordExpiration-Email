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

    public class RunAutomatedJobs
    {
        private readonly IFunctionsConfigService functionsConfigSvc;
        private readonly IGraphClientService graphClientSvc;
        public RunAutomatedJobs(IFunctionsConfigService _functionsConfigSvc, IGraphClientService _graphClientSvc)
        {
            functionsConfigSvc = _functionsConfigSvc;
            graphClientSvc = _graphClientSvc;
        }

        [Function("RunAutomatedJobs")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "runAutomatedJobs")] HttpRequestData req,
            FunctionContext executionContext
        )
        {
            ILogger logger = executionContext.GetLogger("SendExpirationNotices");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            // Get the setting for 'sendMailFromUPN'.
            string mailFromUPN = AppSettings.GetSetting("sendMailFromUPN");

            List<UserSearchConfigItem> searchConfigItems = new List<UserSearchConfigItem>();
            foreach (string automatedItem in functionsConfigSvc.AutomatedSearchConfigs)
            {
                searchConfigItems.Add(functionsConfigSvc.GetUserSearchConfig(automatedItem));
            }

            

            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            return response;
        }
    }
}
