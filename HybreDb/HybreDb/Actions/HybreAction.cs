using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Actions.Result;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Tables;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HybreDb.Actions {
    [JsonConverter(typeof(HybreActionJsonSerialiser))]
    public interface IHybreAction {
        HybreResult Execute(Database db);
    }

    public static class HybreAction {
        public static IHybreAction Parse(string json) {
            return JsonConvert.DeserializeObject<IHybreAction>(json);
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

        public static HybreResult Execute(Database db, IHybreAction act) {
            HybreResult res;
            var sw = new Stopwatch();
            sw.Start();
            try {
                res = act.Execute(db);
            }
            catch (Exception e) {
                db.Revert();
                res = new HybreError(e);
            }
            res.ElapsedTime = sw.ElapsedMilliseconds;

            return res;
        }
    }

    public class HybreActionJsonSerialiser : JsonConverter {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            reader.Read();
            reader.Read();

            if(reader.Path.ToLower() != "method")
                throw new ArgumentException("Expected `method` as first json parameter");

            var m = reader.Value as string;
            var cName = "HybreDb.Actions.HybreAction" + char.ToUpper(m[0]) + m.Substring(1).ToLower();
            var t = Type.GetType(cName);

            reader.Read();
            reader.Read();
            var o = Activator.CreateInstance(t, new object[0]);
            
            serializer.Populate(reader, o);
            
            reader.Read();
            return o;
        }

        public override bool CanConvert(Type objectType) {
            return typeof (IHybreAction).IsAssignableFrom(objectType);
        }
    }

}
