using System;
using Newtonsoft.Json;

namespace BcatBotFramework.Json
{
    public class StringZeroByteTrimmerConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.ReadAsString();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((string)value).Trim('\0'));
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

    }
}