using System.Collections.Generic;
using HybreDb.Actions.Result;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Tables;
using Newtonsoft.Json;

namespace HybreDb.Actions {
    /// <summary>
    ///     Action which inserts given data into given table
    /// </summary>
    public class HybreActionInsert : IHybreAction {
        [JsonProperty("data")]
        public Dictionary<string, object> Data;

        [JsonProperty("table")]
        public string TableName;

        public HybreResult Execute(Database db) {
            Table t = db[TableName];

            IDataType[] row = HybreAction.ParseData(t, Data);

            t.Insert(row);
            t.Commit();

            return new HybreUpdateResult {Affected = 1};
        }
    }
}