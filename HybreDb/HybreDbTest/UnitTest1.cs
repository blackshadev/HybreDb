using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HybreDb.BPlusTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HybreDbTest {
    [TestClass]
    public class TreeTest {
        private Tree<int> Tree; 
        private int N = 10000;
        private List<int> RandomNumbers; 


        public TreeTest() {
            Tree = new Tree<int>(50);
            RandomNumbers = GenerateRandomNumbers(N);
        }

        protected static List<int> GenerateRandomNumbers(int n) {
            var l = new List<int>();

            var rnd = new Random();
            while (l.Count < n) {
                var i = rnd.Next();
                if (l.Contains(i)) continue;

                l.Add(i);
            }

            return l;
        }
            
        [TestMethod]
        public void TestInserts() {
            
            var sw = new Stopwatch();
            sw.Start();
            foreach (int t in RandomNumbers)
                Tree.Insert(t, t);
            sw.Stop();
            Trace.WriteLine("Insert took " + sw.ElapsedMilliseconds + "ms");

            CheckTree();
            CheckAccess();
            //CheckRemoves();
            CheckTree();
        }

        public void CheckAccess() {
            var sw = new Stopwatch();
            sw.Start();
            foreach (var n in Tree) {
                var v = Tree[n];
                Assert.IsFalse(v != n, "Access failed");
            }
            sw.Stop();
            Trace.WriteLine("Access took " + sw.ElapsedMilliseconds + "ms");
        }

        public void CheckRemoves() {
            var sw = new Stopwatch();
            sw.Start();

            foreach (var n in RandomNumbers) {
                Tree.Remove(n);
            }

            sw.Stop();
            Trace.WriteLine("Removes took " + sw.ElapsedMilliseconds + "ms");
        }

        public void CheckTree() {
            var sw = new Stopwatch();
            int total = 0;
            int prev = int.MinValue;

            sw.Start();
            foreach (var v in Tree) {
                Assert.IsFalse(v < prev, "Invalid tree");
                prev = v;
                total++;
            }
            sw.Stop();

            Assert.IsFalse(total != N, "Missing keys");

            Trace.WriteLine("CheckTree took " + sw.ElapsedMilliseconds + "ms");
        }

        [TestMethod] 
        public void TestRemoval() {
            var t = new Tree<int>(5);
            var nums = RandomNumbers.Take(100).ToArray();
            foreach (int i in nums)
                t.Insert(i, i);

            foreach (int i in nums) {
                Assert.IsFalse(t[i] != i, "Tree is invalid");
            }

            for(var i = 0; i < nums.Length; i++) {
                t.Remove(nums[i]);
                
                for(int j = i + 1; j < nums.Length; j++)
                    Assert.IsFalse(t[nums[j]] != nums[j] , "Remove corrupted tree");
            }

            t.Insert(25, 25);
        }
    }
}
