using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using HybreDb.BPlusTree;
using HybreDb.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HybreDb.BPlusTree.DataTypes;

namespace HybreDbTest {
    [TestClass]
    public class DiskTree {

        public const int N = 10000;
        public Stopwatch sw = new Stopwatch();

        [TestMethod]
        public void TestInserts() {
            var numbers = new Number[N];
            GenerateRandomNumbers(numbers);

            var t = new DiskTree<IDataType, TestData>("test.dat", 20);

            sw.Start();
            foreach(var n in numbers)
                t.Insert(n, new TestData { Key = n, Value = "test_" + n} );
            sw.Stop();
            Trace.WriteLine("Insert took " + sw.ElapsedMilliseconds + "ms");
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
            sw.Reset();

        }

        [TestMethod] 
        public void WriteCheck() {
            if(File.Exists("test.dat"))
                File.Delete("test.dat");

            var t = new DiskTree<Number, TestData>("test.dat", 32);

            for (var i = 0; i < 10; i++)
                t.Insert(i, new TestData{ Key = i, Value = "test_" + i});
            
            t.Write();


        }

        [TestMethod]
        public void ReadCheck() {
            var t = new DiskTree<Number, TestData>("test.dat", 32);

            var sw = new Stopwatch();
            sw.Start();
            t.Read();
            sw.Stop();
            Trace.WriteLine("Read took " + sw.ElapsedTicks + "ticks");
            sw.Reset();

            sw.Start();
            int prev = int.MinValue;
            foreach(var k in t) {
                Assert.IsFalse(k.Key < prev, "Read failed");
                prev = k.Key;
            }
            sw.Stop();
            Trace.WriteLine("Iter took " + sw.ElapsedMilliseconds + "ms");
            

        }

        public static void CheckAccess(DiskTree<IDataType, TestData> t, Number[] nums) {

            foreach (var n in nums) 
                Assert.IsFalse(n != t[n].Key, "Inaccessable number");
            
        }

        public static void CheckIterate(DiskTree<IDataType, TestData> t, Number[] nums) {
            int prev = int.MinValue;

            int iX = 0;
            foreach (var n in t) {
                Assert.IsFalse(prev >= n.Key,  "Iterate out of order");
                prev = n.Key;
                iX++;
            }
            Assert.IsFalse(nums.Length != iX, "Missing items");
        }

        public static void GenerateRandomNumbers(Number[] nums) {
            var rnd = new Random();

            for (int i = 0; i < nums.Length;) {
                var n = rnd.Next();
                if (nums.Contains(new Number(n))) continue;
                nums[i++] = n;
            } 
        }
    }
}
