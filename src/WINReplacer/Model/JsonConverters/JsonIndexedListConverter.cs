using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static WINReplacer.IndexedList;

namespace WINReplacer.JsonConverters
{
    public class JsonIndexedListConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IndexedList);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var indexedArray = (IndexedList)value;

            writer.WriteStartObject();
            writer.WritePropertyName("Apps");
            serializer.Serialize(writer, indexedArray.GetIndexedArray());
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            IndexedList indexedList = null;
            while (reader.Read())
            {
                if (reader.TokenType != JsonToken.PropertyName)
                    break;

                var propertyName = (string)reader.Value;
                if (!reader.Read())
                    continue;

                if (propertyName == "Apps")
                {
                    indexedList = new IndexedList(serializer.Deserialize<HashSet<Index>>(reader));
                }
            }

            return indexedList == null? new IndexedList() : indexedList;
        }
    }
}
