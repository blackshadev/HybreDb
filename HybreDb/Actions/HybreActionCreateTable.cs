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
    public class HybreActionCreateTable : IHybreAction {

        [JsonProperty("table")]
        public string TableName;

        [JsonProperty("columns")]
        public DataColumn[] Columns;

        [JsonProperty("data")]
        public Dictionary<string, object>[] Data;

        public HybreResult Execute(Database db) {

            var t = db.NewTable(TableName, Columns);
            
            var d = new IDataType[0][];
            int i = 0;

            if (Data != null) {
                d = new IDataType[Data.Length][];

                for (i = 0; i < Data.Length; i++)
                    d[i] = HybreAction.ParseData(t, Data[i]);

                
            }

            db.Write();

            t.BulkInsert(d);

            t.Commit();

            return new HybreUpdateResult { Affected = i };
        }
    }
}
