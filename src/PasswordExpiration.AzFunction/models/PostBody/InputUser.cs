using System;
using System.Text.Json.Serialization;

namespace PasswordExpiration.AzFunction.Models.PostBody
{
    public class InputUser
    {
        public InputUser() {}

        [JsonPropertyName("userPrincipalName")]
        public string UserPrincipalName { get; set; }

        [JsonPropertyName("maxPasswordAge")]
        public int MaxPasswordAge { get; set; }

        [JsonPropertyName("closeToExpirationDays")]
        public int CloseToExpirationDays { get; set; }
    }
}