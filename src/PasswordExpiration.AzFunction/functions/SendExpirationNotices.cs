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

    public class SendExpirationNotices
    {
        private readonly IFunctionsConfigService functionsConfigSvc;
        private readonly IGraphClientService graphClientSvc;
        public SendExpirationNotices(IFunctionsConfigService _functionsConfigSvc, IGraphClientService _graphClientSvc)
        {
            functionsConfigSvc = _functionsConfigSvc;
            graphClientSvc = _graphClientSvc;
        }

        [Function("SendExpirationNotices")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sendExpirationNotices/{configId}")] HttpRequestData req,
            string configId,
            FunctionContext executionContext
        )
        {
            var logger = executionContext.GetLogger("SendExpirationNotices");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            // Get the setting for 'sendMailFromUPN'.
            string mailFromUPN = AppSettings.GetSetting("sendMailFromUPN");

            // Get the user search config template supplied in the request.
            UserSearchConfigItem userSearchConfig = SharedFunctionMethods.GetUserSearchConfigItem(
                functionsConfigSvc,
                configId
            );

            // Get the email template defined in the user search config.
            EmailTemplateItem emailTemplate = functionsConfigSvc.GetEmailTemplate(userSearchConfig.EmailTemplateId);

            logger.LogInformation($"User Search Config to be used: {userSearchConfig.Name} ({userSearchConfig.Id})");
            logger.LogInformation($"Email Template to be used: {emailTemplate.TemplateName} ({emailTemplate.TemplateId})");

            // Create the 'UserTools' object from the GraphClientService.
            UserTools graphUserTools = new(graphClientSvc);
            MailTools graphMailTools = new(graphClientSvc);

            // Get all users that match the criteria in the search config.
            List<UserPasswordExpirationDetails> usersWithExpiringPasswords = await SharedFunctionMethods.GetUserDetailsByLastNames(
                graphUserTools,
                userSearchConfig,
                logger
            );

            // Send an email to each user.
            foreach (UserPasswordExpirationDetails userItem in usersWithExpiringPasswords)
            {
                SharedFunctionMethods.SendEmailToUser(
                    logger,
                    graphMailTools,
                    mailFromUPN,
                    userItem,
                    functionsConfigSvc,
                    userSearchConfig,
                    false
                );

                logger.LogInformation($"Email has been sent to '{userItem.User.UserPrincipalName}'.");
            }

            // Generate the response to send back to the client with the parsed user information.
            logger.LogInformation("Returning data to client.");
            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            response.WriteString(JsonConverter.ConvertToJson<List<UserPasswordExpirationDetails>>(usersWithExpiringPasswords));
            return response;
        }
    }
}
