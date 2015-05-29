using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HybreDb.Tables {
    public class DataRowSerializer : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return typeof (DataRowSerializer).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var row = value as DataRow;

            writer.WriteStartObject();
            for (var i = 0; i < row.Count; i++) {
                writer.WritePropertyName(row.Table.Columns[i].Name);
                writer.WriteValue(row[i].GetValue());
            }
            writer.WriteEndObject();

        }
    }
}
