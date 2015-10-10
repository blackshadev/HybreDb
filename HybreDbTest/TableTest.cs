using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HybreDb;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Tables;
using HybreDb.Tables.Types;
using NUnit.Framework;
using DateTime = System.DateTime;
using Text = HybreDb.Tables.Types.Text;

namespace HybreDbTest {
    public class TableTest {
        public const int N = 100000;

        public const string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0987654321";
        public static Database Db = new Database("UnitTest", true);

        [TestCase]
        public void Create() {
            Table tab = DummyData.TestTable(Db, "Test");
            DummyData.TestRows(tab);

            Console.WriteLine("After insert");
            Console.WriteLine(tab.ToString());

            Assert.IsTrue(tab.Rows.Count() == 5, "Missing rows");

            tab = Db.Reopen(tab);

            Console.WriteLine("After Read");
            Console.WriteLine(tab.ToString());

            Assert.IsTrue(tab.Rows.Count() == 5, "Missing rows after read");

            Console.WriteLine("\nTessa lookup");
            IEnumerable<DataRow> tessas = tab.FindRows(new KeyValuePair<string, object>("Name", new Text("Tessa")));

            Console.WriteLine(String.Join("\n", tessas.Select(n => n.ToString())));
            Assert.IsTrue(tessas.Count() == 2, "Invalid lookup on indexed column");

            Console.WriteLine("\n 22 lookup");
            IEnumerable<DataRow> tters = tab.FindRows(new KeyValuePair<string, object>("Unindexed_Age", new Number(22)));
            Console.WriteLine(String.Join("\n", tters.Select(n => n.ToString())));
            Assert.IsTrue(tters.Count() == 2, "Invalid lookup on unindexed column");

            foreach (DataRow r in tessas.ToArray())
                tab.Remove(r.Index);
            
            tab.Commit();
            
            Console.WriteLine("\nAfter Delete");
            Console.WriteLine(tab.ToString());
            Assert.IsTrue(tab.Rows.Count() == 3, "Invalid row count after delete");

            foreach (DataRow r in tters.ToArray())
                tab.Update(r.Index, "Age", new Number(23));
            
            tab = Db.Reopen(tab);

            Console.WriteLine("\nAfter Read");
            Console.WriteLine(tab.ToString());

            Console.WriteLine("\n 23 lookup");
            IEnumerable<DataRow> dters = tab.FindRows(new KeyValuePair<string, object>("Age", new Number(23)));
            Console.WriteLine(String.Join("\n", dters.Select(n => n.ToString())));
            Assert.IsTrue(dters.Count() == 2, "Invalid lookup on index column after delete");
        }

        [TestCase]
        public void BulkInsert() {
            IDataType[][] set = GenerateDataset(N);
            var cols = new[] {
                new DataColumn {Name = "Name", DataType = DataTypes.Types.Text, IndexType = IndexTree.IndexType.Index },
                new DataColumn {Name = "Age", DataType = DataTypes.Types.Number, IndexType = IndexTree.IndexType.Index },
                new DataColumn {Name = "UnIndexed_Age", DataType = DataTypes.Types.Number},
                new DataColumn {Name = "Inserted", DataType = DataTypes.Types.DateTime}
            };

            Table tab = Db.NewTable("BulkBench", cols);
            Time("BulkInsert " + N + " records", () => tab.BulkInsert(set));
            tab.Commit();

            int i = 0;
            Time("Count", () => { i = tab.Rows.Count(); });
            Assert.IsFalse(i != N, "Missing records");

            Time("Commit", () => { tab.Commit(); });

            Time("\nRead in", () => { tab = Db.Reopen(tab); });

            Time("Count", () => { i = tab.Rows.Count(); });

            Time("Count", () => { i = tab.Rows.Count(); });

            Time("Index Age", () => {
                IEnumerable<DataRow> rows = tab.FindRows(new KeyValuePair<string, object>("Age", new Number(90)));
                //Console.WriteLine(String.Join("\n", rows.Select(e => e.ToString())));
            });

            Console.WriteLine("\n");
            Time("Unindexed Age", () => {
                IEnumerable<DataRow> rows =
                    tab.FindRows(new KeyValuePair<string, object>("UnIndexed_Age", new Number(90)));
                //Console.WriteLine(String.Join("\n", rows.Select(e => e.ToString())));
            });

            Assert.IsFalse(i != N, "Missing records");
        }

        [TestCase]
        public void Bench() {
            IDataType[][] set = GenerateDataset(N);

            var cols = new[] {
                new DataColumn {Name = "Name", DataType = DataTypes.Types.Text, IndexType = IndexTree.IndexType.Index},
                new DataColumn {Name = "Age", DataType = DataTypes.Types.Number, IndexType = IndexTree.IndexType.Index},
                new DataColumn {Name = "UnIndexed_Age", DataType = DataTypes.Types.Number},
                new DataColumn {Name = "Inserted", DataType = DataTypes.Types.DateTime}
            };

            Table tab = Db.NewTable("Bench", cols);

            int i = 0;
            Time("Insert " + N + " records", () => {
                for (i = 0; i < N; i++)
                    tab.Insert(set[i]);
            });


            Time("Commit", () => { tab.Commit(); });

            Time("Count", () => { i = tab.Rows.Count(); });
            Assert.IsFalse(i != N, "Missing records");


            Time("\nRead in", () => { tab = Db.Reopen(tab); });

            Time("Count", () => { i = tab.Rows.Count(); });

            Time("Count", () => { i = tab.Rows.Count(); });

            Time("Index Age", () => {
                IEnumerable<DataRow> rows = tab.FindRows(new KeyValuePair<string, object>("Age", new Number(90)));
                Console.WriteLine(String.Join("\n", rows.Select(e => e.ToString())));
            });

            Console.WriteLine("\n");
            Time("Unindexed Age", () => {
                IEnumerable<DataRow> rows =
                    tab.FindRows(new KeyValuePair<string, object>("UnIndexed_Age", new Number(90)));
                Console.WriteLine(String.Join("\n", rows.Select(e => e.ToString())));
            });

            Assert.IsFalse(i != N, "Missing records");
        }


        private static IDataType[][] GenerateDataset(int N) {
            var rnd = new Random();
            var o = new IDataType[N][];

            for (int i = 0; i < N; i++) {
                var a = new Number(rnd.Next(0, 100));
                o[i] = new IDataType[] {
                    new Text(RandomString(rnd.Next(0, 25), rnd)),
                    a,
                    a,
                    new HybreDb.Tables.Types.DateTime(DateTime.Now)
                };
            }

            return o;
        }

        private static string RandomString(int n, Random r) {
            var c = new char[n];

            for (int i = 0; i < n; i++)
                c[i] = Chars[r.Next(0, Chars.Length)];

            return new string(c);
        }

        private static void Time(string txt, Action act) {
            var sw = new Stopwatch();
            sw.Start();
            act();
            sw.Stop();
            Trace.WriteLine(txt + " took " + sw.ElapsedMilliseconds + "ms");
        }
    }
}