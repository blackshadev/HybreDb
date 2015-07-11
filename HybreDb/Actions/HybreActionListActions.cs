using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Actions.Result;

namespace HybreDb.Actions {
    public class HybreActionListActions : IHybreAction {
        public HybreResult Execute(Database db) {
            const string @namespace = "HybreDb.Actions";
            var q = from t in Assembly.GetExecutingAssembly().GetTypes()
                    where t.IsClass && t.Namespace == @namespace && t.IsPublic && typeof(IHybreAction).IsAssignableFrom(t)
                    select t;
            
            return new ActionListResult(q);
        }
    }
}
