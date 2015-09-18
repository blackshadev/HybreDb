using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using HybreDb.BPlusTree;
using HybreDb.Tables.Types;
using HybreDb.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HybreDb.BPlusTree.DataTypes;

namespace HybreDbTest {
    [TestClass]
    public class TreeTest {
        
        private Tree<Number, TestData> Tree; 
        private int N = 10000;
        private List<Number> RandomNumbers; 


        public TreeTest() {
            Tree = new Tree<Number, TestData>(50);
            RandomNumbers = GenerateRandomNumbers(N);
        }

        protected static List<Number> GenerateRandomNumbers(int n) {
            var l = new List<Number>();

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
                Tree.Insert(t, new TestData {Key = t, Value = "Test_" + t });
            sw.Stop();
            Trace.WriteLine("Insert took " + sw.ElapsedMilliseconds + "ms");

            CheckTree();
            CheckAccess();
            MeasureRandomAccess(1000000);
            //CheckRemoves();
            CheckTree();
        }

        private void MeasureRandomAccess(int n) {
            var rnd = new Random();
            var sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < n; i++) {
                int iX = rnd.Next(0, N);
                var num = RandomNumbers[iX];
                Assert.IsFalse(num != Tree[num].Key, "failed");
            }

            sw.Stop();
            Trace.WriteLine("Random access of " + n + " elements took " + sw.ElapsedMilliseconds + "ms");
        }

        public void CheckAccess() {
            var sw = new Stopwatch();
            sw.Start();
            foreach (var n in RandomNumbers) {
                var v = Tree[n];
                Assert.IsFalse(v.Key != n, "Accessed failed");
            }
            sw.Stop();
            Trace.WriteLine("Accessed took " + sw.ElapsedMilliseconds + "ms");
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
                Assert.IsFalse(v.Key < prev, "Invalid tree");
                prev = v.Key;
                total++;
            }
            sw.Stop();

            Assert.IsFalse(total != N, "Missing keys");

            Trace.WriteLine("CheckTree took " + sw.ElapsedMilliseconds + "ms");
        }

        [TestMethod] 
        public void TestRemoval() {
            var t = new Tree<Number, TestData>(5);
            var sw = new Stopwatch();
            var nums = RandomNumbers.Take(1000).ToArray();

            sw.Start();
            foreach (int i in nums)
                t.Insert(i, new TestData { Key = i, Value = "Test_" + i } );
            sw.Stop();
            Trace.WriteLine("Insert took " + sw.ElapsedMilliseconds + "ms");


            sw.Reset();
            sw.Start();
            foreach (int i in nums) {
                Assert.IsFalse(t[i].Key != i, "Tree is invalid");
            }
            sw.Stop();
            Trace.WriteLine("Check took " + sw.ElapsedMilliseconds + "ms");


            for(var i = 0; i < nums.Length; i++) {
                t.Remove(nums[i]);
                
                for(int j = i + 1; j < nums.Length; j++)
                    Assert.IsFalse(t[nums[j]].Key != nums[j] , "Remove corrupted tree");
            }

            t.Insert(25, new TestData { Key = 25, Value = "new" });
        }

        [TestMethod] 
        public void TestBulkInsert() {
            var nums = RandomNumbers.Select(e => new KeyValuePair<Number, TestData>(e, new TestData { Key = e, Value = "Test_" + e })).ToArray();
            

            var sw = new Stopwatch();
            sw.Start();
            var t = new Tree<Number, TestData>(50);
            t.Init(nums);
            sw.Stop();
            Trace.WriteLine("Bulk insert took " + sw.ElapsedMilliseconds + "ms");

            var iX = t.Count();

            Assert.IsFalse(nums.Length != iX, "Missing entries");

            foreach (var n in nums) 
                Assert.IsFalse(n.Value != t[n.Key], "Invalid key");
            

        }

        [TestMethod] 
        public void TestText() {
            var t = new Tree<Text, TestData>(50);

            var data = RandomNumbers.Select(e => new KeyValuePair<Text, TestData>(new Text("Tester_" + e), new TestData {Key = e, Value = "Tester_" + e}) );

            var sw = new Stopwatch();
            sw.Start();
            foreach (var i in data) {
                t.Insert(i.Key, i.Value);
            }
            sw.Stop();
            Trace.WriteLine("Insert took " + sw.ElapsedMilliseconds + "ms");
            sw.Reset();
            
            sw.Start();
            foreach (var d in data) {
                Assert.IsFalse(d.Value.Key != t[d.Key].Key, "String values don't match");
            }
            sw.Stop();
            Trace.WriteLine("Accesses took " + sw.ElapsedMilliseconds + "ms");
            sw.Reset();
        }

    }

}
