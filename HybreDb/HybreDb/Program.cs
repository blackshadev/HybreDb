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
using HybreDb.BPlusTree.DataTypes;

namespace HybreDb {
    public class Program {
        
        static void Main(string[] args) {

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

            Console.ReadKey();
        }
    }
}
