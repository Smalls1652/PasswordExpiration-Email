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
    using Models.Configs;
    using Helpers.Services;

    public class SendExpirationNotice
    {
        private readonly IFunctionsConfigService functionsConfigSvc;
        private readonly IGraphClientService graphClientSvc;
        public SendExpirationNotice(IFunctionsConfigService _functionsConfigSvc, IGraphClientService _graphClientSvc)
        {
            functionsConfigSvc = _functionsConfigSvc;
            graphClientSvc = _graphClientSvc;
        }

        [Function("SendExpirationNotice")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "sendExpirationNotice/{configId}/{userPrincipalName}")]HttpRequestData req,
            string configId,
            string userPrincipalName,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("SendExpirationNotice");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            // Get the setting for 'sendMailFromUPN'.
            string mailFromUPN = AppSettings.GetSetting("sendMailFromUPN");

            UserSearchConfigItem searchConfigItem = functionsConfigSvc.GetUserSearchConfig(configId);
            EmailTemplateItem emailTemplateItem = functionsConfigSvc.GetEmailTemplate(searchConfigItem.EmailTemplateId);

            // Create the 'UserTools' and 'MailTools' objects from the 'GraphClientService'.
            UserTools graphUserTools = new UserTools(graphClientSvc);
            MailTools graphMailTools = new MailTools(graphClientSvc);

            // Search for the user and parse their password expiration details.
            User foundUser = graphUserTools.GetUser(userPrincipalName);
            UserPasswordExpirationDetails userPasswordExpirationDetails = new UserPasswordExpirationDetails(
                foundUser,
                TimeSpan.FromDays(searchConfigItem.MaxPasswordAge),
                TimeSpan.FromDays(10)
            );

            // Generate the email message body with the user's details.
            string emailBody = MailBodyGenerator.CreateMailGenerator(
                userPasswordExpirationDetails,
                functionsConfigSvc.GetEmailTemplateHtmlFileFullPath(emailTemplateItem),
                TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")
            );

            // Send the email to the user.
            graphMailTools.SendMessageWithAttachment(
                mailFromUPN,
                foundUser,
                $"Alert: Password Expiration Notice ({userPasswordExpirationDetails.PasswordExpiresIn.Value.Days} days)",
                emailBody,
                functionsConfigSvc.GetEmailTemplateAttachmentFileFullPath(emailTemplateItem).ToArray()
            );

            // Generate an 'Ok' response code back to the client.
            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            return response;
        }
}
}
