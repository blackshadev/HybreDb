using System.Collections.Generic;
using HybreDb.Actions.Result;
using HybreDb.Relational;
using HybreDb.Tables;
using Newtonsoft.Json;

namespace HybreDb.Actions {
    public class HybreActionAddRelation : IHybreAction {
        [JsonProperty("data")]
        public Dictionary<string, object> Data;

        [JsonProperty("from")]
        public int From;

        [JsonProperty("relation")]
        public string RelationName;

        [JsonProperty("table")]
        public string TableName;

        [JsonProperty("to")]
        public int To;

        public HybreResult Execute(Database db) {
            Table t = db[TableName];

            Relation rel = t.Relations[RelationName];

            rel.Add(From, To, HybreAction.ParseData(rel, Data));

            rel.Commit();

            return new HybreUpdateResult {Affected = 1};
        }
    }
}