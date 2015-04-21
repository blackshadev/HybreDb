﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.BPlusTree;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Storage;
using Newtonsoft.Json.Bson;

namespace HybreDb.Tables {
    public class Table: IDisposable {
        public const int BucketSize = 64;
        public const int CacheSize = 64;


        public string Name { get; protected set; }
        protected int Counter;

        /// <summary>
        /// Offset within the file untill the non static data
        /// </summary>
        protected long FileHeaderOffset;

        public DataColumns Columns;
        public DiskTree<Number, DataRow> Rows; 

        protected FileStream Stream;

        /// <summary>
        /// Creates a new table base on the given DataColumns
        /// </summary>
        /// <param name="name">Table name</param>
        /// <param name="c">Columns</param>
        public Table(string name, DataColumn[] c) {
            Name = name;
            Columns = new DataColumns(this, c);

            Rows = new DiskTree<Number, DataRow>(name + ".idx.bin", BucketSize, CacheSize);
            Rows.OnDataRead += v => { v.Table = this; };

            Stream = new FileStream(Name + ".table.bin", FileMode.Create);
            Counter = 0;

            Write();
        }


        /// <summary>
        /// Creates a existing Table by reading it in from file.
        /// </summary>
        /// <param name="name"></param>
        public Table(string name) {
            Name = name;
            Stream = new FileStream(Name + ".table.bin", FileMode.OpenOrCreate);

            Rows = new DiskTree<Number, DataRow>(name + ".idx.bin", BucketSize, CacheSize);
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
        /// Also sets the FileHeaderOffset
        /// </summary>
        protected void Write() {
            Stream.Position = 0;
            var wrtr = new BinaryWriter(Stream);
            wrtr.Write(Columns.Length);
            foreach (var c in Columns)
                c.Serialize(wrtr);

            FileHeaderOffset = Stream.Position;

            Commit();
        }

        /// <summary>
        /// Reads in the table definition and changeable data,
        /// Also sets the FileHeaderOffset
        /// </summary>
        protected void Read() {
            Rows.Read();

            Stream.Position = 0;
            var rdr = new BinaryReader(Stream);

            Columns = new DataColumns(this, rdr);

            FileHeaderOffset = Stream.Position;

            // read in dynamics
            Counter = rdr.ReadInt32();
        }

        /// <summary>
        /// Commits all changes in the table to File. 
        /// These changes in the index tree and changes in the counter.
        /// </summary>
        public void Commit() {
            Rows.Write();

            foreach (var c in Columns)
                c.Commit();

            Stream.Position = FileHeaderOffset;
            var wrtr = new BinaryWriter(Stream);
            wrtr.Write(Counter);
        }
        
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
        /// Given a condition on a column, find the numbers of row indices which satisfy the given condition.
        /// </summary>
        /// <param name="Condition">Keyvaluepair with the column name as key and the value which need to match as value</param>
        /// <returns>Datarows which satisfies the condition</returns>
        public IEnumerable<DataRow> Find(KeyValuePair<string, object> Condition) {
            var n = Columns[Condition.Key].Match(Condition.Value);
            return GetData(n);
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
            foreach (var c in Columns)
                c.Dispose();
        }
    }
}
