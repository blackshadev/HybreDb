using HybreDb.Actions.Result;
using Newtonsoft.Json;

namespace HybreDb.Actions {
    public class HybreActionDropRelation : IHybreAction {
        [JsonProperty("table")]
        public string TableName;

        [JsonProperty("relation")]
        public string RelationName;



        public HybreResult Execute(Database db) {
            db[TableName].Relations.DropRelation(RelationName);

            return new HybreUpdateResult();
        }
    }
}