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

        public DataColumn[] Columns;
        public DiskTree<Number, DataRow> Rows; 

        protected FileStream Stream;

        public Table(string name, DataColumn[] c) {
            Name = name;
            Columns = c;

            Rows = new DiskTree<Number, DataRow>(name + ".idx.bin", BucketSize, CacheSize);
            Rows.OnDataRead += v => { v.Table = this; };

            Stream = new FileStream(Name + ".table.bin", FileMode.Create);
            Counter = 0;

            Init();
            Write();
        }

        public Table(string name) {
            Name = name;
            Stream = new FileStream(Name + ".table.bin", FileMode.OpenOrCreate);

            Rows = new DiskTree<Number, DataRow>(name + ".idx.bin", BucketSize, CacheSize);
            Rows.OnDataRead += v => { v.Table = this; };

            Read();
        }

        protected void Init() {
            foreach (var _c in Columns) {
                _c.Table = this;
                _c.CreateIndex();
            }

        }

        public void Insert(IDataType[] data) {

            if(data.Length != Columns.Length) throw new ArgumentException("Data columns do not match table columns");

            for (var i = 0; i < data.Length; i++) {
                if(data[i].GetType() != Columns[i].Type)
                    throw new ArgumentException("Type mismatch between given data and table. Column " + i + " should be of type " + Columns[i].DataType);
            }

            var r = new DataRow {
                Table = this,
                Data = data,
                Index = new Number(Counter++)
            };

            Rows.Insert(r.Index, r);

            for (var i = 0; i < Columns.Length; i++) {
                if(Columns[i].Index != null)
                    Columns[i].Index.Add(r.Data[i], r.Index);
            }

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

            var c = rdr.ReadInt32();
            Columns = new DataColumn[c];

            for (var i = 0; i < c; i++) {
                var _c = Columns[i] = new DataColumn{ Table = this };
                _c.Deserialize(rdr);

                if (_c.HasIndex) {
                    _c.CreateIndex();
                    _c.Index.Read();
                }
            }

            FileHeaderOffset = Stream.Position;

            // read in dynamics
            Counter = rdr.ReadInt32();
        }

        public void Commit() {
            Rows.Write();

            foreach (var c in Columns)
                c.Commit();

            Stream.Position = FileHeaderOffset;
            var wrtr = new BinaryWriter(Stream);
            wrtr.Write(Counter);
        }

        public override string ToString() {
            var sb = new StringBuilder();

            foreach (var c in Columns)
                sb.Append(c.Name).Append(';');

            sb.Append('\n');

            foreach (var r in Rows) {
                foreach (var c in r.Value.Data)
                    sb.Append(c).Append(';');
                sb.Append('\n');
            }

            return sb.ToString();
        }

        public void Dispose() {
            Stream.Dispose();
            Rows.Dispose();
            foreach (var c in Columns)
                c.Dispose();
        }
    }
}
