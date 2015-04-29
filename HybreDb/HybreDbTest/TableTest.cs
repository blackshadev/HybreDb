using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using HybreDb;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Tables;
using HybreDb.Tables.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DateTime = HybreDb.Tables.Types.DateTime;

namespace HybreDbTest {
    [TestClass]
    public class TableTest {

        public const string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0987654321";
        public static Database Db = new Database("UnitTest", true);

        [TestMethod]
        public void Create() {


            var tab = DummyData.TestTable(Db, "Test");
            DummyData.TestRows(tab);

            Trace.WriteLine("After insert");
            Trace.WriteLine(tab.ToString());

            tab = Db.Reopen(tab);
            
            Trace.WriteLine("After Read");
            Trace.WriteLine(tab.ToString());

            Trace.WriteLine("\nTessa lookup");
            var tessas = tab.FindRows(new KeyValuePair<string, object>("Name", new Text("Tessa")));

            Trace.WriteLine(String.Join("\n", tessas.Select(n => n.ToString())));
            
            Trace.WriteLine("\n 22 lookup");
            var tters = tab.FindRows(new KeyValuePair<string, object>("Age", new Number(22)));
            Trace.WriteLine(String.Join("\n", tters.Select(n => n.ToString())));

            foreach (var r in tessas.ToArray())
                tab.Remove(r.Index);

            tab.Commit();

            Trace.WriteLine("\nAfter Delete");
            Trace.WriteLine(tab.ToString());

            foreach (var r in tters.ToArray())
                tab.Update(r.Index, "Age", new Number(23));


            tab = Db.Reopen(tab);

            Trace.WriteLine("\nAfter Read");
            Trace.WriteLine(tab.ToString());

            Trace.WriteLine("\n 23 lookup");
            var dters = tab.FindRows(new KeyValuePair<string, object>("Age", new Number(23)));
            Trace.WriteLine(String.Join("\n", dters.Select(n => n.ToString())));

        }

        [TestMethod] 
        public void Bench() {
            const int n = 100000;

            var set = GenerateDataset(n);
            
            var cols = new[] {
                new DataColumn { Name = "Name", DataType = DataTypes.Types.Text, HasIndex = true },
                new DataColumn { Name = "Age", DataType = DataTypes.Types.Number, HasIndex = true },
                new DataColumn { Name = "UnIndexed_Age", DataType = DataTypes.Types.Number },
                new DataColumn { Name = "Inserted", DataType = DataTypes.Types.DateTime }
            };

            var tab = Db.NewTable("Bench", cols);
            
            int i = 0;
            Time("Insert " + n + " records", () => { 
                for(i = 0; i < n; i++)
                    tab.Insert(set[i]);
            });


            Time("Count", () => { i = tab.Rows.Count(); });
            Assert.IsFalse(i != n, "Missing records");
            
            Time("Commit", () => { tab.Commit(); });

            Time("\nRead in", () => { tab = Db.Reopen(tab); });
            
            Time("Count", () => { i = tab.Rows.Count(); });

            Time("Count", () => { i = tab.Rows.Count(); });

            Time("Index Age", () => {
                var rows = tab.FindRows(new KeyValuePair<string, object>("Age", new Number(90)));
                Console.WriteLine(String.Join("\n", rows.Select(e => e.ToString())));
            });

            Console.WriteLine("\n");
            Time("Unindexed Age", () => {
                var rows = tab.FindRows(new KeyValuePair<string, object>("UnIndexed_Age", new Number(90)));
                Console.WriteLine(String.Join("\n", rows.Select(e => e.ToString())));
            });

            Assert.IsFalse(i != n, "Missing records");
        }


        private static IDataType[][] GenerateDataset(int N) {
            var rnd = new Random();
            var o = new IDataType[N][];


            for (var i = 0; i < N; i++) {
                var a = new Number(rnd.Next(0, 100));
                o[i] = new IDataType[] {
                    new Text(RandomString(rnd.Next(0, 25), rnd)),
                    a,
                    a,
                    new DateTime(System.DateTime.Now), 
                };
            }

            return o;
        }

        private static string RandomString(int n, Random r) {
            var c = new char[n];

            for (var i = 0; i < n; i++)
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
