using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using HybreDb.BPlusTree;
using HybreDb.Tables.Types;
using HybreDb.Test;
using NUnit.Framework;

namespace HybreDbTest {
    public class DiskTreeTest {
        public const int N = 10000;
        public const int BucketSize = 8;
        public const int CacheSize = 8;

        public Stopwatch sw = new Stopwatch();
        public DiskTreeTest Cache { get; private set; }

        [TestCase, Sequential] 
        public void TestInserts() {
            var numbers = new Number[N];
            GenerateRandomNumbers(numbers);

            if (File.Exists("test_insert.dat"))
                File.Delete("test_insert.dat");

            var t = new DiskTree<Number, TestData>("test_insert.dat", BucketSize, CacheSize);
            t.Init();

            sw.Start();
            foreach (Number n in numbers)
                t.Insert(n, new TestData {Key = n, Value = "test_" + n});
            sw.Stop();
            Trace.WriteLine("Insert took " + sw.ElapsedMilliseconds + "ms");
            sw.Reset();

            t.Write();

            sw.Start();
            CheckAccess(t, numbers);
            sw.Stop();
            Trace.WriteLine("Access took " + sw.ElapsedMilliseconds + "ms");
            sw.Reset();

            sw.Start();
            CheckIterate(t, numbers);
            sw.Stop();
            Trace.WriteLine("Iterate took " + sw.ElapsedMilliseconds + "ms");
            sw.Reset();
        }

        [TestCase, Sequential] 
        public void WriteCheck() {
            if (File.Exists("test_write.dat"))
                File.Delete("test_write.dat");

            var numbers = new Number[N];
            GenerateRandomNumbers(numbers);
            KeyValuePair<Number, TestData>[] nums =
                numbers.Select(e => new KeyValuePair<Number, TestData>(e, new TestData {Key = e, Value = "Test_" + e}))
                    .ToArray();
            var sw = new Stopwatch();

            sw.Start();
            var t = new DiskTree<Number, TestData>("test_write.dat", BucketSize, CacheSize);
            t.Init(nums);
            sw.Stop();
            Console.WriteLine("Bulk insert took " + sw.ElapsedMilliseconds + "ms");

            sw.Reset();
            sw.Start();
            t.Write();
            sw.Stop();
            Console.WriteLine("Writeout took  " + sw.ElapsedMilliseconds + "ms");

            sw.Reset();
            sw.Start();
            t.Read();
            sw.Stop();
            Trace.WriteLine("Read took " + sw.ElapsedMilliseconds + "ms");

            sw.Reset();
            sw.Start();
            CheckAccess(t, numbers);
            sw.Stop();
            Trace.WriteLine("Access took " + sw.ElapsedMilliseconds + "ms");

            sw.Reset();
            sw.Start();
            CheckIterate(t, numbers);
            sw.Stop();
            Trace.WriteLine("Iterate took " + sw.ElapsedMilliseconds + "ms");
        }


        public static void CheckAccess(DiskTree<Number, TestData> t, Number[] nums) {
            foreach (Number n in nums)
                Assert.IsFalse(n != t[n].Key, "Inaccessable number");
        }

        public static void CheckIterate(DiskTree<Number, TestData> t, Number[] nums) {
            int prev = int.MinValue;

            int iX = 0;
            foreach (var n in t) {
                Assert.IsFalse(prev >= n.Key, "Iterate out of order");
                prev = n.Key;
                iX++;
            }
            Assert.IsFalse(nums.Length != iX, "Missing items");
        }

        public static void GenerateRandomNumbers(Number[] nums) {
            var rnd = new Random();

            for (int i = 0; i < nums.Length;) {
                int n = rnd.Next();
                if (nums.Contains(new Number(n))) continue;
                nums[i++] = n;
            }
        }
    }
}