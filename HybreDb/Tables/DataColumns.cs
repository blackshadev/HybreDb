﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HybreDb.Storage;

namespace HybreDb.Tables {
    public class DataColumns : IEnumerable<DataColumn>, IByteSerializable, IDisposable {
        protected Dictionary<string, int> ByName;
        protected DataColumn[] Columns;
        protected List<int> IndexedColumnsList;

        protected Table Table;


        public DataColumns(Table t) {
            Table = t;
        }

        /// <summary>
        ///     Creates DataColumns with given DataColumns in col
        /// </summary>
        /// <param name="t">Bound table</param>
        /// <param name="cols">Columns to add</param>
        public DataColumns(Table t, DataColumn[] cols) : this(t) {
            Columns = cols;

            IndexedColumnsList = new List<int>(cols.Length);
            ByName = new Dictionary<string, int>(cols.Length, StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < cols.Length; i++) {
                ByName.Add(cols[i].Name, i);
                cols[i].Table = t;

                if (!cols[i].HasIndex) continue;

                IndexedColumnsList.Add(i);
                cols[i].CreateIndex();
            }
        }

        /// <summary>
        ///     Creates existing DataColumns based on the given BinaryReader
        /// </summary>
        /// <param name="t"></param>
        /// <param name="rdr"></param>
        public DataColumns(Table t, BinaryReader rdr) : this(t) {
            Deserialize(rdr);

            foreach (DataColumn c in Columns) {
                if (c.HasIndex) c.Index.Read();
                c.Table = t;
            }
        }

        public int Length {
            get { return Columns.Length; }
        }

        /// <summary>
        ///     Gives all columns which have a index on them with there data index.
        /// </summary>
        public IEnumerable<KeyValuePair<int, DataColumn>> IndexColumns {
            get { return IndexedColumnsList.Select(c => new KeyValuePair<int, DataColumn>(c, Columns[c])); }
        }

        /// <summary>
        ///     Gets a DataColumn based on it's name
        /// </summary>
        /// <param name="x">DataColumn name</param>
        public DataColumn this[string x] {
            get { return Columns[ByName[x]]; }
        }

        /// <summary>
        ///     Gets a DataColumn based on it's data index
        /// </summary>
        /// <param name="x">Data index of the column</param>
        public DataColumn this[int x] {
            get { return Columns[x]; }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IEnumerator<DataColumn> GetEnumerator() {
            return ((IEnumerable<DataColumn>) Columns).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /// <summary>
        ///     Gets the index of a given column name
        /// </summary>
        /// <param name="name">Name of the column</param>
        /// <returns>Ordinal index within data items</returns>
        public int GetIndex(string name) {
            return ByName[name];
        }

        /// <summary>
        ///     Disposes all columns
        /// </summary>
        protected virtual void Dispose(bool disposing) {
            foreach (DataColumn c in Columns) c.Dispose();
        }

        public int IndexOf(string colName) {
            return ByName[colName];
        }

        #region Serialisation

        public void Serialize(BinaryWriter wrtr) {
            wrtr.Write(Columns.Length);

            foreach (DataColumn c in Columns)
                c.Serialize(wrtr);
        }

        public void Deserialize(BinaryReader rdr) {
            int l = rdr.ReadInt32();

            Columns = new DataColumn[l];
            ByName = new Dictionary<string, int>(l, StringComparer.OrdinalIgnoreCase);
            IndexedColumnsList = new List<int>(l);


            for (int i = 0; i < l; i++) {
                Columns[i] = new DataColumn(Table, rdr);
                ByName.Add(Columns[i].Name, i);

                if (!Columns[i].HasIndex) continue;

                IndexedColumnsList.Add(i);
                Columns[i].CreateIndex();
            }
        }

        #endregion
    }
}