using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;

namespace WINReplacer.JsonConverters
{
    class JsonFixedSizeQueueConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(FixedSizedQueue<App>);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var fixedSizedQueue = (FixedSizedQueue<App>)value;

            writer.WriteStartObject();
            writer.WritePropertyName("Apps");
            serializer.Serialize(writer, fixedSizedQueue.GetQueue());
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            FixedSizedQueue<App> queue = null;
            while (reader.Read())
            {
                if (reader.TokenType != JsonToken.PropertyName)
                    break;

                var propertyName = (string)reader.Value;
                if (!reader.Read())
                    continue;

                if (propertyName == "Apps")
                {
                    queue = new FixedSizedQueue<App>(serializer.Deserialize<ConcurrentQueue<App>>(reader));
                }
            }

            return queue == null? new FixedSizedQueue<App>() : queue;
        }
    }
}
