using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Actions.Result;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Relational;
using HybreDb.Tables;
using HybreDb.Tables.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HybreDb.Actions {
    /// <summary>
    /// Every action must implement the IHybreAction. This makes it possible to execute a given action.
    /// </summary>
    public interface IHybreAction {
        /// <summary>
        /// Executes defined action.
        /// </summary>
        /// <param name="db">The database on which to execute the action</param>
        /// <returns>Result of the action</returns>
        HybreResult Execute(Database db);
    }

    /// <summary>
    /// Static class to simplify interaction with HybreActions
    /// </summary>
    public static class HybreAction {

        /// <summary>
        /// Parses a JSON string and tries to create a HybreAction of it based on the method and params in the JSON object
        /// </summary>
        /// <param name="json">String representation of a JSON object with method and params as fields</param>
        /// <returns>The parsed HybreAction</returns>
        public static IHybreAction Parse(string json) {
            var o = JObject.Parse(json);
            var m = (string)o["method"];
            var cName = "HybreDb.Actions.HybreAction" + char.ToUpper(m[0]) + m.Substring(1);
            var t = Type.GetType(cName, true, true);

            return (IHybreAction) o["params"].ToObject(t);
        }

        /// <summary>
        /// Interprets the given data to the columns of given table.
        /// </summary>
        /// <param name="t">Table to which the data must be transformed</param>
        /// <param name="data">Dictionary of the data. The key is the columnName of the data and the value is the data of that column</param>
        /// <returns>Interpreted data as typed data</returns>
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

        public static IDataType[] ParseData(Relation r, Dictionary<string, object> data) {
            if(r.Table.Columns.Length - 3 != data.Count)
                throw new ArgumentException("Invalid number of data given, expected " + (r.Table.Columns.Length - 3) + " got " + data.Count);
            
            var row = new IDataType[data.Count];
            foreach (var d in data) {
                var iX = r.Table.Columns.GetIndex(d.Key);
                row[iX - 3] = r.Table.Columns[iX].DataType.CreateType(d.Value);
            }

            return row;

        }

        public static IDataType[] ParseDataWithRel(Relation r, Dictionary<string, object> data) {
            if (r.Table.Columns.Length - 1 != data.Count)
                throw new ArgumentException("Invalid number of data given, expected " + (r.Table.Columns.Length - 1) + " got " + data.Count);

            var row = new IDataType[data.Count + 1];
            foreach (var d in data) {
                var iX = r.Table.Columns.GetIndex(d.Key);
                row[iX] = r.Table.Columns[iX].DataType.CreateType(d.Value);
            }

            row[0] = new NumberPair(row[1] as Number, row[2] as Number);

            return row;
        }

        /// <summary>
        /// Executes a given action on given database
        /// </summary>
        /// <param name="db">Database to execute the action on</param>
        /// <param name="act">Action to execute on</param>
        /// <returns>Result of the action</returns>
        /// <remarks>This function catches exceptions thrown in execution of the action, these exceptions are returned as a instance of HybreResult by HybreError</remarks>
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
            res.ElapsedTime = sw.Elapsed.TotalMilliseconds;

            return res;
        }
    }

}
