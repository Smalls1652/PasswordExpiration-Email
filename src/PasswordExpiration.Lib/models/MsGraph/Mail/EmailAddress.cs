using System;
using System.Text.Json.Serialization;

namespace PasswordExpiration.Lib.Models.Graph.Mail
{
    /// <summary>
    /// An object representing the email address of a user to use when generating a mail message.
    /// </summary>
    public class EmailAddress
    {
        public EmailAddress() {}

        public EmailAddress(string address)
        {
            Address = address;
        }

        public EmailAddress(string address, string name)
        {
            Address = address;
            Name = name;
        }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}