using System;
using System.Text.Json.Serialization;

namespace PasswordExpiration.AzFunction.Models.Configs.Search
{
    public class UserSearchConfigCollection
    {
        public UserSearchConfigCollection() {}

        [JsonPropertyName("searchConfigs")]
        public UserSearchConfigItem[] SearchConfigs { get; set; }
    }
}