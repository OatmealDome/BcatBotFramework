using System;
using System.Collections.Generic;
using Nintendo.Bcat;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BcatBotFramework.Json
{
    public class LanguageMappingDictionaryConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Get the target Dictionary if it exists
            Dictionary<Language, string> textMappings = existingValue != null ? (Dictionary<Language, string>)existingValue : new Dictionary<Language, string>();

            // Get the object
            JObject jObject = JObject.Load(reader);

            // Loop over each propery
            foreach (JProperty jProperty in jObject.Properties())
            {
                // Add this property's key and value to the Dictionary
                textMappings.Add(LanguageExtensions.FromCode(jProperty.Name), (string)jProperty.Value);
            }

            // Return the Dictionary
            return textMappings;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // Get the target Dictionary
            Dictionary<Language, string> textMappings = (Dictionary<Language, string>)value;

            // Start writing an object
            writer.WriteStartObject();

            // Loop over each pair and write it out
            foreach (KeyValuePair<Language, string> pair in textMappings)
            {
                // Write the Bcat code for the language
                writer.WritePropertyName(pair.Key.GetCode());

                // Write the value as-is
                writer.WriteValue(pair.Value);
            }

            // End the object
            writer.WriteEndObject();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Dictionary<Language, string>);
        }

    }
}