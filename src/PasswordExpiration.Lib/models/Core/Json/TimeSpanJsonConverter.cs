using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PasswordExpiration.Lib.Models.Core.Json
{
    /// <summary>
    /// A custom converter for handling <see cref="TimeSpan">TimeSpan</see> typed properties when serializing and deserializing objects for JSONs.
    /// </summary>
    public class TimeSpanJsonConverter : JsonConverter<TimeSpan>
    {

        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return TimeSpan.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}