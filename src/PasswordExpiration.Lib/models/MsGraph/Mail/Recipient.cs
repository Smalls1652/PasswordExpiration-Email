using System;
using System.Text.Json.Serialization;

namespace PasswordExpiration.Lib.Models.Graph.Mail
{
    public class Recipient
    {
        public Recipient() {}

        public Recipient(EmailAddress emailAddress)
        {
            EmailAddress = emailAddress;
        }

        [JsonPropertyName("emailAddress")]
        public EmailAddress EmailAddress { get; set; }
    }
}