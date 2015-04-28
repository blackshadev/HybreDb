using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.BPlusTree;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Relational;
using HybreDb.Tables.Types;

namespace HybreDb.Tables {
    public class Table: IDisposable {
        public const int BucketSize = 64;
        public const int CacheSize = 64;


        public string Name { get; protected set; }

        public Database Database { get; protected set; }
        protected int Counter;

        /// <summary>
        /// Offset within the file the counter field
        /// </summary>
        protected long CounterOffset;

        /// <summary>
        /// Offset within the file where the relations begins.
        /// </summary>
        protected long RelationsOffset;

        /// <summary>
        /// Column structure of the data contained in the table
        /// </summary>
        public DataColumns Columns;
        
        /// <summary>
        /// Actual data stored in a Disk based B+ Tree
        /// </summary>
        public DiskTree<Number, DataRow> Rows;

        /// <summary>
        /// Relations to other tables
        /// </summary>
        public Relations Relations;

        protected FileStream Stream;

        /// <summary>
        /// Creates a new table base on the given DataColumns
        /// </summary>
        /// <param name="name">Table name</param>
        /// <param name="c">Columns</param>
        public Table(Database db, string name, DataColumn[] c) {
            Database = db;
            Name = name;
            Columns = new DataColumns(this, c);
            Relations = new Relations(this);

            Rows = new DiskTree<Number, DataRow>(Database.GetPath(name) + ".idx.bin", BucketSize, CacheSize);
            Rows.OnDataRead += v => { v.Table = this; };

            Stream = DbFile.Open(Database.GetPath(Name) + ".table.bin" );
            Counter = 0;

            Write();
        }


        /// <summary>
        /// Creates a existing Table by reading it in from file.
        /// </summary>
        /// <param name="name"></param>
        public Table(Database db, string name) {
            Database = db;
            Name = name;
            Stream = DbFile.Open(Database.GetPath(name) + ".table.bin");
            Relations = new Relations(this);

            Rows = new DiskTree<Number, DataRow>(Database.GetPath(name + ".idx.bin"), BucketSize, CacheSize);
            Rows.OnDataRead += v => { v.Table = this; };

            Read();
        }

        /// <summary>
        /// Inserts given ordered data in the table.
        /// </summary>
        /// <param name="data">Data to insert where each index corresponts to the datacolumn</param>
        public void Insert(IDataType[] data) {

            // Check data types
            if(data.Length != Columns.Length) throw new ArgumentException("Data columns do not match table columns");
            for (var i = 0; i < data.Length; i++) {
                if(data[i].GetType() != Columns[i].Type)
                    throw new ArgumentException("Type mismatch between given data and table. Column " + i + " should be of type " + Columns[i].DataType);
            }


            var r = new DataRow(this, Counter++, data);

            Rows.Insert(r.Index, r);

            // Insert Indexes
            foreach ( var c in Columns.IndexColumns)
                    c.Value.Index.Add(r[c.Key], r.Index);
        }

        /// <summary>
        /// Writes the table definition to file, 
        /// because table definitions won't change, overwrite the changeables with commit.
        /// Also sets the CounterOffset
        /// </summary>
        public void Write() {
            Stream.Seek(0, SeekOrigin.End);
            var start = Stream.Position;

            var wrtr = new BinaryWriter(Stream);

            Columns.Serialize(wrtr);

            CounterOffset = Stream.Position;
            
            Commit();

            RelationsOffset = Stream.Position;

            Relations.Serialize(wrtr);

            wrtr.Write(start);
        }

        /// <summary>
        /// Reads in the table definition and changeable data,
        /// Also sets the CounterOffset
        /// </summary>
        public void Read() {
            Rows.Read();

            Stream.Seek(-8, SeekOrigin.End);
            var rdr = new BinaryReader(Stream);
            Stream.Position = rdr.ReadInt32();

            Columns = new DataColumns(this, rdr);

            CounterOffset = Stream.Position;

            // read in dynamics
            Counter = rdr.ReadInt32();

            RelationsOffset = Stream.Position;
        }

        /// <summary>
        /// Used to read in relations after all tables have ben read.
        /// </summary>
        internal void ReadRelations() {
            Stream.Position = RelationsOffset;

            Relations.Deserialize(new BinaryReader(Stream));
        }

