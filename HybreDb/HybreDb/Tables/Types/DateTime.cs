using System;
using System.IO;

namespace HybreDb.Tables.Types {
    public class DateTime : DataType {
         private System.DateTime Data;

        public DateTime() {}


        public DateTime(System.DateTime d) {
            Data = d;
        }

        public DateTime(string dt) {
            Data = dt.ToLower() == "now" ? System.DateTime.Now : System.DateTime.Parse(dt);
        }

        public DateTime(BinaryReader rdr) {
            Read(rdr);
        }

        public override void Serialize(BinaryWriter b) {
            b.Write(Data.ToBinary());
        }


        public override void Deserialize(BinaryReader rdr) {
            Read(rdr);
        }

        public void Read(BinaryReader b) {
            Data = System.DateTime.FromBinary(b.ReadInt64());
        }

        public override int CompareTo(object obj) {
            var o = obj as DateTime;

            if(o == null) throw new ArgumentException("Comparisions must have the same type");

            return Data.CompareTo(o.Data);
        }


        public override object GetValue() {
            return Data;
        }

        public override string ToString() {
            return Data.ToString();
        }
    }
}
