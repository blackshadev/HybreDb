using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Storage;

namespace HybreDb.Tables {
    public class DataColumn : ITreeSerializable {
        public Type Type {
            get { return DataType.GetSystemType(); }
        }

        public DataType.Types DataType;
        public string Name;

        public void Serialize(System.IO.BinaryWriter wrtr) {
            wrtr.Write((byte)DataType);
            wrtr.Write(Name);
        }

        public void Deserialize(System.IO.BinaryReader rdr) {
            DataType = (DataType.Types) rdr.ReadByte();
            Name = rdr.ReadString();
        }

    }
}
