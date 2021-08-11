using System;
using System.IO;
using System.Text.Json.Serialization;

namespace PasswordExpiration.Lib.Models.Graph.Mail
{
    using PasswordExpiration.Lib.Core;

    /// <summary>
    /// An object representing a file attachment to use when generating a mail message.
    /// </summary>
    public class FileAttachment
    {
        public FileAttachment() {}

        public FileAttachment(string filePath, bool isInline)
        {
            string resolvedFilePath = Path.GetFullPath(filePath);
            FileInfo fileItem = new FileInfo(resolvedFilePath);

            DataType = "#microsoft.graph.fileAttachment";
            Name = fileItem.Name;
            ContentBytes = System.Convert.ToBase64String(File.ReadAllBytes(fileItem.FullName));
            IsInline = isInline;
        }

        [JsonPropertyName("@odata.type")]
        public string DataType { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("contentBytes")]
        public string ContentBytes { get; set; }

        [JsonPropertyName("isInline")]
        public bool IsInline { get; set; }

        public string ConvertToJson()
        {
            return JsonConverter.ConvertToJson<FileAttachment>(this);
        }
    }
}