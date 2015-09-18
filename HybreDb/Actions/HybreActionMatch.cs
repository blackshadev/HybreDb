using System.Collections.Generic;
using HybreDb.Actions.Result;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Tables;
using HybreDb.Tables.Types;
using Newtonsoft.Json;

namespace HybreDb.Actions {
    /// <summary>
    ///     Action which finds records in a given table based on given conditions
    /// </summary>
    public class HybreActionMatch : IHybreAction {
        [JsonProperty("condition")]
        public ActionCondition[] Condition;

        [JsonProperty("table")]
        public string TableName;

        public HybreResult Execute(Database db) {
            Table t = db[TableName];

            var rows = new Numbers();
            for (int i = 0; i < Condition.Length; i++) {
                ActionCondition c = Condition[i];
                DataColumn field = t.Columns[c.FieldName];
                IDataType data = field.DataType.CreateType(c.Value);
                ActionCondition.ConditionType type = c.Type;


                Numbers local_rows = t.FindKeys(new KeyValuePair<string, object>(c.FieldName, data));

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

        [JsonProperty("field")]
        public string FieldName;

        [JsonProperty("type")]
        public ConditionType Type;

        [JsonProperty("value")]
        public object Value;
    }
}