using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Actions.Result;
using HybreDb.Tables;
using Newtonsoft.Json;

namespace HybreDb.Actions {

    public class HybreActionListTables : IHybreAction {

        public HybreResult Execute(Database db) {
            var results = new HybreResult[db.Count];

            var iX = 0;
            foreach (var t in db) {
                results[iX++] = new HybreStructureResult { Table = t };
            }

            return new HybreMultipleResult {Results = results};
        }
    }
}
