using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace PasswordExpiration.AzFunction.Helpers.Services
{
    using Lib.Models.Core;
    using Lib.Models.Graph.Core;

    /// <summary>
    /// Hosts the Microsoft Graph session client and provides methods to send API calls to it. Acts as a service that is shared between functions.
    /// </summary>
    public class GraphClientService : IGraphClientService
    {
        public GraphClientService(ILogger<GraphClientService> _logger)
        {
            logger = _logger;
            Connect();
        }

        private readonly ILogger<GraphClientService> logger;

        public Uri BaseUri { get; set; }

        public bool IsConnected { get; set; }

        private IConfidentialClientAppConfig ConfidentialClientApp;

        private static HttpClient httpClient;

        public void Connect()
        {
            string appId = AppSettings.GetSetting("azureAdAppId");
            string azureAdTenantId = AppSettings.GetSetting("azureAdTenantId");
            string appSecret = AppSettings.GetSetting("azureAdAppSecret");

            ApiScopesConfig scopesConfig = new ApiScopesConfig()
            {
                Scopes = new string[]{
                    "https://graph.microsoft.com/.default"
                }
            };

            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://graph.microsoft.com/beta/");
            httpClient.DefaultRequestHeaders.Add("ConsistencyLevel", "eventual");

            ConfidentialClientApp = new ConfidentialClientAppWithSecret(appId, azureAdTenantId, appSecret, scopesConfig);
            ConfidentialClientApp.Connect();

            logger.LogInformation("Graph client was created.");
        }

        public void TestAuthToken()
        {
            DateTimeOffset currentDateTime = DateTimeOffset.Now;
            if (currentDateTime >= ConfidentialClientApp.AuthenticationResult.ExpiresOn)
            {
                logger.LogInformation("The authentication token for the GraphClient has expired. Refreshing...");
                ConfidentialClientApp.Connect();
                logger.LogInformation("Authentication token has been refreshed.");
            }
        }

        public string SendApiCall(string endpoint, string apiPostBody, HttpMethod httpMethod)
        {
            string apiResponse = null;

            HttpRequestMessage requestMessage = new HttpRequestMessage(httpMethod, endpoint);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.ConfidentialClientApp.AuthenticationResult.AccessToken);

            TestAuthToken();
            
            switch (String.IsNullOrEmpty(apiPostBody))
            {
                case false:
                    requestMessage.Content = new StringContent(apiPostBody);
                    requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    break;

                default:
                    break;
            }

            bool isFinished = false;
            //HttpResponseMessage responseMessage;

            while (!isFinished)
            {
                HttpResponseMessage responseMessage = httpClient.SendAsync(requestMessage).GetAwaiter().GetResult();

                switch (responseMessage.StatusCode)
                {
                    case HttpStatusCode.TooManyRequests:
                        RetryConditionHeaderValue retryAfterValue = responseMessage.Headers.RetryAfter;
                        TimeSpan retryAfterBuffer = retryAfterValue.Delta.Value.Add(TimeSpan.FromSeconds(15));

                        Console.WriteLine($"--- !!! Throttling for: {retryAfterBuffer.TotalSeconds} seconds !!! ---");
                        Thread.Sleep(retryAfterBuffer);
                        break;

                    default:
                        isFinished = true;

                        apiResponse = responseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        responseMessage.Dispose();
                        break;
                }
            }

            requestMessage.Dispose();

            return apiResponse;
        }
    }
}