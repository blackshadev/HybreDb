using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Actions.Result;
using HybreDb.Tables;
using HybreDb.Tables.Types;
using Newtonsoft.Json;

namespace HybreDb.Actions {
    /// <summary>
    /// Action which finds records in a given table based on given conditions
    /// </summary>
    public class HybreActionMatch : IHybreAction {

        [JsonProperty("table")]
        public string TableName;

        [JsonProperty("condition")]
        public ActionCondition[] Condition;

        public HybreResult Execute(Database db) {
            var t = db[TableName];

            var rows = new Numbers();
            for(var i = 0; i < Condition.Length; i++) {
                var c = Condition[i];
                var field = t.Columns[c.FieldName];
                var data = field.DataType.CreateType(c.Value);
                var type = c.Type;


                var local_rows = t.FindKeys(new KeyValuePair<string, object>(c.FieldName, data));

                if (type == ActionCondition.ConditionType.And && i > 0)
                    rows.Intersect(local_rows);
                else
                    rows.Add(local_rows);
            }

            var res = new HybreUniformResult();
            res.Add(t, t.GetData(rows));
            
            return res;
        }
    }

    public class ActionCondition {
        public enum ConditionType {
            And,
            Or
        };

        [JsonProperty("type")]
        public ConditionType Type;

        [JsonProperty("field")] 
        public string FieldName;

        [JsonProperty("value")]
        public object Value;



    }
}
