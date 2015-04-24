using System;
using System.IO;

namespace HybreDb.Tables.Types {
    public class Number : DataType {
        private int Data;

        public Number() {}


        public Number(int i) {
            Data = i;
        }

        public Number(BinaryReader rdr) : base(rdr) {}

        public override void Serialize(BinaryWriter wrtr) {
            wrtr.Write(Data);
        }

        public override void Deserialize(BinaryReader rdr) {
            Data = rdr.ReadInt32();
        }

        public static implicit operator Number(int i) {
            return new Number(i);
        }

        public static implicit operator int(Number n) {
            return n.Data;
        }

        /// <summary>
        /// HashCode implementation used for Numbers hashset
        /// </summary>
        public override int GetHashCode() {
            return Data.GetHashCode();
        }

        /// <summary>
        /// Equals implementation used for Numbers hashset
        /// </summary>
        public override bool Equals(object obj) {
            var n = obj as Number;
            return n != null && n.Data == Data;
        }

        public override string ToString() {
            return Data.ToString();
        }

        public override int CompareTo(object obj) {
            var n = obj as Number;
            if (n == null) throw new ArgumentException("Number cannot be compared to a non numeric");

            return Data.CompareTo(n.Data);
        }
    }
}
