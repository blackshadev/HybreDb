using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Tables;
using Newtonsoft.Json;

namespace HybreDb.Actions {

    [JsonConverter(typeof(HybreResultJsonSerializer))]
    public class HybreDataResult {
        public Table Table;
        public IEnumerable<DataRow> Rows;

        public HybreDataResult(Table t, IEnumerable<DataRow> rows) {
            Rows = rows;
            Table = t;
        }
    }

    public class HybreResultJsonSerializer : JsonConverter {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var res = value as HybreDataResult;

            writer.WriteStartObject();
            writer.WritePropertyName("columns");

            writer.WriteStartArray();

            foreach (var c in res.Table.Columns) {
                writer.WriteStartObject();
                writer.WritePropertyName("name");
                writer.WriteValue(c.Name);
                writer.WritePropertyName("type");
                writer.WriteValue(c.DataType.ToString());
                writer.WriteEndObject();
            }

            writer.WriteEndArray();

            writer.WritePropertyName("rows");
            writer.WriteStartArray();

            foreach (var r in res.Rows) {
                writer.WriteStartArray();

                foreach(var dat in r)
                    writer.WriteValue(dat.GetValue());

                writer.WriteEndArray();
            }

            writer.WriteEndArray();

        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType) {
            return typeof (HybreResultJsonSerializer).IsAssignableFrom(objectType);
        }
    }

}
