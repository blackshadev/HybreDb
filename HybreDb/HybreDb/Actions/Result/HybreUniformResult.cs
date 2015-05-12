using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Relational;
using HybreDb.Tables;
using HybreDb.Tables.Types;
using Newtonsoft.Json;

namespace HybreDb.Actions.Result {

    [JsonConverter(typeof(UniformResultSerialiser))]
    public class HybreUniformResult : HybreResult {

        public Dictionary<Table, Dictionary<Number, DataRow>> TableData;

        public Dictionary<Relation, Dictionary<Number, DataRow>> RelationData;

        public HybreUniformResult() {
            TableData = new Dictionary<Table, Dictionary<Number, DataRow>>();
            RelationData = new Dictionary<Relation, Dictionary<Number, DataRow>>();
        }

        public void Add(Table tab, IEnumerable<DataRow> newrows) {
            Dictionary<Number, DataRow> rows;

            var f = TableData.TryGetValue(tab, out rows);
            if (!f) rows = TableData[tab] = new Dictionary<Number, DataRow>();

            foreach (var r in newrows)
                rows[r.Index] = r;

        }

        public void Add(Relation rel, IEnumerable<DataRow> newrows) {
            Dictionary<Number, DataRow> rows;

            var f = RelationData.TryGetValue(rel, out rows);
            if (!f) rows = RelationData[rel] = new Dictionary<Number, DataRow>();

            foreach (var r in newrows)
                rows[r.Index] = r;
        }
    }

    public class UniformResultSerialiser : JsonConverter {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var res = value as HybreUniformResult;

            writer.WriteStartObject();

            writer.WritePropertyName("elapsedTime");
            writer.WriteValue(res.ElapsedTime);

            writer.WritePropertyName("tableData");
            writer.WriteStartObject();

            foreach (var kvp in res.TableData) {
                writer.WritePropertyName(kvp.Key.Name);
                HybreResult.SerializeTable(writer, kvp.Key, kvp.Value.Values);
            }

            writer.WriteEndObject();

            writer.WritePropertyName("relationData");
            writer.WriteStartObject();
            foreach (var kvp in res.RelationData) {
                writer.WritePropertyName(kvp.Key.Name);
                HybreResult.SerializeRelation(writer, kvp.Key, kvp.Value.Values);
            }
            writer.WriteEndObject();

            writer.WriteEndObject();

        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType) {
            return typeof (HybreUniformResult).IsAssignableFrom(objectType);
        }
    }
}
