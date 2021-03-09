﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace graph_email
{
    using Models.ClientApp;

    public class ClientCreator
    {
        public AuthenticationResult BuildApp(ClientAppConfig configSettings)
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

            authResult = GetToken(clientApp, apiScopes).GetAwaiter().GetResult();

            return authResult;
        }

        public async Task<AuthenticationResult> GetToken(IConfidentialClientApplication app, IEnumerable<string> scopes)
        {
            AuthenticationResult result;
            result = await app.AcquireTokenForClient(scopes)
                .ExecuteAsync();

            return result;

        }
    }
}