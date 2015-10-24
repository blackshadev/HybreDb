using System.Collections.Generic;
using HybreDb.Actions.Result;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Relational;
using HybreDb.Tables;
using Newtonsoft.Json;
using System;

namespace HybreDb.Actions {
    public class HybreActionCreateRelation : IHybreAction {
        [JsonProperty("attributes")]
        public DataColumn[] Attributes;

        [JsonProperty("data")]
        public Dictionary<string, object>[] Data;

        [JsonProperty("relationType")]
        public RelationType RelationType;

        [JsonProperty("destinationTable")]
        public string Destination;

        [JsonProperty("relation")]
        public string RelationName;

        [JsonProperty("sourceTable")]
        public string Source;

        public HybreResult Execute(Database db) {
            Relation rel = db.NewRelation(RelationName, Source, Destination, Attributes);

            var data = new IDataType[Data.Length][];

            for (int i = 0; i < data.Length; i++)
                data[i] = HybreAction.ParseDataWithRel(rel, Data[i]);

            db[Source].Write();

            rel.Table.BulkInsert(data);

            rel.Commit();

            return new HybreUpdateResult {Affected = 0};
        }
    }
}