using System;
using System.IO;
using System.Text.Json.Serialization;

namespace PasswordExpiration.AzFunction.Models.Configs
{
    /// <summary>
    /// Config settings for an email template to utilize when sending to users.
    /// </summary>
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

        [JsonPropertyName("isCustom")]
        public bool IsCustom { get; set; }
    }
}