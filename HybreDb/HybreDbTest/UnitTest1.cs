using System;
using System.Collections.Generic;
using HybreDb.BPlusTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HybreDbTest {
    [TestClass]
    public class TreeTest {
        private Tree<int> Tree;

        public TreeTest() {
            Tree = new Tree<int>(5);
        }

        [TestMethod]
        public void TestInserts() {
            int n = 1000;

            var rnd = new Random();
            var nums = new List<int>();

            while (nums.Count < n) {
                int k = rnd.Next();
                if (nums.Contains(k)) continue;
                Tree.Insert(k, k);
                nums.Add(k);
            }

            CheckTree();
        }


        public void CheckTree() {

            int prev = int.MinValue;
            foreach (var v in Tree) {
                Assert.IsFalse(v < prev, "Invalid tree");
                prev = v;
            }
        }
    }
}
