﻿using System.Collections.Generic;
using System.Linq;
using HybreDb.Relational;
using HybreDb.Tables;
using Newtonsoft.Json;

namespace HybreDb.Actions.Result {
    public class HybreResult {
        [JsonProperty("elapsedTime")]
        public double ElapsedTime;

        public static void SerializeColumns(JsonWriter writer, IEnumerable<DataColumn> cols) {
            writer.WriteStartObject();

            foreach (DataColumn c in cols) {
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

        protected static void SerializeDataRows(JsonWriter writer, Table tab, IEnumerable<DataRow> rows) {
            writer.WriteStartObject();

            foreach (DataRow r in rows) {
                writer.WritePropertyName(r.Index.ToString());

                writer.WriteStartObject();

                for (int i = 0; i < tab.Columns.Length; i++) {
                    if (tab.Columns[i].Hidden) continue;
                    writer.WritePropertyName(tab.Columns[i].Name);
                    writer.WriteValue(r[i].GetValue());
                }

                writer.WriteEndObject();
            }

            writer.WriteEndObject();
        }

        public static void SerializeTable(JsonWriter writer, Table sourceTable, IEnumerable<DataRow> rows) {
            writer.WriteStartObject();

            writer.WritePropertyName("tableName");
            writer.WriteValue(sourceTable.Name);

            writer.WritePropertyName("columns");
            SerializeColumns(writer, sourceTable.Columns.Where(e => !e.Hidden));

            writer.WritePropertyName("rows");
            SerializeDataRows(writer, sourceTable, rows);

            writer.WriteEndObject();
        }

        public static void SerializeRelation(JsonWriter writer, Relation rel, IEnumerable<DataRow> rows) {
            writer.WriteStartObject();

            writer.WritePropertyName("relationType");
            writer.WriteValue(rel.RelationType);

            writer.WritePropertyName("sourceTable");
            writer.WriteValue(rel.Source.Name);

            writer.WritePropertyName("destinationTable");
            writer.WriteValue(rel.Destination.Name);

            writer.WritePropertyName("attributes");
            SerializeColumns(writer, rel.Table.Columns.Where(e => !e.Hidden));

            writer.WritePropertyName("rows");
            SerializeDataRows(writer, rel.Table, rows);
            
            writer.WriteEndObject();
        }

        public static void SerializeRelations(JsonWriter writer, IEnumerable<Relation> rels) {
            writer.WriteStartObject();

            foreach (Relation rel in rels) {
                writer.WritePropertyName(rel.Name);
                writer.WriteStartObject();

                writer.WritePropertyName("relationType");
                writer.WriteValue(rel.RelationType);

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