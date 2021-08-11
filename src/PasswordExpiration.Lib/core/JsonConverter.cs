using System;
using System.Text.Json;

namespace PasswordExpiration.Lib.Core
{
    public static class JsonConverter
    {
        public static T ConvertFromJson<T>(string inputString)
        {
            T convertedItem = JsonSerializer.Deserialize<T>(inputString);

            return convertedItem;
        }

        public static string ConvertToJson<T>(T inputObject)
        {
            JsonSerializerOptions serializerOptions = new JsonSerializerOptions {
                WriteIndented = true
            };
            
            string convertedItem = JsonSerializer.Serialize<T>(inputObject, serializerOptions);

            return convertedItem;
        }
    }
}