using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace HybreDb.Tables.Types {
    /// <summary>
    ///     Datatype containing two Numbers
    /// </summary>
    public class NumberPair : DataType {
        public Number A { get; protected set; }
        public Number B { get; protected set; }

        public NumberPair() {}

        public NumberPair(Tuple<int, int> a) {
            A = a.Item1;
            B = a.Item2;
        }

        public NumberPair(BinaryReader rdr) {
            Read(rdr);
        }

        public NumberPair(Number a, Number b) {
            A = a;
            B = b;
        }      

        public override int GetHashCode() {
            return A.GetHashCode() ^ B.GetHashCode();
        }

        public override bool Equals(object obj) {
            var n = obj as NumberPair;
            return n.A == A && n.B == B;
        }

        /// <summary>
        ///     Compares two instances of NumberPairs, first compares on the first number, if that is equal use the second number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int CompareTo(object obj) {
            var n = obj as NumberPair;

            if (n == null) throw new ArgumentException("obj is not a NumberPair object");

            int a = A.CompareTo(n.A);
            return a != 0 ? a : B.CompareTo(n.B);
        }

        public override object GetValue() {
            return new[] {A.GetValue(), B.GetValue()};
        }

        public override string ToString() {
            return "(" + A + "," + B + ")";
        }

        #region Serialisation

        public override void Serialize(BinaryWriter b) {
            A.Serialize(b);
            B.Serialize(b);
        }


        public override void Deserialize(BinaryReader b) {
            Read(b);
        }

        protected virtual void Read(BinaryReader b) {
            A = new Number(b);
            B = new Number(b);
        }

        #endregion
    }
}