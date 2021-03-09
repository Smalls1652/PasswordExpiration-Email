using System.Text.Json.Serialization;

namespace graph_email.Models.Mail
{
    public class MessageBody
    {
        [JsonPropertyName("contentType")]
        public string ContentType { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}