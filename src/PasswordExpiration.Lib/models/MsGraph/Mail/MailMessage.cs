using System;
using System.Text.Json.Serialization;

namespace PasswordExpiration.Lib.Models.Graph.Mail
{
    using PasswordExpiration.Lib.Core;

    /// <summary>
    /// An object to represent a mail message to be sent.
    /// </summary>
    public class MailMessage
    {
        public MailMessage() {}

        public MailMessage(Message message, bool saveToSentItems)
        {
            Message = message;
            SaveToSentItems = saveToSentItems;
        }

        [JsonPropertyName("message")]
        public Message Message { get; set; }

        [JsonPropertyName("saveToSentItems")]
        public bool SaveToSentItems { get; set; }

        public string ConvertToJson()
        {
            return JsonConverter.ConvertToJson<MailMessage>(this);
        }
    }
}