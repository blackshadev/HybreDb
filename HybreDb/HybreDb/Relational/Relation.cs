﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HybreDb.BPlusTree;
using HybreDb.Storage;
using HybreDb.Tables;
using HybreDb.Tables.Types;
using TDataType = HybreDb.Tables.Types.Text;

namespace HybreDb.Relational {
    public class Relation : IByteSerializable, IEnumerable<KeyValuePair<NumberPair, TDataType>>, IDisposable {

        public Database Database;

        public string Name;
        public Table Source;
        public Table Destination;

        public DiskTree<NumberPair, TDataType> Rows; 


        protected Relation(Database db) {
            Database = db;
        }

        public Relation(string name, Table src, Table dest) {
            if(src.Database != dest.Database)
                throw new ArgumentException("Tables are not of the same database");
            
            Name = name;
            Database = src.Database;

            Source = src;
            Destination = dest;

            Rows = new DiskTree<NumberPair, TDataType>(RelationNameFormat(this));
            Commit();
        }

        public Relation(Database db, BinaryReader rdr) :this(db) {
            Deserialize(rdr);
        }


        public void Add(Number a, Number b, TDataType dat) {
            var nums = new NumberPair(a, b);

            Rows.Insert(nums, dat);
        }

        public TDataType Get(Number a, Number b) {
            var nums = new NumberPair(a, b);

            return Rows.Get(nums);
        }

        /// <summary>
        /// Commit any changes in the B+ Tree
        /// </summary>
        public void Commit() { Rows.Write(); }

        #region Serialisation
        public void Serialize(BinaryWriter wrtr) {
            wrtr.Write(Name);

            wrtr.Write(Source.Name);

            wrtr.Write(Destination.Name);

        }

        public void Deserialize(BinaryReader rdr) {
            Name = rdr.ReadString();
            Source = Database[rdr.ReadString()];
            Destination = Database[rdr.ReadString()];

            Rows = new DiskTree<NumberPair, Text>(RelationNameFormat(this));
            Rows.Read();
        }
        #endregion

        protected static string RelationNameFormat(Relation r) {
            return  r.Database.GetPath(r.Source.Name + "." + r.Name + "." + r.Destination.Name + ".idx.bin");
        }

        public IEnumerable<Tuple<DataRow, DataRow, TDataType>> Data {
            get {
                return this.Select(kvp => new Tuple<DataRow, DataRow, Text>(Source[kvp.Key.A], Destination[kvp.Key.B], kvp.Value));
            }
        }

        public IEnumerator<KeyValuePair<NumberPair, TDataType>> GetEnumerator() {
            return Rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Dispose() {
            Rows.Dispose();
            Rows = null;
            Source = null;
            Destination = null;
            Database = null;
        }
    }
}
