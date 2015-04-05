using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Storage;

namespace HybreDb.BPlusTree.DataTypes {
    public class Number : ITreeSerializable, IComparable {
        private int Data;

        public void Serialize(BinaryWriter wrtr) {
            wrtr.Write(Data);
        }

        public void Deserialize(BinaryReader rdr) {
            Data = rdr.ReadInt32();
        }

        public static implicit operator Number(int i) {
            return new Number {Data = i};
        }

        public static implicit operator int(Number n) {
            return n.Data;
        }

        public int CompareTo(object obj) {
            var n = obj as Number;
            if (n == null) throw new ArgumentException("Number cannot be compared to a non numeric");

            return Data.CompareTo(n.Data);
        }
    }
}
