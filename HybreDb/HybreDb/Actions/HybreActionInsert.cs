using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Actions.Result;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Tables;
using Newtonsoft.Json;

namespace HybreDb.Actions {
    public class HybreActionInsert : IHybreAction {

        [JsonProperty("table")]
        public string TableName;

        [JsonProperty("data")]
        public Dictionary<string, object> Data;

        public HybreResult Execute(Database db) {
            var t = db[TableName];

            if(t.Columns.Length != Data.Count)
                throw new ArgumentException("Invalid number of data given, expected " + t.Columns.Length + " got " + Data.Count);

            var row = new IDataType[t.Columns.Length];
            foreach (var d in Data) {
                var iX = t.Columns.GetIndex(d.Key);
                row[iX] = t.Columns[iX].DataType.CreateType(d.Value);
            }

            t.Insert(row);
            t.Commit();
            
            return new HybreUpdateResult { Affected = 1 };
        }
    }
}
