using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using HybreDb;
using HybreDb.Relational;
using HybreDb.Tables;
using HybreDb.Tables.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HybreDbTest {
    [TestClass]
    public class Relations {
        public const int N   = 100000;
        public const int N_R = 100000;

        Database Db = new Database("RelationTest", true);
        Database DbB = new Database("RelationBench", true);


        [TestMethod]
        public void Basic() {
            Table t;
            if (!Db.Any()) {
                t = DummyData.TestTable(Db, "Test");
                DummyData.TestRows(t);

                var attrs = new[] {
                    new DataColumn("From", DataTypes.Types.Text), 
                };

                Db.NewRelation("Knows", "Test", "Test", attrs);

                t.Write();

                DummyData.TestRelations(t);

                t.Commit();
            }

            t = Db["Test"];
            foreach (var rel in t.Relations["Knows"].Data) {
                var str = String.Format("{0} -[{1}:{2}]-> {3}", rel.Item1["Name"], "Knows", rel.Item3, rel.Item2["Name"]);
                Console.WriteLine(str);
            }

            t = Db.Reopen(t);
            
            Console.WriteLine("\nAfter reopen");
            foreach (var rel in t.Relations["Knows"].Data) {
                var str = String.Format("{0} -[{1}:{2}]-> {3}", rel.Item1["Name"], "Knows", rel.Item3, rel.Item2["Name"]);
                Console.WriteLine(str);
            }

            var nums = t.FindKeys(new KeyValuePair<string, object>("Name", new Text("Tessa")));
            t.RemoveAll(nums);

            Console.WriteLine("\nAfter remove `tessa`");
            foreach (var rel in t.Relations["Knows"].Data) {
                var str = String.Format("{0} -[{1}:{2}]-> {3}", rel.Item1["Name"], "Knows", rel.Item3, rel.Item2["Name"]);
                Console.WriteLine(str);
            }

        }

        [TestMethod]
        public void Bench() {
            Table t;
            if (!DbB.Any()) {
                t = DummyData.TestTable(DbB, "Test");
                
                var attrs = new[] {
                    new DataColumn("From", DataTypes.Types.Text), 
                };

                DbB.NewRelation("Knows", "Test", "Test", attrs);

                t.Write();

                var ds = DummyData.RandomDataset(N);
                var t1 = t;
                Time("Insert Rows", () => {
                    foreach(var r in ds)
                        t1.Insert(r);
                });

                
                t.Commit();

                var rels = DummyData.RandomRelations(N, N_R);
                Time("Relations Insert", () => {
                    foreach(var r in rels)
                        t1.Relations["Knows"].Add(r.Item1.A, r.Item1.B, r.Item2);
                });

                t.Commit();
            }

            var t2 = DbB["Test"];
            Time("Row Count", () => Console.WriteLine("Row Count: " + t2.Rows.Count()));

            Time("Relation Count", () => Console.WriteLine("Relation Count: " + t2.Relations["Knows"].Count()));

            t2 = DbB.Reopen(t2);
            Console.WriteLine("\nAfter reopen");
            Trace.WriteLine("\nAfter reopen");
            
            Time("Row Count", () => Console.WriteLine("Row Count: " + t2.Rows.Count()));

            Time("Relation Count", () => Console.WriteLine("Relation Count: " + t2.Relations["Knows"].Count()));


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
