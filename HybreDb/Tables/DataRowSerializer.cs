using System;
using Newtonsoft.Json;

namespace HybreDb.Tables {
    public class DataRowSerializer : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return typeof (DataRowSerializer).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var row = value as DataRow;

            writer.WriteStartObject();
            for (int i = 0; i < row.Count; i++) {
                writer.WritePropertyName(row.Table.Columns[i].Name);
                writer.WriteValue(row[i].GetValue());
            }
            writer.WriteEndObject();
        }
    }
}