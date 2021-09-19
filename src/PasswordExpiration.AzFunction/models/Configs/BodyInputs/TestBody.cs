using System.Text.Json.Serialization;

namespace PasswordExpiration.AzFunction.Models.BodyInputs
{
    public class TestBody : ITestBody
    {
        public TestBody() {}

        [JsonPropertyName("isTest")]
        public bool IsTest { get; set; }
    }
}