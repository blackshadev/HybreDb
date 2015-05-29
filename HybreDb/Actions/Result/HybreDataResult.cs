using System;
using System.Collections.Generic;
using System.Linq;
using HybreDb.Tables;
using Newtonsoft.Json;

namespace HybreDb.Actions.Result {

    /// <summary>
    /// Wraps a dataset of a table as HybreResult
    /// </summary>
    [JsonConverter(typeof(HybreDataResultJsonSerializer))]
    public class HybreDataResult : HybreResult {
        public Table Table;
        public IEnumerable<DataRow> Rows;

        public HybreDataResult(Table t, IEnumerable<DataRow> rows) {
            Rows = rows;
            Table = t;
        }
    }

    /// <summary>
    /// Serializes the Dataset by first adding the columns (name and type) 
    /// and than an object of rows where the key is the primary key of that row 
    /// and the value the array of data items.
    /// </summary>
    class HybreDataResultJsonSerializer : JsonConverter {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var res = value as HybreDataResult;

            writer.WriteStartObject();
            writer.WritePropertyName("elapsedTime");
            writer.WriteValue(res.ElapsedTime);

            writer.WritePropertyName("tableName");
            writer.WriteValue(res.Table.Name);

            writer.WritePropertyName("relations");
            writer.WriteStartObject();

            foreach (var rel in res.Table.Relations) {
                writer.WritePropertyName(rel.Name);
                HybreResult.SerializeRelation(writer, rel, new DataRow[0]);
            }

            writer.WriteEndObject();


            writer.WritePropertyName("columns");
            HybreResult.SerializeColumns(writer, res.Table.Columns.Where(e => !e.Hidden));

            writer.WritePropertyName("rows");
            writer.WriteStartObject();

            foreach (var r in res.Rows) {
                writer.WritePropertyName(r.Index.ToString());

                writer.WriteStartArray();

                for (var i = 0; i < res.Table.Columns.Length; i++) {
                    if (res.Table.Columns[i].Hidden) continue;
                    writer.WriteValue(r[i].GetValue());
                }

                writer.WriteEndArray();

            }

            writer.WriteEndObject();

        }

        /// <summary>
        /// Results are not ment to be interpret from json
        /// </summary>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType) {
            return typeof (HybreDataResultJsonSerializer).IsAssignableFrom(objectType);
        }
    }

}
