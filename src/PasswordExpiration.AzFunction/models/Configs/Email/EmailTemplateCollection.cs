using System;
using System.Text.Json.Serialization;

namespace PasswordExpiration.AzFunction.Models.Configs.Email
{
    /// <summary>
    /// An object hosting an array of <see cref="EmailTemplateItem">EmailTemplateItem</see> objects.
    /// </summary>
    public class EmailTemplateCollection
    {
        public EmailTemplateCollection() {}

        [JsonPropertyName("templates")]
        public EmailTemplateItem[] Templates { get; set; }
    }
}