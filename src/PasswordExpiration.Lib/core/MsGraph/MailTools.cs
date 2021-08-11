using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace PasswordExpiration.Lib.Core.Graph
{
    using PasswordExpiration.Lib.Models.Core;
    using PasswordExpiration.Lib.Models.Graph.Mail;
    using PasswordExpiration.Lib.Models.Graph.Users;

    /// <summary>
    /// Hosts methods for interacting with Microsoft Graph mail API endpoints.
    /// </summary>
    public class MailTools
    {
        public MailTools(IGraphClient graphClient)
        {
            GraphClient = graphClient;
        }

        public IGraphClient GraphClient { get; set; }

        public static MailMessage CreateMailMessage(User toUser, string subject, string messageBody)
        {
            Message messageObj = new Message(
                subject,
                new MessageBody(messageBody, "HTML"),
                new Recipient[] {
                    new Recipient(new EmailAddress(toUser.UserPrincipalName))
                }
            );

            MailMessage mailMessageObj = new MailMessage(messageObj, false);

            return mailMessageObj;
        }

        public static MailMessage CreateMailMessageWithAttachment(User toUser, string subject, string messageBody, string attachmentPath)
        {

            FileAttachment[] fileAttachments = new FileAttachment[] {
                new FileAttachment(attachmentPath, true)
            };

            Message messageObj = new Message(
                subject,
                new MessageBody(messageBody, "HTML"),
                new Recipient[] {
                    new Recipient(new EmailAddress(toUser.UserPrincipalName))
                },
                fileAttachments
            );

            MailMessage mailMessageObj = new MailMessage(messageObj, false);

            return mailMessageObj;
        }

        /// <summary>
        /// Sends a message to a user.
        /// </summary>
        /// <param name="fromMailAddress">The email address the message should come from.</param>
        /// <param name="toUser">The email address the message should send to.</param>
        /// <param name="subject">The subject of the message.</param>
        /// <param name="messageBody">The HTML contents of the email.</param>
        public string SendMessage(string fromMailAddress, User toUser, string subject, string messageBody)
        {
            Message messageObj = new Message(
                subject,
                new MessageBody(messageBody, "HTML"),
                new Recipient[] {
                    new Recipient(new EmailAddress(toUser.UserPrincipalName))
                }
            );

            MailMessage mailMessageObj = new MailMessage(messageObj, false);

            string mailMessageJson = JsonConverter.ConvertToJson<MailMessage>(mailMessageObj);

            Console.WriteLine(mailMessageJson);

            string apiResponse = GraphClient.SendApiCall($"users/{fromMailAddress}/sendMail", mailMessageJson, HttpMethod.Post);

            return apiResponse;
        }

        /// <summary>
        /// Sends a message to a user with an attachment.
        /// </summary>
        /// <param name="fromMailAddress">The email address the message should come from.</param>
        /// <param name="toUser">The email address the message should send to.</param>
        /// <param name="subject">The subject of the message.</param>
        /// <param name="messageBody">The HTML contents of the email.</param>
        /// <param name="attachmentPath">The file path of the attachment.</param>
        public void SendMessageWithAttachment(string fromMailAddress, User toUser, string subject, string messageBody, string attachmentPath)
        {

            FileAttachment[] fileAttachments = new FileAttachment[] {
                new FileAttachment(attachmentPath, true)
            };

            Message messageObj = new Message(
                subject,
                new MessageBody(messageBody, "HTML"),
                new Recipient[] {
                    new Recipient(new EmailAddress(toUser.UserPrincipalName))
                },
                fileAttachments
            );

            MailMessage mailMessageObj = new MailMessage(messageObj, false);

            string mailMessageJson = JsonConverter.ConvertToJson<MailMessage>(mailMessageObj);

            string apiResponse = GraphClient.SendApiCall($"users/{fromMailAddress}/sendMail", mailMessageJson, HttpMethod.Post);
        }
    }
}