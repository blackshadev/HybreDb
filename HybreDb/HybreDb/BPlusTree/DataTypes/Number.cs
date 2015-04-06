﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Storage;

namespace HybreDb.BPlusTree.DataTypes {
    public class Number : IDataType {
        private int Data;


        public Number(int i) {
            Data = i;
        }

        public void Serialize(BinaryWriter wrtr) {
            wrtr.Write(Data);
        }

        public void Deserialize(BinaryReader rdr) {
            Data = rdr.ReadInt32();
        }

        public static implicit operator Number(int i) {
            return new Number(i);
        }

        public static implicit operator int(Number n) {
            return n.Data;
        }

        public override string ToString() {
            return Data.ToString();
        }

        public int CompareTo(object obj) {
            var n = obj as Number;
            if (n == null) throw new ArgumentException("Number cannot be compared to a non numeric");

            return Data.CompareTo(n.Data);
        }
    }
}
