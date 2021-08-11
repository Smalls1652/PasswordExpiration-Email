using System;
using System.Text.Json.Serialization;

namespace PasswordExpiration.Lib.Models.Graph.Mail
{
    public class FileAttachment
    {
        public FileAttachment() {}

        public FileAttachment(string name, string contentBytes, bool isInline)
        {
            Name = name;
            ContentBytes = contentBytes;
            IsInline = isInline;
        }

        [JsonPropertyName("@odata.type")]
        public string DataType = "#microsoft.graph.fileAttachment";

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("contentBytes")]
        public string ContentBytes { get; set; }

        [JsonPropertyName("isInline")]
        public bool IsInline { get; set; }
    }
}