        /// <summary>
        /// Commits all changes in the table to File. 
        /// These changes in the index tree and changes in the counter.
        /// Not the actual stucture of the table, use write for this purpose.
        /// </summary>
        public void Commit() {
            Rows.Write();
            
            foreach (var c in Columns)
                c.Commit();

            foreach (var r in Relations)
                r.Commit();

            Stream.Position = CounterOffset;
            var wrtr = new BinaryWriter(Stream);
            wrtr.Write(Counter);
            Stream.Flush();
        }
        

        /// <summary>
        /// Gets a row by number/int
        /// </summary>
        /// <param name="idx">Primary key of that row</param>
        /// <returns>Datarow beloning to given key</returns>
        public DataRow this[int idx] {
            get { return Rows.Get(idx); }
        }

        /// <summary>
        /// Gets data given Numbers as indexes in Rows
        /// </summary>
        /// <param name="nums">Numbers in the table</param>
        public IEnumerable<DataRow> GetData(Numbers nums) {
            return nums.Select(n => Rows.Get(n));
        }

        /// <summary>
        /// Given a condition on a column, find the rows which satisfy the given condition.
        /// </summary>
        /// <param name="condition">Keyvaluepair with the column name as key and the value which need to match as value</param>
        /// <returns>Datarows which satisfies the condition</returns>
        public IEnumerable<DataRow> Find(KeyValuePair<string, object> condition) {
            var n = FindRows(condition);
            return GetData(n);
        }

        /// <summary>
        /// Given a condition on a column, find the primary keys of the rows that satisfy the given condition
        /// </summary>
        /// <param name="condition">Keyvaluepair with the column name as key and the value which need to match as value</param>
        /// <returns>Datarows which satisfies the condition</returns>
        public Numbers FindRows(KeyValuePair<string, object> condition) {
            return Columns[condition.Key].Match(condition.Value);
        }


        public void RemoveAll(Numbers nums) {
            var arr = nums.AsArray();
            foreach(var k in arr) Remove(k);
        }

        /// <summary>
        /// Removes a row with given primary key
        /// </summary>
        /// <param name="idx">Primary key of the row to delete</param>
        public void Remove(Number idx) {
            DataRow r = null;
            var changed = Rows.Update(idx, (l, k, v) => {
                if (v == null) return false;

                r = v;

                // make sure node is big enough
                if (l.Count >= l.Capacity/4) {

                    l.Buckets.Remove(k);

                    return true;
                }

                return false;

            });

            Relations.RemoveItem(idx);

            // Not found
            if (r == null)
                throw new ArgumentException("Key value not found");
            
            // Perform manual remove
            if(!changed) Rows.Remove(idx);

            foreach (var kvp in Columns.IndexColumns)
                kvp.Value.Index.Remove(r[kvp.Key], idx);
           
        }

        /// <summary>
        /// Updates the data in a column from a given row
        /// </summary>
        /// <param name="idx">Row data to update</param>
        /// <param name="colName">Column name of the column within the row to uodate</param>
        /// <param name="data">New value</param>
        public void Update(Number idx, string colName, IDataType data) {
            var colIdx = Columns.IndexOf(colName);
            var col = Columns[colIdx];

            if(!col.CheckType(data))
                throw new ArgumentException("Invalid data type for column `"  + col.Name 
                    + "`. Expected `" + col.DataType.GetSystemType().Name + "` got `" 
                    + data.GetType().Name + "`");

            object oldData = null;
            Rows.Update(idx, (l, k, v) => {
                if (v == null) return false;

                oldData = v[colIdx];
                v[colIdx] = data;

                return true;
            });

            if(oldData == null) 
                throw new ArgumentException("Key value not found");

            col.Index.Remove(oldData, idx);
            col.Index.Add(data, idx);
        }


        /// <summary>
        /// A string representation which the first row as the column names and after that each entry is a row with data.
        /// </summary>
        public override string ToString() {
            var sb = new StringBuilder();

            foreach (var c in Columns)
                sb.Append(c.Name).Append(';');

            sb.Append('\n');

            foreach (var r in Rows) {
                foreach (var c in r.Value)
                    sb.Append(c).Append(';');
                sb.Append('\n');
            }

            return sb.ToString();
        }

        /// <summary>
        ///  Disposes all resources held by the table.
        /// </summary>
        public void Dispose() {
            Stream.Dispose();
            Rows.Dispose();
            Relations.Dispose();
            Columns.Dispose();
        }

    }
}
