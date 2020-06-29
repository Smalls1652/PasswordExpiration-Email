using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace graph_email
{
    namespace Mail
    {
        public class MailMessage
        {
            public Classes.MessageOptions message { get; set; }
            public bool saveToSentItems { get; set; }
        }

        namespace Classes
        {
            public class Conversion
            {
                public string ToJson (MailMessage msg)
                {
                    string jsonString = JsonConvert.SerializeObject(msg);

                    return jsonString;
                }
            }

            public class MessageOptions
            {
                public string subject { get; set; }

                public MessageBody body { get; set; }

                public List<EmailAddress> toRecipients { get; set; }
                public List<EmailAddress> ccRecipients { get; set; }

                public List<FileAttachment> attachments { get; set; }
                public bool hasAttachments { get; set; }
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

            public class FileAttachment
            {
                [JsonProperty("@odata.type")]
                public string OdataType = "#microsoft.graph.fileAttachment";
                public string name { get; set; }
                public string contentBytes { get; set; }
                public bool isInline { get; set; }
            }
        }
    }
}