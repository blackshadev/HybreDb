using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Actions.Result;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HybreDb.Actions {
    public interface IHybreAction {
        HybreResult Execute(Database db);
    }

    public static class HybreAction {
        public static IHybreAction Parse(string json) {
            var o = JObject.Parse(json);

            var m = (string) o["method"];
            var cName = "HybreDb.Actions.HybreAction" + char.ToUpper(m[0]) + m.Substring(1);

            
            var t = Type.GetType(cName);
            return (IHybreAction)o["params"].ToObject(t);

        }
    }

}
