using System.Collections.Generic;
using HybreDb.Actions.Result;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Tables;
using Newtonsoft.Json;

namespace HybreDb.Actions {
    /// <summary>
    ///     Action which updates the data of a given record
    /// </summary>
    public class HybreActionUpdate : IHybreAction {
        [JsonProperty("data")]
        public Dictionary<string, object> Data;

        [JsonProperty("key")]
        public int Key;

        [JsonProperty("table")]
        public string TableName;

        public HybreResult Execute(Database db) {
            Table t = db[TableName];

            foreach (var d in Data) {
                IDataType data = t.Columns[d.Key].DataType.CreateType(d.Value);
                t.Update(Key, d.Key, data);
            }

            t.Commit();

            return new HybreUpdateResult {Affected = 1};
        }
    }
}