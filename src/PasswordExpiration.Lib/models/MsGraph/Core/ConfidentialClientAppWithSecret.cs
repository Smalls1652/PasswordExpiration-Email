using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Identity.Client;

namespace PasswordExpiration.Lib.Models.Graph.Core
{
    /// <summary>
    /// The confidential client app for authenticating to the Microsoft Graph API when utilizing an application secret.
    /// </summary>
    public class ConfidentialClientAppWithSecret : IConfidentialClientAppConfig
    {
        public ConfidentialClientAppWithSecret() {}

        public ConfidentialClientAppWithSecret(string clientId, string tenantId, string secret, ApiScopesConfig scopesConfig)
        {
            ClientId = clientId;
            TenantId = tenantId;
            ScopesConfig = scopesConfig;
            clientSecret = secret;
        }

        public string ClientId { get; set; }

        public string TenantId { get; set; }

        public ApiScopesConfig ScopesConfig { get; set; }

        private protected string clientSecret;

        public IConfidentialClientApplication ConfidentialClientApp { get; set; }

        public AuthenticationResult AuthenticationResult { get; set; }

        public void Connect()
        {
            if (null == this.ConfidentialClientApp)
            {
                CreateClientApp();
            }

            Task<AuthenticationResult> getTokenTask = Task<AuthenticationResult>.Run(
                async () => await GetTokenForClientAsync(ScopesConfig.Scopes)
            );

            Task.WaitAll(getTokenTask);

            AuthenticationResult = getTokenTask.Result;
        }

        public void CreateClientApp()
        {
            ConfidentialClientApp = ConfidentialClientApplicationBuilder.Create(this.ClientId)
                .WithClientSecret(this.clientSecret)
                .Build();

            this.clientSecret = null;
        }

        private async Task<AuthenticationResult> GetTokenForClientAsync(IEnumerable<string> scopes)
        {
            AuthenticationResult authResult = null;

            authResult = await this.ConfidentialClientApp.AcquireTokenForClient(scopes)
                .WithAuthority(AzureCloudInstance.AzurePublic, this.TenantId)
                .ExecuteAsync();

            return authResult;
        }
    }
}