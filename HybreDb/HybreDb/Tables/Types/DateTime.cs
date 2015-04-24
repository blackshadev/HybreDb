using System;
using System.IO;

namespace HybreDb.Tables.Types {
    public class DateTime : DataType {
         private System.DateTime Data;

        public DateTime() {}


        public DateTime(System.DateTime d) {
            Data = d;
        }

        public DateTime(BinaryReader rdr) : base(rdr) {}

        public override void Serialize(BinaryWriter b) {
            b.Write(Data.ToBinary());
        }

        public override void Deserialize(BinaryReader b) {
            Data = System.DateTime.FromBinary(b.ReadInt64());
        }

        public override int CompareTo(object obj) {
            var o = obj as DateTime;

            if(o == null) throw new ArgumentException("Comparisions must have the same type");

            return Data.CompareTo(o.Data);
        }

        public override string ToString() {
            return Data.ToString();
        }
    }
}
