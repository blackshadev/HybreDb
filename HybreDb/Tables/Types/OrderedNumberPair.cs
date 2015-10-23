using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HybreDb.Tables.Types {
    /// <summary>
    /// UnorderedNumberPair is a numberpair where (A,B)==(B,A)
    /// This is achieved by making the A always the smallest of the 2 and B always the biggest
    /// </summary>
    public class OrderedNumberPair : NumberPair {

        public OrderedNumberPair() { }

        public OrderedNumberPair(Tuple<int, int> a) : base(
            new Tuple<int, int>(a.Item1 < a.Item2 ? a.Item1 : a.Item2, a.Item1 < a.Item2 ? a.Item2 : a.Item1)
        ) { }

        public OrderedNumberPair(BinaryReader rdr) : base(rdr) { }

        public OrderedNumberPair(Number a, Number b) : base(a < b ? a : b, a < b ? b : a) { }

        protected override void Read(BinaryReader b) {
            base.Read(b);
            Number tmp;
            if (A > B) { tmp = A; A = B; B = tmp; }
        }
    }
}
