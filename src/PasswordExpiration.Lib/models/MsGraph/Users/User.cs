using System;
using System.Text.Json.Serialization;

namespace PasswordExpiration.Lib.Models.Graph.Users
{
    public class User
    {
        public User() {}
        
        [JsonPropertyName("accountEnabled")]
        public bool AccountEnabled { get; set; }

        [JsonPropertyName("userPrincipalName")]
        public string UserPrincipalName { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("givenName")]
        public string GivenName { get; set; }

        [JsonPropertyName("surname")]
        public string Surname { get; set; }

        [JsonPropertyName("lastPasswordChangeDateTime")]
        public Nullable<DateTimeOffset> LastPasswordChangeDateTime { get; set; }

        [JsonPropertyName("onPremisesDistinguishedName")]
        public string OnPremisesDistinguishedName { get; set; }

        public override string ToString()
        {
            return this.UserPrincipalName;
        }
    }
}