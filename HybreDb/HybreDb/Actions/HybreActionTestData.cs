using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Actions.Result;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Tables;
using HybreDb.Tables.Types;
using Newtonsoft.Json;
using DateTime = HybreDb.Tables.Types.DateTime;

namespace HybreDb.Actions {
    public class HybreActionTestData : IHybreAction {

        public HybreResult Execute(Database db) {

            db.NewTable("People", new [] {
                new DataColumn("Name", DataTypes.Types.Text, true),
                new DataColumn("Age", DataTypes.Types.Number, true),
                new DataColumn("UnindexedAge", DataTypes.Types.Number),
                new DataColumn("Inserted", DataTypes.Types.DateTime)
            });
            db.NewRelation("Knows", "People", "People", new[] {
                new DataColumn("From", DataTypes.Types.Text)
            });

            db.Write();

            var tab = db["People"];
            tab.Insert(new IDataType[] {
                new Text("Vincent"),
                new Number(22),
                new Number(22),
                new DateTime(System.DateTime.Now)
            });
            tab.Insert(new IDataType[] {
                new Text("Wouter"),
                new Number(22),
                new Number(22),
                new DateTime(System.DateTime.Now)
            });
            tab.Insert(new IDataType[] {
                new Text("Tessa"),
                new Number(20),
                new Number(20),
                new DateTime(System.DateTime.Now)
            });
            tab.Insert(new IDataType[] {
                new Text("Tessa"),
                new Number(26),
                new Number(26),
                new DateTime(System.DateTime.Now)
            });
            tab.Insert(new IDataType[] {
                new Text("Stefan"),
                new Number(21),
                new Number(21),
                new DateTime(System.DateTime.Now)
            });

            tab.Commit();

            var rel = tab.Relations["Knows"];

            rel.Add(0, 1, new IDataType[] { new Text("UvA") });
            rel.Add(0, 2, new IDataType[] { new Text("Baken") });
            rel.Add(0, 3, new IDataType[] { new Text("Zus") });
            rel.Add(0, 4, new IDataType[] { new Text("UvA") });

            rel.Add(2, 0, new IDataType[] { new Text("Baken") });
            rel.Add(1, 2, new IDataType[] { new Text("UvA") });
            rel.Add(2, 1, new IDataType[] { new Text("UvA") });
            rel.Add(2, 0, new IDataType[] { new Text("UvA") });
            rel.Add(4, 0, new IDataType[] { new Text("UvA") });
            
            tab.Commit();

            return new HybreUpdateResult { Affected = tab.Rows.Count() };
        }
    }
}
