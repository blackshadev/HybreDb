﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Storage;
using HybreDb.Tables;

namespace HybreDb.Test {
    public class TestData : IByteSerializable {

        public int Key;
        public string Value;

        public TestData() {}

        public TestData(BinaryReader rdr) {
            Deserialize(rdr);
        }

        public void Serialize(BinaryWriter wrtr) {
            wrtr.Write(Key);
            wrtr.Write(Value);
        }

        public void Deserialize(BinaryReader rdr) {
            Key = rdr.ReadInt32();
            Value = rdr.ReadString();
        }

        public override string ToString() {
            return Value;
        }
    }
}
