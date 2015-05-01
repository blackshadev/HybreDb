using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Actions.Result;
using Newtonsoft.Json;

namespace HybreDb.Actions {
    /// <summary>
    /// Action which deletes a row with given key
    /// </summary>
    public class HybreActionDelete : IHybreAction {

        [JsonProperty("table")]
        public string TableName;

        [JsonProperty("key")]
        public int Key;

        public HybreResult Execute(Database db) {
            var t = db[TableName];

            t.Remove(Key);
            
            t.Commit();

            return new HybreUpdateResult { Affected = 1 };
        }
    }
}
