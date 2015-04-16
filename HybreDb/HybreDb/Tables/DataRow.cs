using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HybreDb.Storage;
using HybreDb.BPlusTree.DataTypes;

namespace HybreDb.Tables {
    public class DataRow : ITreeSerializable {
        public Table Table;
        public IDataType[] Data;
        public Number Index;

        public void Serialize(BinaryWriter wrtr) {
            Index.Serialize(wrtr);

            wrtr.Write(Data.Length);
            foreach(var r in Data)
                r.Serialize(wrtr);
        }

        public void Deserialize(BinaryReader rdr) {
            Index = new Number(rdr);

            Data = new IDataType[rdr.ReadInt32()];
            for(var i = 0; i < Data.Length; i++) {
                Data[i] = Table.Columns[i].DataType.CreateType(rdr);
            }
        }
    }
}
