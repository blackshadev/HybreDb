using HybreDb.Actions.Result;
using Newtonsoft.Json;

namespace HybreDb.Actions {
    public class HybreActionDropTable : IHybreAction {
        [JsonProperty("table")]
        public string TableName;

        public HybreResult Execute(Database db) {
            db.DropTable(TableName);

            return new HybreUpdateResult();
        }
    }
}