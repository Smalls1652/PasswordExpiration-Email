using System.Text.Json.Serialization;

namespace graph_email.Models.Mail
{
    public class EmailAddress
    {
        [JsonPropertyName("emailAddress")]
        public AddressOptions MailAddress { get; set; }
    }
}