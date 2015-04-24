using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Storage;
using HybreDb.Tables;

namespace HybreDb.Relational {
    /// <summary>
    /// Defines an attribute within a relation
    /// </summary>
    public class RelationAttribute : IByteSerializable {

        /// <summary>
        /// Attribute name
        /// </summary>
        public string Name;

        /// <summary>
        /// DataType of the attribute
        /// </summary>
        public DataTypes.Types DataType;
        
        /// <summary>
        /// Relation containing the attribute
        /// </summary>
        public Relation Relation;

        protected RelationAttribute(Relation rel) {
            Relation = rel;
        }

        /// <summary>
        /// Definition constructor
        /// </summary>
        public RelationAttribute(string name, DataTypes.Types t) {
            Name = name;
            DataType = t;
        }

        public RelationAttribute(Relation rel, BinaryReader rdr) : this(rel) {
            Deserialize(rdr);
        }

        #region Serialization
        public void Serialize(BinaryWriter wrtr) {
            wrtr.Write(Name);
            wrtr.Write((byte)DataType);
        }

        public void Deserialize(BinaryReader rdr) {
            Name = rdr.ReadString();
            DataType = (DataTypes.Types) rdr.ReadByte();
        }
        #endregion
    }
}
