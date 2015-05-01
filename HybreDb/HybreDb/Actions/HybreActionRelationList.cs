using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Actions.Result;
using Newtonsoft.Json;

namespace HybreDb.Actions {
    public class HybreActionRelationList :IHybreAction {

        [JsonProperty("table")]
        public string TableName;

        [JsonProperty("relation")]
        public string Relation;

        public HybreResult Execute(Database db) {
            var t = db[TableName];
            var r = t.Relations[Relation];

            return new HybreRelationDataResult {Relation = r, Data = r.Table.Rows.Select(e => e.Value)};
        }
    }
}
