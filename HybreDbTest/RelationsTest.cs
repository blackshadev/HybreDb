using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HybreDb;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Tables;
using HybreDb.Tables.Types;
using NUnit.Framework;
using Text = HybreDb.Tables.Types.Text;

namespace HybreDbTest {
    public class Relations {
        public const int N = 110000;
        public const int N_R = 110000;

        public const int SampleSize = 100000;
        public const int SampleSize_R = 100000;


        private readonly Database Db = new Database("RelationTest", true);
        private readonly Database DbB = new Database("RelationBench", true);


        [TestCase] public void Basic() {
            Table t;
            if (!Db.Any()) {
                t = DummyData.TestTable(Db, "Test");
                DummyData.TestRows(t);

                var attrs = new[] {
                    new DataColumn("From", DataTypes.Types.Text)
                };

                Db.NewRelation("Knows", "Test", "Test", attrs);

                t.Write();

                DummyData.TestRelations(t);

                t.Commit();
            }

            t = Db["Test"];
            foreach (var rel in t.Relations["Knows"].Data) {
                string str = String.Format("{0} -[{1}:{2}]-> {3}", rel.Item1["Name"], "Knows", rel.Item3,
                    rel.Item2["Name"]);
                Console.WriteLine(str);
            }

            t = Db.Reopen(t);

            Console.WriteLine("\nAfter reopen");
            foreach (var rel in t.Relations["Knows"].Data) {
                string str = String.Format("{0} -[{1}:{2}]-> {3}", rel.Item1["Name"], "Knows", rel.Item3,
                    rel.Item2["Name"]);
                Console.WriteLine(str);
            }

            Numbers nums = t.FindKeys(new KeyValuePair<string, object>("Name", new Text("Tessa")));
            t.RemoveAll(nums);

            Console.WriteLine("\nAfter remove `tessa`");
            foreach (var rel in t.Relations["Knows"].Data) {
                string str = String.Format("{0} -[{1}:{2}]-> {3}", rel.Item1["Name"], "Knows", rel.Item3,
                    rel.Item2["Name"]);
                Console.WriteLine(str);
            }
        }

        [TestCase] 
        public void Bench() {
            Table t;
            if (!DbB.Any()) {
                t = DummyData.TestTable(DbB, "Test");

                var attrs = new[] {
                    new DataColumn("From", DataTypes.Types.Text)
                };

                DbB.NewRelation("Knows", "Test", "Test", attrs);

                t.Write();

                IDataType[][] ds = DummyData.RandomDataset(N);
                IEnumerable<IDataType[]> rows = ds.Take(N - SampleSize);

                Table t1 = t;
                foreach (var r in rows)
                    t1.Insert(r);

                rows = ds.Skip(N - SampleSize);
                Time("Insert " + SampleSize + " Rows", () => {
                    foreach (var r in rows)
                        t1.Insert(r);
                });


                t.Commit();

                Tuple<NumberPair, IDataType[]>[] rels = DummyData.RandomRelations(N, N_R);
                IEnumerable<Tuple<NumberPair, IDataType[]>> rels_s = rels.Take(N_R - SampleSize_R);

                foreach (var r in rels_s)
                    t1.Relations["Knows"].Add(r.Item1.A, r.Item1.B, r.Item2);

                rels_s = rels.Skip(N_R - SampleSize_R);
                Time("Insert " + SampleSize_R + " Relations", () => {
                    foreach (var r in rels_s)
                        t1.Relations["Knows"].Add(r.Item1.A, r.Item1.B, r.Item2);
                });

                t.Commit();
            }

            Table t2 = DbB["Test"];
            Time("Row Count", () => Console.WriteLine("Row Count: " + t2.Rows.Count()));

            Time("Relation Count", () => Console.WriteLine("Relation Count: " + t2.Relations["Knows"].Count()));

            Trace.WriteLine("Read operations " + t2.Rows.Reads);
            Trace.WriteLine("Write operations " + t2.Rows.Writes);
            Trace.WriteLine("Free operations " + t2.Rows.Freed);

            t2 = DbB.Reopen(t2);
            Console.WriteLine("\nAfter reopen");
            Trace.WriteLine("\nAfter reopen");

            Time("Row Count", () => Console.WriteLine("Row Count: " + t2.Rows.Count()));

            Time("Relation Count", () => Console.WriteLine("Relation Count: " + t2.Relations["Knows"].Count()));

            Time("Row Count", () => Console.WriteLine("Row Count: " + t2.Rows.Count()));

            Time("Relation Count", () => Console.WriteLine("Relation Count: " + t2.Relations["Knows"].Count()));

            Trace.WriteLine("Read operations " + t2.Rows.Reads);
            Trace.WriteLine("Write operations " + t2.Rows.Writes);
            Trace.WriteLine("Free operations " + t2.Rows.Freed);
        }

        private static void Time(string txt, Action act) {
            GC.Collect();
            Stopwatch sw = Stopwatch.StartNew();
            act();
            sw.Stop();
            Trace.WriteLine(txt + " took " + sw.ElapsedMilliseconds + "ms");
        }
    }
}