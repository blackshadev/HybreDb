using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Tables;
using HybreDb.Tables.Types;
using DateTime = HybreDb.Tables.Types.DateTime;

namespace HybreDbTest {
    public static class DummyData {

        public static Table TestTable(Database db, string n) {
            var cols = new[] {
                new DataColumn("Name", DataTypes.Types.Text, true),
                new DataColumn("Age", DataTypes.Types.Number, true),
                new DataColumn("Unindexed_Age", DataTypes.Types.Number),
                new DataColumn("Inserted", DataTypes.Types.DateTime)

            };
            return db.NewTable(n, cols);

        }

        public static void TestRows(Table tab) {
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
        }
    }
}
