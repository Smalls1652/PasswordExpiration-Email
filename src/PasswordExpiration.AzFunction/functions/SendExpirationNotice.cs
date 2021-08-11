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

            // Get the setting for 'sendMailFromUPN'.
            string mailFromUPN = AppSettings.GetSetting("sendMailFromUPN");

            // Check to see if 'maxAge' was provided in the query.
            string maxAgeQuery = HttpUtility.ParseQueryString(req.Url.Query).Get("maxAge");
            int maxAge;
            switch (string.IsNullOrEmpty(maxAgeQuery))
            {
                // If 'maxAge' is null (Not provided), then set the max age to the default.
                case true:
                    maxAge = 56;
                    break;

                // Otherwise, set the 'maxAge' to what the user provided.    
                default:
                    maxAge = Convert.ToInt32(maxAgeQuery);
                    break;
            }

            // Create the 'UserTools' and 'MailTools' objects from the 'GraphClientService'.
            UserTools graphUserTools = new UserTools(graphClientSvc);
            MailTools graphMailTools = new MailTools(graphClientSvc);

            // Search for the user and parse their password expiration details.
            User foundUser = graphUserTools.GetUser(userPrincipalName);
            UserPasswordExpirationDetails userPasswordExpirationDetails = new UserPasswordExpirationDetails(
                foundUser,
                TimeSpan.FromDays(maxAge),
                TimeSpan.FromDays(10)
            );

            // Generate the email message body with the user's details.
            string emailBody = MailBodyGenerator.CreateMailGenerator(
                userPasswordExpirationDetails,
                "./email_employee.html",
                TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")
            );

            // Send the email to the user.
            graphMailTools.SendMessageWithAttachment(
                mailFromUPN,
                foundUser,
                $"Alert: Password Expiration Notice ({userPasswordExpirationDetails.PasswordExpiresIn.Value.Days} days)",
                emailBody,
                "./employee_pwdreset.png"
            );

            // Generate an 'Ok' response code back to the client.
            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            return response;
        }
}
}
