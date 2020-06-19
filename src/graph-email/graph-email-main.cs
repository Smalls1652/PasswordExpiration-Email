using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace graph_email
{
    public class ClientAppConfig
    {
        public string ClientId { get; set; }
        public string TenantId { get; set; }
        public string ClientSecret { get; set; }
        public X509Certificate2 ClientCertificate { get; set; }
    }

    public class ClientCreator
    {
        public AuthenticationResult buildApp(ClientAppConfig configSettings)
        {

            switch ((string.IsNullOrEmpty(configSettings.ClientSecret) == false) && (null != configSettings.ClientCertificate))
            {
                case true:
                    throw (new Exception("Both the ClientSecret and ClientCertificate properties were provided. Only one can be provided."));

                default:
                    break;
            }

            IConfidentialClientApplication clientApp;
            AuthenticationResult authResult;

            string clientAuthority = $"https://login.microsoftonline.com/{configSettings.TenantId}";
            string[] apiScopes = new string[] { ".default" };

            switch ((null == configSettings.ClientCertificate))
            {
                case true:
                    clientApp = ConfidentialClientApplicationBuilder.Create(configSettings.ClientId)
                        .WithClientSecret(configSettings.ClientSecret)
                        .WithAuthority(clientAuthority)
                        .WithTenantId(configSettings.TenantId)
                        .Build();
                    break;

                default:
                    clientApp = ConfidentialClientApplicationBuilder.Create(configSettings.ClientId)
                        .WithCertificate(configSettings.ClientCertificate)
                        .WithAuthority(clientAuthority)
                        .WithTenantId(configSettings.TenantId)
                        .Build();
                    break;
            }

            authResult = getToken(clientApp, apiScopes).GetAwaiter().GetResult();

            return authResult;
        }

        public async Task<AuthenticationResult> getToken(IConfidentialClientApplication app, IEnumerable<string> scopes)
        {
            AuthenticationResult result;
            result = await app.AcquireTokenForClient(scopes)
                .ExecuteAsync();

            return result;

        }

        public X509Certificate2 GetClientCert(string thumbprint)
        {
            X509Certificate2 clientCert;

            X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            certStore.Open(OpenFlags.ReadOnly);

            X509Certificate2Collection certCollection = certStore.Certificates;

            X509Certificate2Collection validCerts = certCollection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
            X509Certificate2Collection foundCerts = validCerts.Find(X509FindType.FindByThumbprint, thumbprint, false);
            clientCert = foundCerts[0];

            return clientCert;
        }
    }
}
