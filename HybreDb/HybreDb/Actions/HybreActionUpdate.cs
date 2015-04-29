using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Actions.Result;
using HybreDb.Tables;
using Newtonsoft.Json;

namespace HybreDb.Actions {
    public class HybreActionUpdate : IHybreAction {

        [JsonProperty("table")]
        public string TableName;

        [JsonProperty("key")]
        public int Key;

        [JsonProperty("data")]
        public Dictionary<string, object> Data;

        public HybreResult Execute(Database db) {
            var t = db[TableName];

            foreach (var d in Data) {
                var data = t.Columns[d.Key].DataType.CreateType(d.Value);
                t.Update(Key, d.Key, data);
            }

            t.Commit();

            return new HybreUpdateResult { Affected = 1 };
        }
    }
}
