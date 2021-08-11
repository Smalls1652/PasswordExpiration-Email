using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PasswordExpiration.Lib.Models.Graph.Users
{
    /// <summary>
    /// An object that represents a collection of Microsoft Graph <see cref="User">User</see> objects.
    /// </summary>
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