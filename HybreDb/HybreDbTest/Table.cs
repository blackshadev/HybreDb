using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using HybreDb;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Tables;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataType = HybreDb.Tables.DataType;

namespace HybreDbTest {
    [TestClass]
    public class Table {

        public const string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0987654321";

        [TestMethod]
        public void Create() {


            var cols = new[] {
                new DataColumn("Name", DataType.Types.Text, true),
                new DataColumn("Age", DataType.Types.Number),
                new DataColumn("Inserted", DataType.Types.DateTime)

            };
            var tab = new HybreDb.Tables.Table("Test", cols);

            tab.Insert(new IDataType[] {
                new Text("Vincent"),
                new Number(22),
                new HybreDb.BPlusTree.DataTypes.DateTime(System.DateTime.Now)
            });
            tab.Insert(new IDataType[] {
                new Text("Wouter"),
                new Number(21),
                new HybreDb.BPlusTree.DataTypes.DateTime(System.DateTime.Now)
            });
            tab.Insert(new IDataType[] {
                new Text("Tessa"),
                new Number(20),
                new HybreDb.BPlusTree.DataTypes.DateTime(System.DateTime.Now)
            });
            tab.Insert(new IDataType[] {
                new Text("Tessa"),
                new Number(26),
                new HybreDb.BPlusTree.DataTypes.DateTime(System.DateTime.Now)
            });

            Trace.WriteLine("After insert");
            Trace.WriteLine(tab.ToString());

            tab.Commit();
            tab.Dispose();

            tab = new HybreDb.Tables.Table("Test");
            
            Trace.WriteLine("After Read");
            Trace.WriteLine(tab.ToString());
        }

        [TestMethod] 
        public void Bench() {
            const int n = 100000;

            var set = GenerateDataset(n);
            
            var cols = new[] {
                new DataColumn { Name = "Name", DataType = DataType.Types.Text, HasIndex = true},
                new DataColumn { Name = "Age", DataType = DataType.Types.Number },
                new DataColumn { Name = "Inserted", DataType = DataType.Types.DateTime }
            };

            var tab = new HybreDb.Tables.Table("Bench", cols);
            
            int i = 0;
            Time("Insert " + n + " records", () => { 
                for(i = 0; i < n; i++)
                    tab.Insert(set[i]);
            });


            Time("Count", () => { i = tab.Rows.Count(); });
            Assert.IsFalse(i != n, "Missing records");
            
            Time("Commit", () => { tab.Commit(); });
            tab.Dispose();

            Time("\nRead in", () => { tab = new HybreDb.Tables.Table("Bench"); });
            
            Time("Count", () => { i = tab.Rows.Count(); });
            
            Assert.IsFalse(i != n, "Missing records");
        }

        private static IDataType[][] GenerateDataset(int N) {
            var rnd = new Random();
            var o = new IDataType[N][];


            for (var i = 0; i < N; i++) {
                o[i] = new IDataType[] {
                    new Text(RandomString(rnd.Next(0, 25), rnd)),
                    new Number(rnd.Next()),
                    new HybreDb.BPlusTree.DataTypes.DateTime(System.DateTime.Now), 
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
