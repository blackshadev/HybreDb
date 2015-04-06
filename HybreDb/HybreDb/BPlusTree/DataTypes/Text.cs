using System;
using System.IO;

namespace HybreDb.BPlusTree.DataTypes {
    public class Text : IDataType {
        private string Data;

        public Text(string d) {
            Data = d;
        }

        public int CompareTo(object obj) {
            var o = obj as Text;
            if (o == null) throw new ArgumentException("Cannot compare to non textual types");

            return String.Compare(Data, o.Data, StringComparison.Ordinal);
        }

        public void Serialize(BinaryWriter wrtr) {
            throw new NotImplementedException();
        }

        public void Deserialize(BinaryReader rdr) {
            throw new NotImplementedException();
        }

        public static implicit operator Text(string t) {
            return new Text(t);
        }

        public static implicit operator string(Text t) {
            return t.Data;
        }
    }
}
