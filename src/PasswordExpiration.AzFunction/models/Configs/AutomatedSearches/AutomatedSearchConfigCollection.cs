using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PasswordExpiration.AzFunction.Models.Configs
{
    public class AutomatedSearchConfigCollection
    {
        public AutomatedSearchConfigCollection() {}

        [JsonPropertyName("automatedSearchConfigs")]
        public List<string> AutomatedSearchConfigs { get; set; }
    }
}