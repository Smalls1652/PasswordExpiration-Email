using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace graph_email.Models.Mail
{
    public class MessageOptions
    {
        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("body")]
        public MessageBody Body { get; set; }

        [JsonPropertyName("toRecipients")]
        public List<EmailAddress> ToRecipients { get; set; }

        [JsonPropertyName("ccRecipients")]
        public List<EmailAddress> CcRecipients { get; set; }

        [JsonPropertyName("attachments")]
        public List<FileAttachment> Attachments { get; set; }

        [JsonPropertyName("hasAttachments")]
        public bool HasAttachments { get; set; }
    }
}