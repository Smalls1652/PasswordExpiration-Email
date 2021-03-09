using System.Text.Json.Serialization;

namespace graph_email.Models.Mail
{
    public class FileAttachment
    {
        [JsonPropertyName("@odata.type")]
        public string OdataType = "#microsoft.graph.fileAttachment";

        [JsonPropertyName("name")]
        public string FileName { get; set; }

        [JsonPropertyName("contentBytes")]
        public string ContentBytes { get; set; }

        [JsonPropertyName("isInline")]
        public bool IsInline { get; set; }
    }
}