using System.IO;
using HybreDb.Storage;

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