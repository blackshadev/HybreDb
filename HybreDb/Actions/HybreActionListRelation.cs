using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Actions.Result;
using HybreDb.Tables.Types;
using Newtonsoft.Json;

namespace HybreDb.Actions {
    
    /// <summary>
    /// Action which lists a given relation
    /// </summary>
    public class HybreActionListRelation : IHybreAction {

        [JsonProperty("table")]
        public string TableName;

        [JsonProperty("relation")]
        public string Relation;

        public HybreResult Execute(Database db) {
            var t = db[TableName];
            var r = t.Relations[Relation];

            var srcNumbers = new Numbers();
            var dstNumbers = new Numbers();
            var relrows = r.Table.Rows.Select(e => e.Value).ToArray();

            foreach (var _r in relrows) {
                srcNumbers.Add((Number)_r[1]);
                dstNumbers.Add((Number)_r[2]);
            }

            var srcRows = r.Source.GetData(srcNumbers);
            var dstRows = r.Destination.GetData(dstNumbers);

            var result = new HybreUniformResult();

            result.Add(r.Source, srcRows);
            result.Add(r.Destination, dstRows);

            result.Add(r, relrows);
            
            return result;
        }
    }
}
