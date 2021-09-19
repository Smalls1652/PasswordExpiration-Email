using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions;
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
        public async void Run(
            [TimerTrigger(
                schedule: "0 0 23 * * *",
                RunOnStartup = false
            )] TimerInfo timer,
            FunctionContext executionContext
        )
        {
            ILogger logger = executionContext.GetLogger("RunAutomatedJobs");
            if (timer.IsPastDue)
            {
                logger.LogInformation("Timer is running late!");
            }
            logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

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
                        false
                    );
                }

                logger.LogInformation("All automated jobs have finished running.");
            }
        }
    }
}
