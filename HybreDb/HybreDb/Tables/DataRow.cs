using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HybreDb.Storage;
using HybreDb.BPlusTree.DataTypes;

namespace HybreDb.Tables {

    /// <summary>
    /// A DataRow within the database
    /// </summary>
    public class DataRow : ITreeSerializable {
        /// <summary>
        /// Table holding this DataRow
        /// </summary>
        public Table Table;
        
        /// <summary>
        /// Unique index of the row.
        /// </summary>
        /// <remarks>Unique within the table</remarks>
        public Number Index;
        
        /// <summary>
        /// Data within this DataRow.
        /// </summary>
        public IDataType[] Data;

        public void Delete() {
            Table.Remove(Index);
        }

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

        public override string ToString() {

            var sb = new StringBuilder();
            for (var i = 0; i < Data.Length; i++)
                sb.Append(Table.Columns[i].Name).Append(": ").Append(Data[i]).Append(", ");

            sb.Length -= 2;

            return sb.ToString();
        }
    }
}
