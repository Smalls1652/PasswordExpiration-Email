using System.Text.Json.Serialization;

namespace graph_email.Models.Mail
{
    public class AddressOptions
    {
        [JsonPropertyName("address")]
        public string Address { get; set; }
    }
}