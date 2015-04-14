using System;
using System.IO;

namespace HybreDb.BPlusTree.DataTypes {
    public class Text : DataType {
        private string Data;

        public Text() {}

        public Text(string d) {
            Data = d;
        }

        public Text(BinaryReader rdr) : base(rdr) {}

        public override int CompareTo(object obj) {
            var o = obj as Text;
            if (o == null) throw new ArgumentException("Cannot compare to non textual types");

            return String.Compare(Data, o.Data, StringComparison.Ordinal);
        }

        public override void Serialize(BinaryWriter wrtr) {
            wrtr.Write(Data);
        }

        public override void Deserialize(BinaryReader rdr) {
            Data = rdr.ReadString();
        }

        public static implicit operator Text(string t) {
            return new Text(t);
        }

        public static implicit operator string(Text t) {
            return t.Data;
        }

        public override string ToString() {
            return Data;
        }
    }
}
