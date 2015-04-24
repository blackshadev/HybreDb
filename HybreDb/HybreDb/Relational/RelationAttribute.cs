using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Storage;
using HybreDb.Tables;

namespace HybreDb.Relational {
    public class RelationAttribute : IByteSerializable {

        public string Name;
        public DataTypes.Types DataType;
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

        public void Serialize(BinaryWriter wrtr) {
            wrtr.Write(Name);
            wrtr.Write((byte)DataType);
        }

        public void Deserialize(BinaryReader rdr) {
            Name = rdr.ReadString();
            DataType = (DataTypes.Types) rdr.ReadByte();
        }
    }
}
