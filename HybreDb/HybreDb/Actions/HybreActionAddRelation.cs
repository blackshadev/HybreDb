using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Actions.Result;
using HybreDb.Tables.Types;
using Newtonsoft.Json;

namespace HybreDb.Actions {
    public class HybreActionAddRelation : IHybreAction {

        [JsonProperty("table")]
        public string TableName;

        [JsonProperty("relation")]
        public string RelationName;

        [JsonProperty("from")]
        public int From;

        [JsonProperty("to")]
        public int To;

        [JsonProperty("data")]
        public Dictionary<string, object> Data;

        public HybreResult Execute(Database db) {
            var t = db[TableName];

            var rel = t.Relations[RelationName];

            rel.Add(From, To, HybreAction.ParseData(rel.Table, Data) );

            rel.Commit();

            return new HybreUpdateResult{Affected = 1};
        }
    }
}
