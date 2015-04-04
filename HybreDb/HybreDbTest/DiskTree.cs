using System;
using System.Diagnostics;
using System.Linq;
using HybreDb.BPlusTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HybreDbTest {
    [TestClass]
    public class DiskTree {

        public const int N = 10000;
        public Stopwatch sw = new Stopwatch();


        [TestMethod]
        public void TestInserts() {
            var numbers = new int[N];
            GenerateRandomNumbers(numbers);

            var t = new DiskTree<int>("test.dat", 20);

            sw.Start();
            foreach(var n in numbers)
                t.Insert(n, n);
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

        public static void CheckAccess(DiskTree<int> t, int[] nums) {

            foreach (var n in nums) 
                Assert.IsFalse(n != t[n], "Inaccessable number");
            
        }

        public static void CheckIterate(DiskTree<int> t, int[] nums) {
            int prev = int.MinValue;

            int iX = 0;
            foreach (var n in t) {
                Assert.IsFalse(prev >= n,  "Iterate out of order");
                prev = n;
                iX++;
            }
            Assert.IsFalse(nums.Length != iX, "Missing items");
        }

        public static void GenerateRandomNumbers(int[] nums) {
            var rnd = new Random();

            for (int i = 0; i < nums.Length;) {
                var n = rnd.Next();
                if (nums.Contains(n)) continue;
                nums[i++] = n;
            } 
        }
    }
}
