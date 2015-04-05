using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.BPlusTree;
using HybreDb.Storage;
using HybreDb.Test;

namespace HybreDb {
    public class Program {
        
        static void Main(string[] args) {

            var t = new Tree<TestData>(5);

            var nums = new int[] {9, 5, 1, 13, 54, 4, 23, 8, 45, 3, 12, 44};
            

            foreach(var i in nums)
                t.Insert(i, new TestData { Key = i, Value = "test_" + i });

            Console.WriteLine("Before delete");
            foreach(var v in t)
                Console.WriteLine(v);

            foreach (var i in nums)
                t.Remove(i);


            Console.WriteLine("\nAfter delete");
            foreach (var v in t)
                Console.WriteLine(v);

            Console.ReadKey();
        }
    }
}
