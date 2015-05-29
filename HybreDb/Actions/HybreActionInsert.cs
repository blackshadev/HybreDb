using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Actions.Result;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Tables;
using Newtonsoft.Json;

namespace HybreDb.Actions {
    /// <summary>
    /// Action which inserts given data into given table
    /// </summary>
    public class HybreActionInsert : IHybreAction {

        [JsonProperty("table")]
        public string TableName;

        [JsonProperty("data")]
        public Dictionary<string, object> Data;

        public HybreResult Execute(Database db) {
            var t = db[TableName];

            var row = HybreAction.ParseData(t, Data);

            t.Insert(row);
            t.Commit();
            
            return new HybreUpdateResult { Affected = 1 };
        }
    }
}
