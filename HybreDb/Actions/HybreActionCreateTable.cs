using System.Collections.Generic;
using HybreDb.Actions.Result;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Tables;
using Newtonsoft.Json;

namespace HybreDb.Actions {
    public class HybreActionCreateTable : IHybreAction {
        [JsonProperty("columns")]
        public DataColumn[] Columns;

        [JsonProperty("data")]
        public Dictionary<string, object>[] Data;

        [JsonProperty("table")]
        public string TableName;

        public HybreResult Execute(Database db) {
            Table t = db.NewTable(TableName, Columns);

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

            return new HybreUpdateResult {Affected = i};
        }
    }
}