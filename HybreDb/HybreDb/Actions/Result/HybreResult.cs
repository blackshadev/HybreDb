using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Relational;
using HybreDb.Tables;
using Newtonsoft.Json;

namespace HybreDb.Actions.Result {
    public class HybreResult {
        [JsonProperty("elapsedTime")]
        public long ElapsedTime;

        public static void SerializeColumns(JsonWriter writer, IEnumerable<DataColumn> cols) {
            writer.WriteStartObject();

            foreach (var c in cols) {
                writer.WritePropertyName(c.Name);
                writer.WriteStartObject();

                writer.WritePropertyName("name");
                writer.WriteValue(c.Name);

                writer.WritePropertyName("type");
                writer.WriteValue(c.DataType);

                writer.WritePropertyName("indexed");
                writer.WriteValue(c.HasIndex);

                writer.WriteEndObject();
            }

            writer.WriteEndObject();
        }

        public static void SerializeRelations(JsonWriter writer, IEnumerable<Relation> rels) {
            writer.WriteStartObject();

            foreach (var rel in rels) {
                writer.WritePropertyName(rel.Name);
                writer.WriteStartObject();

                writer.WritePropertyName("name");
                writer.WriteValue(rel.Name);

                writer.WritePropertyName("sourceTable");
                writer.WriteValue(rel.Source.Name);

                writer.WritePropertyName("destinationTable");
                writer.WriteValue(rel.Destination.Name);

                writer.WritePropertyName("attributes");
                SerializeColumns(writer, rel.Table.Columns.Where(e => !e.Hidden));

                writer.WriteEndObject();
            }

            writer.WriteEndObject();
        }
    }
}
