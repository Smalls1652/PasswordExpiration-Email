using System;
using System.Text.Json.Serialization;

namespace PasswordExpiration.Lib.Models.Graph.Mail
{
    /// <summary>
    /// An object to represent the email address of a user when generating a mail message.
    /// </summary>
    /// <list type="bullet">
    ///     <listheader>
    ///         <term>Recipient types</term>
    ///         <description>Can be used with</description>
    ///     </listheader>
    ///     <item>
    ///         <description>To</description>
    ///     </item>
    ///     <item>
    ///         <description>Cc</description>
    ///     </item>
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