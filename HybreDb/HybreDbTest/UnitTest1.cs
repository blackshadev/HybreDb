using System;
using System.Collections.Generic;
using System.Diagnostics;
using HybreDb.BPlusTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HybreDbTest {
    [TestClass]
    public class TreeTest {
        private Tree<int> Tree;

        public TreeTest() {
            Tree = new Tree<int>(50);
        }

        [TestMethod]
        public void TestInserts() {
            int n = 10000;

            var rnd = new Random();
            var nums = new List<int>();

            while (nums.Count < n) {
                int k = rnd.Next();
                if (nums.Contains(k)) continue;                
                nums.Add(k);
            }

            var sw = new Stopwatch();
            sw.Start();
            foreach (int t in nums)
                Tree.Insert(t, t);
            sw.Stop();
            Trace.WriteLine("Insert took " + sw.ElapsedMilliseconds + "ms");

            CheckTree();
        }


        public void CheckTree() {
            var sw = new Stopwatch();
            sw.Start();
            int prev = int.MinValue;
            foreach (var v in Tree) {
                Assert.IsFalse(v < prev, "Invalid tree");
                prev = v;
            }
            sw.Stop();
            Trace.WriteLine("CheckTree took " + sw.ElapsedMilliseconds + "ms");
        }
    }
}
