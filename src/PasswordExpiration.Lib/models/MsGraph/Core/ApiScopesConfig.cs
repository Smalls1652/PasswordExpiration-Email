using System;
using System.Collections.Generic;

namespace PasswordExpiration.Lib.Models.Graph.Core
{
    /// <summary>
    /// A collection of scopes to use when authenticating to the Microsoft Graph API.
    /// </summary>
    public class ApiScopesConfig
    {
        public IEnumerable<string> Scopes { get; set; }
    }
}