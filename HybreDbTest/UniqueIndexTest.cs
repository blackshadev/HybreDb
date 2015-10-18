using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using HybreDb.BPlusTree;
using HybreDb.Tables.Types;
using HybreDb.Test;
using NUnit.Framework;
using HybreDb;
using HybreDb.Tables;

namespace HybreDbTest {
    public class UniqueIndexTest {

        readonly Database Database;
        readonly DataColumn[] Columns;
        
        public UniqueIndexTest() {
            Database = new Database("UniqueTest", true);
            Columns = new[] {
                new DataColumn("Name", DataTypes.Types.Text, IndexTree.IndexType.UniqueIndex),
                new DataColumn("Age", DataTypes.Types.Number, IndexTree.IndexType.UniqueIndex),
                new DataColumn("Unindexed_Age", DataTypes.Types.Number),
                new DataColumn("Inserted", DataTypes.Types.DateTime)
            };
            
        }


        [TestCase]
        public void TestInserts() {
            var tab = Database.NewTable("testTabInsert", Columns);
            tab.Insert(DummyData.Rows[0]);
            tab.Insert(DummyData.Rows[2]);
            tab.Insert(DummyData.Rows[4]);

            Assert.IsTrue(tab.Rows.Count() == 3, "Row Count Doesnt match");
            var res = tab.FindRows(new KeyValuePair<string, object>("Name", DummyData.Rows[2][0]));
            Assert.IsTrue(res.Count() == 1, "Not the appropiate amount of rows returned");

            Assert.IsTrue(res.First()["Name"].CompareTo(DummyData.Rows[2][0]) == 0, "Wrong data row returned");
            Assert.IsTrue(res.First()["Age"].CompareTo(DummyData.Rows[2][1]) == 0, "Wrong data row returned");

            try {
                tab.Insert(DummyData.Rows[3]);
                Assert.IsTrue(false, "Unique value got inserted, shouldn't happen");
            } catch (Exception) { }

            res = tab.FindRows(new KeyValuePair<string, object>("Name", DummyData.Rows[2][0]));
            Assert.IsTrue(res.Count() == 1, "Not the appropiate amount of rows returned");

            Assert.IsTrue(res.First()["Name"].CompareTo(DummyData.Rows[2][0]) == 0, "Wrong data row returned");
            Assert.IsTrue(res.First()["Age"].CompareTo(DummyData.Rows[2][1]) == 0, "Wrong data row returned");

        }

        [TestCase]
        public void TestRemoves() {
            var tab = Database.NewTable("testTabRemoval", Columns);
            tab.Insert(DummyData.Rows[0]);
            tab.Insert(DummyData.Rows[2]);
            tab.Insert(DummyData.Rows[4]);
            var nums = tab.FindKeys(new KeyValuePair<string, object>("Name", DummyData.Rows[2][0]));
            Assert.IsTrue(nums.Count() == 1, "Got more rows than it should");
            tab.RemoveAll(nums);

            nums = tab.FindKeys(new KeyValuePair<string, object>("Name", DummyData.Rows[2][0]));
            Assert.IsTrue(nums.Count() == 0, "Got more rows than it should, after deletion");

            tab.Insert(DummyData.Rows[3]);
            var res = tab.FindRows(new KeyValuePair<string, object>("Name", DummyData.Rows[2][0]));
            Assert.IsTrue(res.Count() == 1, "Not the appropiate amount of rows returned");

            Assert.IsTrue(res.First()["Name"].CompareTo(DummyData.Rows[3][0]) == 0, "Wrong data row returned");
            Assert.IsTrue(res.First()["Age"].CompareTo(DummyData.Rows[3][1]) == 0, "Wrong data row returned");

        }

        [TestCase]
        public void BulkInsert() {
            var tab = Database.NewTable("testTabBulk", Columns);

            tab.BulkInsert(new[] { DummyData.Rows[0], DummyData.Rows[2], DummyData.Rows[4] });
            Assert.IsTrue(tab.Rows.Count() == 3, "Missing rows");
            Database.DropTable("testTabBulk");
            tab = Database.NewTable("testTabBulk", Columns);
            try {
                tab.BulkInsert(DummyData.Rows);
                Assert.IsTrue(false, "Shouldnt succeded");
            } catch(Exception) { }
            Assert.IsTrue(tab.Rows.Count() == 0, "Should be empty");
        }
    }
}
