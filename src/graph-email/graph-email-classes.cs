using System;
using System.Collections.Generic;

namespace graph_email
{
    namespace Mail
    {
        public class MailMessage
        {
            public Classes.MessageOptions message { get; set; }
            public Boolean saveToSentItems { get; set; }
        }

        namespace Classes
        {

            public class MessageOptions
            {
                public string subject { get; set; }

                public MessageBody body { get; set; }

                public List<EmailAddress> toRecipients { get; set; }
                public List<EmailAddress> ccRecipients { get; set; }
            }

            public class MessageBody
            {
                public string contentType { get; set; }
                public string content { get; set; }
            }

            public class EmailAddress
            {
                public AddressOptions emailAddress { get; set; }
            }

            public class AddressOptions
            {
                public string address { get; set; }
            }
        }
    }
}