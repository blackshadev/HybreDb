using System;
using System.IO;
using HybreDb.Storage;

namespace HybreDb.BPlusTree.DataTypes {
    public interface IDataType : IComparable, ITreeSerializable { }

    public abstract class DataType : IDataType {

        protected DataType() { }
        public DataType(BinaryReader rdr) { Deserialize(rdr); }

        public abstract void Serialize(BinaryWriter b);
        public abstract void Deserialize(BinaryReader b);
        public abstract int CompareTo(object obj);
    }
}
