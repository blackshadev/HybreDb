using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HybreDb.Actions.Result {
    public class ActionListResult : HybreResult {

        [JsonProperty("actions")]
        public Dictionary<string, ActionInfo> ActionByNames; 

        public ActionListResult(IEnumerable<Type> actions) {
            ActionByNames = new Dictionary<string, ActionInfo>();

            foreach (var k in actions)
                ActionByNames[k.Name] = new ActionInfo(k);
            
        }

    }

    public class ActionInfo {

        public string Name;

        [JsonProperty("parameters")]
        public Dictionary<string, ActionParameter> Parameters; 

        public ActionInfo(Type action) {
            Name = action.Name;
            Parameters = ActionParameter.ActionParameters(action);


        }

        public class ActionParameter {

            [JsonProperty("name")]
            public string Name;

            [JsonProperty("type")]
            public string TypeName;

            [JsonProperty("parameters")]
            public Dictionary<string, ActionParameter> Parameters; 



            public ActionParameter(JsonPropertyAttribute prop, Type type) {
                Name = prop.PropertyName;
                TypeName = type.Name;
                Parameters = ActionParameters(type);
            }

            public static Dictionary<string, ActionParameter> ActionParameters(Type t) {
                var pars = new Dictionary<string, ActionParameter>();
                foreach (var prop in t.GetFields(BindingFlags.Public | BindingFlags.Instance)) {
                    var attrs = (JsonPropertyAttribute[])prop.GetCustomAttributes(typeof(JsonPropertyAttribute), false);
                    if (attrs.Length < 1) continue;
                    pars[attrs[0].PropertyName] = new ActionParameter(attrs[0], prop.FieldType);
                }

                return pars;
            }
        }
    }
}
