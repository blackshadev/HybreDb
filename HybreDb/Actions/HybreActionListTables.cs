using HybreDb.Actions.Result;
using HybreDb.Tables;

namespace HybreDb.Actions {
    public class HybreActionListTables : IHybreAction {
        public HybreResult Execute(Database db) {
            var results = new HybreResult[db.Count];

            int iX = 0;
            foreach (Table t in db) {
                results[iX++] = new HybreStructureResult {Table = t};
            }

            return new HybreMultipleResult {Results = results};
        }
    }
}