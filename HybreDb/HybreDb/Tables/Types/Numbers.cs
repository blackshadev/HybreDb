using System.Collections;
using System.Collections.Generic;
using System.IO;
using HybreDb.Storage;

namespace HybreDb.Tables.Types {
    public class Numbers : IByteSerializable, IEnumerable<Number> {
        public HashSet<Number> Nums;

        public int Count {
            get { return Nums.Count; }
        }

        public Numbers(BinaryReader rdr) : this() {
            Deserialize(rdr);
        }

        public Numbers() {
            Nums = new HashSet<Number>();
        }

        public Numbers(IEnumerable<Number> nums) {
            Nums = new HashSet<Number>(nums);
        }

        public void Add(Number n) {
            Nums.Add(n);
        }

        public void Add(Numbers n) {
            Nums.UnionWith(n.Nums);
        }

        public void Intersect(Numbers n) {
            Nums.IntersectWith(n.Nums);
        }

        public void Remove(Number n) {
            Nums.Remove(n);
        }

        public void Serialize(BinaryWriter wrtr) {
            wrtr.Write(Nums.Count);
            foreach(var n in Nums)
                n.Serialize(wrtr);
        }

        public void Deserialize(BinaryReader rdr) {
            var l = rdr.ReadInt32();

            Nums.Clear();
            for (var i = 0; i < l; i++)
                Nums.Add(new Number(rdr));

        }

        public IEnumerator<Number> GetEnumerator() {
            return Nums.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public Number[] AsArray() {
            var arr = new Number[Nums.Count];
            Nums.CopyTo(arr);
            return arr;
        }
    }
}
