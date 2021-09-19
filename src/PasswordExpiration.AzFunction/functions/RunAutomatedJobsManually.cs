using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
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
    using Models.BodyInputs;
    using Helpers.Services;

    public class RunAutomatedJobsManually
    {
        private readonly IFunctionsConfigService functionsConfigSvc;
        private readonly IGraphClientService graphClientSvc;
        public RunAutomatedJobsManually(IFunctionsConfigService _functionsConfigSvc, IGraphClientService _graphClientSvc)
        {
            functionsConfigSvc = _functionsConfigSvc;
            graphClientSvc = _graphClientSvc;
        }

        [Function("RunAutomatedJobsManually")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, new string[] { "get", "post"}, Route = "RunAutomatedJobsManually")] HttpRequestData req,
            FunctionContext executionContext
        )
        {
            ILogger logger = executionContext.GetLogger("RunAutomatedJobsManually");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            bool jobIsTest = false;

            byte[] reqBodyByteArray = new byte[req.Body.Length];
            await req.Body.ReadAsync(reqBodyByteArray, 0, ((int)req.Body.Length));
            string reqBodyString = Encoding.UTF8.GetString(reqBodyByteArray);

            if (reqBodyString != null)
            {
                TestBody reqBody = JsonConverter.ConvertFromJson<TestBody>(reqBodyString);
                jobIsTest = reqBody.IsTest;
            }

            // Get the setting for 'sendMailFromUPN'.
            string mailFromUPN = AppSettings.GetSetting("sendMailFromUPN");

            UserTools graphUserTools = new(graphClientSvc);
            MailTools graphMailTools = new(graphClientSvc);

            List<UserSearchConfigItem> searchConfigItems = new();
            foreach (string automatedItem in functionsConfigSvc.AutomatedSearchConfigs)
            {
                searchConfigItems.Add(functionsConfigSvc.GetUserSearchConfig(automatedItem));
            }

            foreach (UserSearchConfigItem searchConfig in searchConfigItems)
            {
                logger.LogInformation($"Now running: {searchConfig.Name} ({searchConfig.Id})");

                EmailTemplateItem emailTemplate = functionsConfigSvc.GetEmailTemplate(searchConfig.EmailTemplateId);
                string emailTemplatePath = functionsConfigSvc.GetEmailTemplateHtmlFileFullPath(emailTemplate);
                string[] emailTemplateAttachmentPaths = functionsConfigSvc.GetEmailTemplateAttachmentFileFullPath(emailTemplate).ToArray();

                List<UserPasswordExpirationDetails> usersWithExpiringPasswords = await SharedFunctionMethods.GetUserDetailsByLastNames(
                    graphUserTools,
                    searchConfig,
                    logger
                );

                foreach (UserPasswordExpirationDetails userItem in usersWithExpiringPasswords)
                {
                    SharedFunctionMethods.SendEmailToUser(
                        logger,
                        graphMailTools,
                        mailFromUPN,
                        userItem,
                        functionsConfigSvc,
                        searchConfig,
                        jobIsTest
                    );
                }

                logger.LogInformation("All automated jobs have finished running.");
            }

            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            return response;
        }
    }
}
