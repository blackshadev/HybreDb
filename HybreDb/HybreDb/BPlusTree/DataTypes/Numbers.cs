using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Storage;

namespace HybreDb.BPlusTree.DataTypes {
    public class Numbers : ITreeSerializable, IEnumerable<Number> {
        public HashSet<Number> Nums;

        public Numbers(BinaryReader rdr) : this() {
            Deserialize(rdr);
        }

        public Numbers() {
            Nums = new HashSet<Number>();
        }

        public void Add(Number n) {
            Nums.Add(n);
        }

        public void Add(Numbers n) {
            Nums.UnionWith(n.Nums);
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
    }
}
