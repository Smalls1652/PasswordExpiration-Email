using System;
using System.Text.Json.Serialization;

namespace PasswordExpiration.AzFunction.Models.Configs.Email
{
    public class EmailTemplateItem
    {
        public EmailTemplateItem() {}

        [JsonPropertyName("templateId")]
        public string TemplateId { get; set; }

        [JsonPropertyName("templateName")]
        public string TemplateName { get; set; }

        [JsonPropertyName("templateHtmlFileName")]
        public string TemplateHtmlFileName { get; set; }

        [JsonPropertyName("includedAttachments")]
        public string[] IncludedAttachments { get; set; }
    }
}