using System;
using System.IO;
using HybreDb.Storage;

namespace HybreDb.BPlusTree.DataTypes {
    public interface IDataType : IComparable, IByteSerializable { }

    /// <summary>
    /// Abstract class for useable datatypes within the tree.
    /// Every DataType can be used as key and as value.
    /// The extended classes of DataType, must have both the empty constructor as well as the constructor with an binary reader
    /// </summary>
    public abstract class DataType : IDataType {

        protected DataType() { }
        
        /// <summary>
        /// Constructor used to deserialize an object
        /// </summary>
        /// <param name="rdr"></param>
        protected DataType(BinaryReader rdr) { Deserialize(rdr); }

        public abstract void Serialize(BinaryWriter b);
        
        public abstract void Deserialize(BinaryReader b);
        
        public abstract int CompareTo(object obj);
    }
}
