using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HybreDb.Actions {
    public class HybreActionGet : IHybreAction {
        [JsonProperty("table")] 
        public string TableName;

        [JsonProperty("key")]
        public int Key;

        public object Execute(Database db) {
            return new HybreDataResult(db[TableName], new [] { db[TableName][Key] } );
        }
    }
}
