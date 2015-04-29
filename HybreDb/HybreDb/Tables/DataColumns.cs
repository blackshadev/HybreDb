using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Storage;

namespace HybreDb.Tables {
    public class DataColumns : IEnumerable<DataColumn>, IByteSerializable, IDisposable {

        public int Length {
            get { return Columns.Length; }
        }

        protected DataColumn[] Columns;
        protected Dictionary<string, int> ByName;
        protected List<int> IndexedColumnsList;

        protected Table Table;
        
        /// <summary>
        /// Gives all columns which have a index on them with there data index.
        /// </summary>
        public IEnumerable<KeyValuePair<int, DataColumn>> IndexColumns {
            get {
                return IndexedColumnsList.Select(c => new KeyValuePair<int, DataColumn>(c, Columns[c]));
            }
        }


        public DataColumns(Table t) { Table = t; }

        /// <summary>
        /// Creates DataColumns with given DataColumns in col
        /// </summary>
        /// <param name="t">Bound table</param>
        /// <param name="cols">Columns to add</param>
        public DataColumns(Table t, DataColumn[] cols) : this(t) {
            Columns = cols;

            IndexedColumnsList = new List<int>(cols.Length);
            ByName = new Dictionary<string, int>(cols.Length, StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < cols.Length; i++) {
                ByName.Add(cols[i].Name, i);
                if (!cols[i].HasIndex) continue;
                
                IndexedColumnsList.Add(i);
                cols[i].Table = t;
                cols[i].CreateIndex();
            }
        }

        /// <summary>
        /// Creates existing DataColumns based on the given BinaryReader
        /// </summary>
        /// <param name="t"></param>
        /// <param name="rdr"></param>
        public DataColumns(Table t, BinaryReader rdr) : this(t) {
            Deserialize(rdr);

            foreach (var c in IndexColumns)
                c.Value.Index.Read();
            
        }

        /// <summary>
        /// Gets a DataColumn based on it's name
        /// </summary>
        /// <param name="x">DataColumn name</param>
        public DataColumn this[string x] {
            get { return Columns[ByName[x]]; }
        }

        /// <summary>
        /// Gets a DataColumn based on it's data index
        /// </summary>
        /// <param name="x">Data index of the column</param>
        public DataColumn this[int x] {
            get { return Columns[x]; }
        }

        /// <summary>
        /// Gets the index of a given column name
        /// </summary>
        /// <param name="name">Name of the column</param>
        /// <returns>Ordinal index within data items</returns>
        public int GetIndex(string name) {
            return ByName[name];
        }

        #region Serialisation
        public void Serialize(BinaryWriter wrtr) {
            wrtr.Write(Columns.Length);

            foreach(var c in Columns)
                c.Serialize(wrtr);
        }

        public void Deserialize(BinaryReader rdr) {
            var l = rdr.ReadInt32();

            Columns = new DataColumn[l];
            ByName = new Dictionary<string, int>(l);
            IndexedColumnsList = new List<int>(l);


            for (var i = 0; i < l; i++) {
                Columns[i] = new DataColumn(Table, rdr);
                ByName.Add(Columns[i].Name, i);
                
                if (!Columns[i].HasIndex) continue;

                IndexedColumnsList.Add(i);
                Columns[i].CreateIndex();
            }

        }
        #endregion

        public IEnumerator<DataColumn> GetEnumerator() {
            return ((IEnumerable<DataColumn>)Columns).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /// <summary>
        /// Disposes all columns
        /// </summary>
        public void Dispose() {
            foreach (var c in Columns)
                c.Dispose();
        }

        public int IndexOf(string colName) {
            return ByName[colName];
        }
    }
}
