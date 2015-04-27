using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybreDb.Tables.Types {
    /// <summary>
    /// Datatype containing two Numbers
    /// </summary>
    public class NumberPair : DataType {

        public Number A { get; protected set; }
        public Number B { get; protected set; }


        public NumberPair() { }

        public NumberPair(Tuple<int, int> a) {
            A = a.Item1;
            A = a.Item2;
        }

        public NumberPair(BinaryReader rdr) : base(rdr) { }

        public NumberPair(Number a, Number b) {
            A = a;
            B = b;
        }

        #region Serialisation
        public override void Serialize(BinaryWriter b) {
            A.Serialize(b);
            B.Serialize(b);
        }

        public override void Deserialize(BinaryReader b) {
            A = new Number(b);
            B = new Number(b);
        }
        #endregion

        /// <summary>
        /// Compares two instances of NumberPairs, first compares on the first number, if that is equal use the second number.
        /// </summary>
        public override int CompareTo(object obj) {
            var n = obj as NumberPair;
            
            if(n == null) throw new ArgumentException("obj is not a NumberPair object");

            var a = A.CompareTo(n.A);
            return a != 0 ? a : B.CompareTo(n.B);
        }
    }
}
