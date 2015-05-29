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
    public class HybreActionCreateRelation : IHybreAction {

        [JsonProperty("sourceTable")]
        public string Source;

        [JsonProperty("destinationTable")]
        public string Destination;

        [JsonProperty("relation")]
        public string RelationName;

        [JsonProperty("attributes")]
        public DataColumn[] Attributes;

        [JsonProperty("data")]
        public Dictionary<string, object>[] Data; 

        public HybreResult Execute(Database db) {
            var rel = db.NewRelation(RelationName, Source, Destination, Attributes);

            var data = new IDataType[Data.Length][];

            for (var i = 0; i < data.Length; i++)
                data[i] = HybreAction.ParseDataWithRel(rel, Data[i]);

            db[Source].Write();

            rel.Table.BulkInsert(data);

            rel.Commit();

            return new HybreUpdateResult{ Affected = 0 };
        }
    }
}
