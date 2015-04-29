using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Actions.Result;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Tables;
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

        public static IDataType[] ParseData(Table t, Dictionary<string, object> data) {
            if (t.Columns.Length != data.Count)
                throw new ArgumentException("Invalid number of data given, expected " + t.Columns.Length + " got " + data.Count);

            var row = new IDataType[t.Columns.Length];
            foreach (var d in data) {
                var iX = t.Columns.GetIndex(d.Key);
                row[iX] = t.Columns[iX].DataType.CreateType(d.Value);
            }

            return row;
        }
    }

}
