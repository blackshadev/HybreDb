using Newtonsoft.Json;

namespace HybreDb.Actions.Result {
    /// <summary>
    ///     A simple wrapper which only holds the affected rows
    /// </summary>
    public class HybreUpdateResult : HybreResult {
        [JsonProperty("affected")]
        public int Affected;
    }
}