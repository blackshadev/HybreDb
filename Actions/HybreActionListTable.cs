using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Actions.Result;
using Newtonsoft.Json;

namespace HybreDb.Actions {
    /// <summary>
    /// Action which lists the contents of a given table
    /// </summary>
    public class HybreActionListTable : IHybreAction {
        [JsonProperty("table")]
        public string TableName;

        public HybreResult Execute(Database db) {
            var res = new HybreUniformResult();
            res.Add(db[TableName], db[TableName].Rows.Select(e => e.Value));
            return res;
        }
    }
}
