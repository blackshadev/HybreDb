using HybreDb.Actions.Result;
using Newtonsoft.Json;

namespace HybreDb.Actions {
    /// <summary>
    ///     Action which gets a row beloning to given key
    /// </summary>
    public class HybreActionGet : IHybreAction {
        [JsonProperty("key")]
        public int Key;

        [JsonProperty("table")]
        public string TableName;

        public HybreResult Execute(Database db) {
            return new HybreDataResult(db[TableName], new[] {db[TableName][Key]});
        }
    }
}