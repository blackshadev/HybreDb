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
            //nums.Add(10);
            //nums.Add(0);
            //nums.Add(15);
            //nums.Add(6);
            //nums.Add(9);
            //nums.Add(8);
            //nums.Add(17);
            //nums.Add(16);
            //nums.Add(116);
            //nums.Add(60);
            //nums.Add(61);
            //nums.Add(98);
            //nums.Add(-8);
            //nums.Add(56);
            //nums.Add(102);
            //nums.Add(-65);
            //nums.Add(21);
            //nums.Add(22);
            //nums.Add(20);
            //nums.Add(14);



            //for (var i = 0; i < nums.Count; i++)
            //    t.Insert(nums[i], nums[i]);

            while (nums.Count < n) {
                int k = rnd.Next();
                if (nums.Contains(k)) continue;
                t.Insert(k, k);
                nums.Add(k);
            }

            int total = 0;
            int prev = int.MinValue;
            foreach (var v in t) {
                if(v < prev)
                    throw new Exception("NOOP");
                Console.WriteLine(v.ToString());
                prev = v;
                total++;
            }
            if (nums.Count != total)
                throw new Exception("Something went wong");

            Console.ReadKey();
        }
    }
}
