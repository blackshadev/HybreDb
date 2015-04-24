using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Tables;

namespace HybreDb.Relational {
    public class RelationData : DataType {
        public Relation Relation;

        protected IDataType[] Data;

        public RelationData() {}

        public RelationData(IDataType[] d) {
            Data = d;
        }

        public RelationData(BinaryReader rdr) : base(rdr) {}

        public override void Serialize(BinaryWriter b) {
            b.Write(Data.Length);

            foreach (var d in Data)
                d.Serialize(b);
            
        }

        public override void Deserialize(BinaryReader b) {
            var l = b.ReadInt32();
            Data = new IDataType[l];

            for (var i = 0; i < l; i++)
                Data[i] = Relation.Attributes[i].DataType.CreateType(b);

        }

        public override string ToString() {
            var sb = new StringBuilder();
            for (var i = 0; i < Relation.Attributes.Count; i++) {
                sb.Append(Relation.Attributes[i].Name + ": " + Data[i]+ ", ");
            }

            sb.Length -= 2;

            return "{" + sb + "}";
        }

        /// <summary>
        /// RelationalData cannot be used as Key data
        /// </summary>
        public override int CompareTo(object obj) {
            throw new NotImplementedException();
        }
    }
}
