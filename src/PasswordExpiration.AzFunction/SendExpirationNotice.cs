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

    public static class SendExpirationNotice
    {
        [Function("SendExpirationNotice")]
        public static HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SendExpirationNotice/{userPrincipalName}")] HttpRequestData req,
            string userPrincipalName,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("SendExpirationNotice");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            string appId = AppSettings.GetSetting("azureAdAppId");
            string azureAdTenantId = AppSettings.GetSetting("azureAdTenantId");
            string appSecret = AppSettings.GetSetting("azureAdAppSecret");
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

            ApiScopesConfig scopesConfig = new ApiScopesConfig()
            {
                Scopes = new string[]{
                    "https://graph.microsoft.com/.default"
                }
            };

            GraphClient graphClient = new GraphClient(
                baseUri: new Uri("https://graph.microsoft.com/beta/"),
                clientId: appId,
                tenantId: azureAdTenantId,
                clientSecret: appSecret,
                apiScopes: scopesConfig
            );

            UserTools graphUserTools = new UserTools(graphClient);
            MailTools graphMailTools = new MailTools(graphClient);

            User foundUser = graphUserTools.GetUser(userPrincipalName);

            UserPasswordExpirationDetails userPasswordExpirationDetails = new UserPasswordExpirationDetails(foundUser, TimeSpan.FromDays(maxAge), TimeSpan.FromDays(10));
            string emailBody = MailBodyGenerator.CreateMailGenerator(userPasswordExpirationDetails, "./email_employee.html", TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

            graphMailTools.SendMessageWithAttachment(mailFromUPN, foundUser, $"Alert: Password Expiration Notice ({userPasswordExpirationDetails.PasswordExpiresIn.Value.Days} days)", emailBody, "./employee_pwdreset.png");

            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);

            return response;
        }
}
}
