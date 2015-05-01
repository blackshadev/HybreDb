﻿using System;
using System.Collections.Generic;
using System.Linq;
using HybreDb.Tables;
using Newtonsoft.Json;

namespace HybreDb.Actions.Result {

    [JsonConverter(typeof(HybreDataResultJsonSerializer))]
    public class HybreDataResult : HybreResult {
        public Table Table;
        public IEnumerable<DataRow> Rows;

        public HybreDataResult(Table t, IEnumerable<DataRow> rows) {
            Rows = rows;
            Table = t;
        }
    }

    class HybreDataResultJsonSerializer : JsonConverter {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var res = value as HybreDataResult;

            writer.WriteStartObject();
            writer.WritePropertyName("elapsedTime");
            writer.WriteValue(res.ElapsedTime);

            writer.WritePropertyName("columns");

            writer.WriteStartArray();

            foreach (var c in res.Table.Columns.Where(c => !c.Hidden)) {
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
                writer.WriteStartObject();

                writer.WritePropertyName(r.Index.ToString());

                writer.WriteStartArray();

                for (var i = 0; i < res.Table.Columns.Length; i++) {
                    if (res.Table.Columns[i].Hidden) continue;
                    writer.WriteValue(r[i].GetValue());
                }

                writer.WriteEndArray();

                writer.WriteEndObject();
            }

            writer.WriteEndArray();

        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType) {
            return typeof (HybreDataResultJsonSerializer).IsAssignableFrom(objectType);
        }
    }

}
