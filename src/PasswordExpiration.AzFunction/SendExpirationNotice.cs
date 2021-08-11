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

    public class SendExpirationNotice
    {
        private readonly IGraphClientService graphClientSvc;
        public SendExpirationNotice(IGraphClientService _graphClientSvc)
        {
            graphClientSvc = _graphClientSvc;
        }

        [Function("SendExpirationNotice")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SendExpirationNotice/{userPrincipalName}")] HttpRequestData req,
            string userPrincipalName,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("SendExpirationNotice");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            string mailFromUPN = AppSettings.GetSetting("sendMailFromUPN");

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
            MailTools graphMailTools = new MailTools(graphClientSvc);

            User foundUser = graphUserTools.GetUser(userPrincipalName);
            UserPasswordExpirationDetails userPasswordExpirationDetails = new UserPasswordExpirationDetails(
                foundUser,
                TimeSpan.FromDays(maxAge),
                TimeSpan.FromDays(10)
            );
            string emailBody = MailBodyGenerator.CreateMailGenerator(
                userPasswordExpirationDetails,
                "./email_employee.html",
                TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")
            );

            graphMailTools.SendMessageWithAttachment(
                mailFromUPN,
                foundUser,
                $"Alert: Password Expiration Notice ({userPasswordExpirationDetails.PasswordExpiresIn.Value.Days} days)",
                emailBody,
                "./employee_pwdreset.png"
            );

            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);

            return response;
        }
}
}
