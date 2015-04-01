using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.BPlusTree;

namespace HybreDb {
    public class Program {
        static void Main(string[] args) {

            int n = 1000;

            var t = new Tree<int>(5);
            var rnd = new Random();

            var nums = new List<int>();

            for (int i = 0; i < 10; i++)
                t.Insert(i, i);

            Console.WriteLine("Before delete");
            foreach(var v in t)
                Console.WriteLine(v);

            t.Remove(5);

            Console.WriteLine("\nAfter delete");
            foreach (var v in t)
                Console.WriteLine(v);

            Console.ReadKey();
        }
    }
}
