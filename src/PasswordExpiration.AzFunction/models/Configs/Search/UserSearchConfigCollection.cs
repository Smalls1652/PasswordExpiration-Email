using System;
using System.Text.Json.Serialization;

namespace PasswordExpiration.AzFunction.Models.Configs.Search
{
    /// <summary>
    /// An object hosting an array of <see cref="UserSearchConfigItem">UserSearchConfigItem</see> objects.
    /// </summary>
    public class UserSearchConfigCollection
    {
        public UserSearchConfigCollection() {}

        [JsonPropertyName("searchConfigs")]
        public UserSearchConfigItem[] SearchConfigs { get; set; }
    }
}