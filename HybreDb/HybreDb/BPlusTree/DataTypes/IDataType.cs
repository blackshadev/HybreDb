using System;
using HybreDb.Storage;

namespace HybreDb.BPlusTree.DataTypes {
    public interface IDataType : IComparable, ITreeSerializable { }
}
