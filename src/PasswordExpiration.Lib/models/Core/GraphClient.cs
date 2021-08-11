using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Identity.Client;

namespace PasswordExpiration.Lib.Models.Core
{
    using PasswordExpiration.Lib.Models.Graph.Core;
    public class GraphClient
    {
        public GraphClient(Uri baseUri, string clientId, string tenantId, X509Certificate2 clientCertificate, ApiScopesConfig apiScopes)
        {
            BaseUri = baseUri;
            httpClient = new HttpClient();
            httpClient.BaseAddress = baseUri;
            httpClient.DefaultRequestHeaders.Add("ConsistencyLevel", "eventual");

            ConfidentialClientApp = new ConfidentialClientAppWithCertificate(clientId, tenantId, clientCertificate, apiScopes);
            ConfidentialClientApp.Connect();
        }

        public GraphClient(Uri baseUri, string clientId, string tenantId, string clientSecret, ApiScopesConfig apiScopes)
        {
            BaseUri = baseUri;
            httpClient = new HttpClient();
            httpClient.BaseAddress = baseUri;
            httpClient.DefaultRequestHeaders.Add("ConsistencyLevel", "eventual");

            ConfidentialClientApp = new ConfidentialClientAppWithSecret(clientId, tenantId, clientSecret, apiScopes);
            ConfidentialClientApp.Connect();
        }

        public Uri BaseUri { get; set; }

        private IConfidentialClientAppConfig ConfidentialClientApp;

        private static HttpClient httpClient;

        public string SendApiCall(string endpoint, string apiPostBody, HttpMethod httpMethod)
        {
            string apiResponse = null;

            DateTimeOffset currentDateTime = DateTimeOffset.Now;
            if (currentDateTime >= ConfidentialClientApp.AuthenticationResult.ExpiresOn)
            {
                ConfidentialClientApp.Connect();
            }

            HttpRequestMessage requestMessage = new HttpRequestMessage(httpMethod, endpoint);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.ConfidentialClientApp.AuthenticationResult.AccessToken);

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