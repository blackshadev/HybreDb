using System.Linq;
using HybreDb.Actions.Result;
using HybreDb.Relational;
using HybreDb.Tables;

namespace HybreDb.Actions {
    public class HybreActionAll : IHybreAction {
        public HybreResult Execute(Database db) {
            var res = new HybreUniformResult();
            foreach (Table t in db) {
                res.Add(t, t.Rows.Select(e => e.Value));

                foreach (Relation rel in t.Relations)
                    res.Add(rel, rel.Table.Rows.Select(e => e.Value));
            }

            return res;
        }
    }
}