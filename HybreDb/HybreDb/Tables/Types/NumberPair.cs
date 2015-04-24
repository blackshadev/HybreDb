using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybreDb.Tables.Types {
    public class NumberPair : DataType {

        public Number A { get; protected set; }
        public Number B { get; protected set; }


        public NumberPair() { }

        public NumberPair(BinaryReader rdr) : base(rdr) { }

        public NumberPair(Number a, Number b) {
            A = a;
            B = b;
        }

        public override void Serialize(BinaryWriter b) {
            A.Serialize(b);
            B.Serialize(b);
        }

        public override void Deserialize(BinaryReader b) {
            A = new Number(b);
            B = new Number(b);
        }

        public override int CompareTo(object obj) {
            var n = obj as NumberPair;
            
            if(n == null) throw new ArgumentException("obj is not a NumberPair object");

            var a = A.CompareTo(n.A);
            return a != 0 ? a : B.CompareTo(n.B);
        }
    }
}
