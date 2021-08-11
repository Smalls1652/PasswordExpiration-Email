using System;
using System.Text.Json;

namespace PasswordExpiration.Lib.Core
{
    using Models.Core.Json;
    public static class JsonConverter
    {
        public static T ConvertFromJson<T>(string inputString)
        {
            JsonSerializerOptions deserializationOptions = new JsonSerializerOptions();
            deserializationOptions.Converters.Add(new TimeSpanJsonConverter());
            
            T convertedItem = JsonSerializer.Deserialize<T>(inputString, deserializationOptions);

            return convertedItem;
        }

        public static string ConvertToJson<T>(T inputObject)
        {
            JsonSerializerOptions serializerOptions = new JsonSerializerOptions {
                WriteIndented = true
            };
            serializerOptions.Converters.Add(new TimeSpanJsonConverter());
            
            string convertedItem = JsonSerializer.Serialize<T>(inputObject, serializerOptions);

            return convertedItem;
        }
    }
}