using System.Text.Json;
using System.Text.Json.Serialization;

namespace graph_email.Models.Mail
{
    public class MailMessage
    {
        [JsonPropertyName("message")]
        public MessageOptions Message { get; set; }

        [JsonPropertyName("saveToSentItems")]
        public bool SaveToSentItems { get; set; }

        public string ToJson()
        {
            string jsonString = JsonSerializer.Serialize<MailMessage>(this);

            return jsonString;
        }
    }
}