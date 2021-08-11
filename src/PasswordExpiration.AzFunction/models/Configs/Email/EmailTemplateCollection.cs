using System;
using System.Text.Json.Serialization;

namespace PasswordExpiration.AzFunction.Models.Configs.Email
{
    public class EmailTemplateCollection
    {
        public EmailTemplateCollection() {}

        [JsonPropertyName("templates")]
        public EmailTemplateItem[] Templates { get; set; }
    }
}