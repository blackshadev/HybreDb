﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Storage;
using HybreDb.Tables.Types;
using Newtonsoft.Json;

namespace HybreDb.Tables {
    /// <summary>
    ///     A DataRow within the database
    /// </summary>
    [JsonConverter(typeof (DataRowSerializer))]
    public class DataRow : IByteSerializable, IEnumerable<IDataType> {
        /// <summary>
        ///     Data within this DataRow.
        /// </summary>
        protected IDataType[] Data;

        /// <summary>
        ///     Unique index of the row.
        /// </summary>
        /// <remarks>Unique within the table</remarks>
        public Number Index;

        /// <summary>
        ///     Table holding this DataRow
        /// </summary>
        public Table Table;

        public DataRow() {}

        public DataRow(Table t, Number idx) {
            Table = t;
            Index = idx;
        }

        public DataRow(Table t, Number idx, IDataType[] d) : this(t, idx) {
            Data = d;
        }

        public int Count {
            get { return Data.Length; }
        }

        /// <summary>
        ///     Accesses the data within the row by index
        /// </summary>
        /// <param name="i">Index of the data object</param>
        public IDataType this[int i] {
            get { return Data[i]; }
            internal set { Data[i] = value; }
        }

        /// <summary>
        ///     Accesses the data by column name
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
            foreach (IDataType r in Data)
                r.Serialize(wrtr);
        }

        public void Deserialize(BinaryReader rdr) {
            Index = new Number(rdr);

            Data = new IDataType[rdr.ReadInt32()];
            for (int i = 0; i < Data.Length; i++) {
                Data[i] = Table.Columns[i].DataType.CreateType(rdr);
            }
        }

        #endregion

        public IEnumerator<IDataType> GetEnumerator() {
            return ((IEnumerable<IDataType>) Data).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /// <summary>
        ///     Simple string representation for debug purpouses
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            var sb = new StringBuilder();
            for (int i = 0; i < Data.Length; i++)
                sb.Append(Table.Columns[i].Name).Append(": ").Append(Data[i]).Append(", ");

            sb.Length -= 2;

            return sb.ToString();
        }
    }
}