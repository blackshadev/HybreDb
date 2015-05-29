using HybreDb.Actions.Result;
using HybreDb.Tables;
using Newtonsoft.Json;

namespace HybreDb.Actions {
    /// <summary>
    ///     Action which deletes a row with given key
    /// </summary>
    public class HybreActionDelete : IHybreAction {
        [JsonProperty("key")]
        public int Key;

        [JsonProperty("table")]
        public string TableName;

        public HybreResult Execute(Database db) {
            Table t = db[TableName];

            t.Remove(Key);

            t.Commit();

            return new HybreUpdateResult {Affected = 1};
        }
    }
}