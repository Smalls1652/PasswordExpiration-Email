using System;
using System.Text.Json.Serialization;

namespace PasswordExpiration.Lib.Models.Graph.Mail
{
    public class MessageBody
    {

        public MessageBody() {}

        public MessageBody(string msgContent, string msgContentType)
        {
            Content = msgContent;
            ContentType = msgContentType;
        }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("contentType")]
        public string ContentType { get; set; }
    }
}