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

            var t = new Tree<int>(4);
            var rnd = new Random();

            var nums = new List<int>();

            while(nums.Count < n) {
                int k = rnd.Next();
                if (nums.Contains(k)) continue;
                t.Insert(k, k);
                nums.Add(k);
            }

            int prev = int.MinValue;
            foreach (var v in t) {
                if(v < prev)
                    throw new Exception("NOOP");
                Console.WriteLine(v.ToString());
                prev = v;
            }

            Console.ReadKey();
        }
    }
}
