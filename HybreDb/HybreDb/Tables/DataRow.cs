using System;
using System.Collections;
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
    public class DataRow : ITreeSerializable, IEnumerable<IDataType> {
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
        protected IDataType[] Data;

        public DataRow() {}

        public DataRow(Table t, Number idx) {
            Table = t;
            Index = idx;
        }

        public DataRow(Table t, Number idx, IDataType[] d) : this(t, idx) {
            Data = d;
        }

        /// <summary>
        /// Accesses the data within the row by index
        /// </summary>
        /// <param name="i">Index of the data object</param>
        public IDataType this[int i] {
            get { return Data[i]; }
            internal set { Data[i] = value; }
        }

        /// <summary>
        /// Accesses the data by column name
        /// </summary>
        /// <param name="colName">Column name</param>
        /// <returns>Data beloning in that column</returns>
        public IDataType this[string colName] {
            get { return this[Table.Columns.IndexOf(colName)]; }
            internal set { this[Table.Columns.IndexOf(colName)] = value; }
        }

        #region Serialisation
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
        #endregion


        /// <summary>
        /// Simple string representation for debug purpouses
        /// </summary>
        /// <returns></returns>
        public override string ToString() {

            var sb = new StringBuilder();
            for (var i = 0; i < Data.Length; i++)
                sb.Append(Table.Columns[i].Name).Append(": ").Append(Data[i]).Append(", ");

            sb.Length -= 2;

            return sb.ToString();
        }

        public IEnumerator<IDataType> GetEnumerator() {
            return ((IEnumerable<IDataType>)Data).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
