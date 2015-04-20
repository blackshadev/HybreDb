using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.BPlusTree;
using HybreDb.Storage;
using HybreDb.Tables;
using HybreDb.Test;
using HybreDb.BPlusTree.DataTypes;

namespace HybreDb {
    public class Program {
        public const string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0987654321";
        public const int Seed = 1;
        
        static void Test1() {

            if(File.Exists("test.dat"))
                File.Delete("test.dat");

            var t = new DiskTree<Text, TestData>("test.dat", 4, 2);

            var nums = new int[] {0,1,2,3,4,5,6,7,8,9};
            

            foreach(var i in nums)
                t.Insert(new Text("Test_" + i), new TestData { Key = i, Value = "test_" + i });

            Console.WriteLine("Before Write");
            foreach(var v in t)
                Console.WriteLine(v);

            t.Write();

            t.Dispose();

            t = new DiskTree<Text, TestData>("test.dat", 4, 10);
            t.Read();

            Console.WriteLine("\nAfter Read");
            foreach (var v in t)
                Console.WriteLine(v);


            //foreach (var i in nums)
            //    t.Remove("Test_" + i);


            //Console.WriteLine("\nAfter delete");
            //foreach (var v in t)
            //    Console.WriteLine(v);

            TestData _d;
            Console.WriteLine("\nTry Insert:");
            _d = t.GetOrInsert(new Text("Test_5"), new TestData { Key = 5, Value = "newTest_5" });
            Console.WriteLine(_d.Value);
            _d = t.GetOrInsert(new Text("Test_10"), new TestData { Key = 10, Value = "newTest_10" });
            Console.WriteLine(_d.Value);

            Console.WriteLine("\nAfter Try Insert");
            foreach (var v in t)
                Console.WriteLine(v);
            

            Console.ReadKey();
        }

        static void Test2() {
            const int n = 100000;

            var set = GenerateDataset(n);

            var cols = new[] {
                new DataColumn { Name = "Name", DataType = Tables.DataType.Types.Text, HasIndex = true},
                new DataColumn { Name = "Age", DataType = Tables.DataType.Types.Number },
                new DataColumn { Name = "Inserted", DataType = Tables.DataType.Types.DateTime }
            };

            var tab = new Table("Test2", cols);

            int i = 0;
            Time("Insert " + n + " records", () => {
                for (i = 0; i < n; i++)
                    tab.Insert(set[i]);
            });


            Time("Count", () => { i = tab.Rows.Count(); });
            if(i != n)
                throw new Exception("Missing records");

            Time("Commit", () => { tab.Commit(); });
            tab.Dispose();

            Time("\nRead in", () => { tab = new Table("Test2"); });

            Time("Count", () => { i = tab.Rows.Count(); });

            if(i != n)
                throw new Exception("Missing records");

        }

        static void Main(string[] args) {
            Test2();
        }

        private static IDataType[][] GenerateDataset(int N) {
            var rnd = new Random(Seed);
            var o = new IDataType[N][];


            for (var i = 0; i < N; i++) {
                o[i] = new IDataType[] {
                    new Text(RandomString(rnd.Next(0, 25), rnd)),
                    new Number(rnd.Next()),
                    new BPlusTree.DataTypes.DateTime(System.DateTime.Now), 
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
