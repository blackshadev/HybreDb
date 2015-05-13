using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Actions.Result;

namespace HybreDb.Actions {
    public class HybreActionAll : IHybreAction {
        public HybreResult Execute(Database db) {
            var res = new HybreUniformResult();
            foreach (var t in db) {
                res.Add(t, t.Rows.Select(e => e.Value));

                foreach(var rel in t.Relations)
                    res.Add(rel, rel.Table.Rows.Select(e => e.Value));
            }

            return res;
        }
    }
}
