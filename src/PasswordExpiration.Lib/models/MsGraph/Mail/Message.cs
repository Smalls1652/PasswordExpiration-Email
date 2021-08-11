using System;
using System.Text.Json.Serialization;

namespace PasswordExpiration.Lib.Models.Graph.Mail
{
    public class Message
    {
        public Message() {}

        public Message(string subject, MessageBody body, Recipient[] toRecipient)
        {
            Subject = subject;
            Body = body;
            ToRecipient = toRecipient;
            HasAttachments = false;
        }

        public Message(string subject, MessageBody body, Recipient[] toRecipient, FileAttachment[] attachments)
        {
            Subject = subject;
            Body = body;
            ToRecipient = toRecipient;
            Attachments = attachments;
            HasAttachments = true;
        }

        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("body")]
        public MessageBody Body { get; set; }

        [JsonPropertyName("toRecipients")]
        public Recipient[] ToRecipient { get; set; }

        [JsonPropertyName("ccRecipients")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Recipient[] CcRecipient { get; set; }

        [JsonPropertyName("attachments")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public FileAttachment[] Attachments { get; set; }

        [JsonPropertyName("hasAttachments")]
        public bool HasAttachments { get; set; }
    }
}