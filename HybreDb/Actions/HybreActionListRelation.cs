using System.Collections.Generic;
using System.Linq;
using HybreDb.Actions.Result;
using HybreDb.Relational;
using HybreDb.Tables;
using HybreDb.Tables.Types;
using Newtonsoft.Json;

namespace HybreDb.Actions {
    /// <summary>
    ///     Action which lists a given relation
    /// </summary>
    public class HybreActionListRelation : IHybreAction {
        [JsonProperty("relation")]
        public string Relation;

        [JsonProperty("table")]
        public string TableName;

        public HybreResult Execute(Database db) {
            Table t = db[TableName];
            Relation r = t.Relations[Relation];

            var srcNumbers = new Numbers();
            var dstNumbers = new Numbers();
            DataRow[] relrows = r.Table.Rows.Select(e => e.Value).ToArray();

            foreach (DataRow _r in relrows) {
                srcNumbers.Add((Number) _r[1]);
                dstNumbers.Add((Number) _r[2]);
            }

            IEnumerable<DataRow> srcRows = r.Source.GetData(srcNumbers);
            IEnumerable<DataRow> dstRows = r.Destination.GetData(dstNumbers);

            var result = new HybreUniformResult();

            result.Add(r.Source, srcRows);
            result.Add(r.Destination, dstRows);

            result.Add(r, relrows);

            return result;
        }
    }
}