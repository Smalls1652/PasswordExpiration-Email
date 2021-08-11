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
    public class MailTools
    {
        public MailTools(GraphClient graphClient)
        {
            GraphClient = graphClient;
        }

        public GraphClient GraphClient { get; set; }

        public MailMessage CreateMailMessage(string fromMailAddress, User toUser, string subject, string messageBody)
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

        public MailMessage CreateMailMessageWithAttachment(string fromMailAddress, User toUser, string subject, string messageBody, string attachmentPath)
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