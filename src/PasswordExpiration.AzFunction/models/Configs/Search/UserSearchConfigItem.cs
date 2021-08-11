using System;
using System.Text.Json.Serialization;

namespace PasswordExpiration.AzFunction.Models.Configs.Search
{
    public class UserSearchConfigItem
    {
        public UserSearchConfigItem() {}

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("domainName")]
        public string DomainName { get; set; }

        [JsonPropertyName("maxPasswordAge")]
        public int MaxPasswordAge { get; set; }

        [JsonPropertyName("emailTemplateId")]
        public string EmailTemplateId { get; set; }
    }
}