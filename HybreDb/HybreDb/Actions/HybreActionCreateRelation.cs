using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Actions.Result;
using HybreDb.Tables;
using Newtonsoft.Json;

namespace HybreDb.Actions {
    public class HybreActionCreateRelation : IHybreAction {

        [JsonProperty("sourceTable")]
        public string Source;

        [JsonProperty("destinationTable")]
        public string Destination;

        [JsonProperty("relation")]
        public string RelationName;

        [JsonProperty("attributes")]
        public DataColumn[] Attributes;

        public HybreResult Execute(Database db) {
            db.NewRelation(RelationName, Source, Destination, Attributes);

            return new HybreUpdateResult{ Affected = 0 };
        }
    }
}
