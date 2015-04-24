using System;
using System.IO;
using HybreDb.Storage;

namespace HybreDb.BPlusTree.DataTypes {
    /// <summary>
    /// Required interface for datatypes within the tree
    /// </summary>
    public interface IDataType : IComparable, IByteSerializable { }

}
