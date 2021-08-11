using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PasswordExpiration.Lib.Models.Graph.Users
{
    public class UserCollection : IResponseCollection<User>
    {
        public UserCollection() {}

        [JsonPropertyName("@odata.context")]
        public string OdataContext { get; set; }

        [JsonPropertyName("@odata.nextLink")]
        public string OdataNextLink { get; set; }

        [JsonPropertyName("value")]
        public List<User> Value { get; set; }
    }
}