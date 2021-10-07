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

    using Helpers;
    using Helpers.Services;
    using Models.Configs;

    public class GetServiceConfig
    {
        private readonly IFunctionsConfigService functionsConfigSvc;
        public GetServiceConfig(IFunctionsConfigService _functionsConfigSvc)
        {
            functionsConfigSvc = _functionsConfigSvc;
        }


        [Function("GetServiceConfig")]
        public HttpResponseData Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "serviceConfig/{configType:required}/{configId?}")] HttpRequestData req,
            string configType,
            string configId,
            FunctionContext executionContext
        )
        {
            var logger = executionContext.GetLogger("GetServiceConfig");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            HttpResponseData response;
            string responseString;
            bool errorOccured = false;
            switch (configType)
            {
                case "emailTemplates":
                    if (string.IsNullOrEmpty(configId) == false)
                    {
                        EmailTemplateItem templateItem = functionsConfigSvc.GetEmailTemplate(configId);
                        if (templateItem == null)
                        {
                            errorOccured = true;
                            responseString = $"Could not find an email template with the id of '{configId}'.";
                        }
                        else
                        {
                            responseString = JsonConverter.ConvertToJson<EmailTemplateItem>(templateItem);
                        }
                    }
                    else
                    {
                        responseString = JsonConverter.ConvertToJson<List<EmailTemplateItem>>(functionsConfigSvc.EmailTemplates);
                    }
                    break;

                case "userSearchConfigs":
                    if (string.IsNullOrEmpty(configId) == false)
                    {
                        UserSearchConfigItem configItem = functionsConfigSvc.GetUserSearchConfig(configId);
                        if (configItem == null)
                        {
                            errorOccured = true;
                            responseString = $"Could not find a search config with the id of '{configId}'.";
                        }
                        else
                        {
                            responseString = JsonConverter.ConvertToJson<UserSearchConfigItem>(configItem);
                        }
                    }
                    else
                    {
                        responseString = JsonConverter.ConvertToJson<List<UserSearchConfigItem>>(functionsConfigSvc.UserSearchConfigs);
                    }
                    break;

                default:
                    errorOccured = true;
                    responseString = $"Requested item, {configType}, is not valid. Valid types are: emailTemplates, userSearchConfigs.";
                    break;
            }

            switch (errorOccured)
            {
                case true:
                    response = req.CreateResponse(HttpStatusCode.NotFound);
                    break;

                default:
                    response = req.CreateResponse(HttpStatusCode.OK);
                    response.Headers.Add("Content-Type", "application/json");
                    break;
            }

            response.WriteString(responseString);

            // Send the response back to the client.
            return response;
        }
    }
}
