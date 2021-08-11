using System;
using System.Text.Json.Serialization;

namespace PasswordExpiration.AzFunction.Models.Configs.Search
{
    /// <summary>
    /// Config settings for how to search for users, what password policy should be applied, and the email template to use.
    /// </summary>
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