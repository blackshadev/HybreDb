using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Storage;
using HybreDb.Tables;
using HybreDb.Tables.Types;

namespace HybreDb.Relational {


    public enum RelationType {
        Unknown = 0,
        DirectedRelation = 1,
        UndirectedRelation = 2
    };

    public abstract class Relation : IByteSerializable, IEnumerable<KeyValuePair<NumberPair, DataRow>>, IDisposable {
        protected Database Database;

        public abstract RelationType RelationType { get; }

        /// <summary>
        ///     Relation to which the relation goes
        /// </summary>
        public Table Destination;

        /// <summary>
        ///     Name of the relation
        /// </summary>
        public string Name;

        /// <summary>
        ///     Table from which the relation originates
        /// </summary>
        public Table Source;

        /// <summary>
        ///     Data of the relations with as the key a NumberPair of the records forming the relation and the data additional data
        ///     belonging to this relation
        /// </summary>
        public Table Table;


        /// <summary>
        ///     Creates a new relation between given tables
        /// </summary>
        /// <param name="name">Name of the relation</param>
        /// <param name="src">Source table of the relation</param>
        /// <param name="dest">Destination table of the relation</param>
        /// <param name="type">Relation type</param>
        protected Relation(string name, Table src, Table dest) {
            if (src.Database != dest.Database)
                throw new ArgumentException("Tables must reside in the same database");

            Name = name;
            Database = src.Database;

            Source = src;
            Destination = dest;
            
        }

        protected Relation(Database db, BinaryReader rdr) {
            Database = db;
            Deserialize(rdr);

            Table = new Table(Database, Source.Name + "." + Name + "." + Destination.Name);
        }



        /// <summary>
        ///     Iterates over the relations returning a tuple of the source row, destination row and the relation's data
        /// </summary>
        public IEnumerable<Tuple<DataRow, DataRow, DataRow>> Data {
            get {
                return
                    this.Select(
                        kvp =>
                            new Tuple<DataRow, DataRow, DataRow>(Source[((Number)kvp.Value[1])],
                                Destination[((Number)kvp.Value[2])], kvp.Value));
            }
        }

        #region Disposing
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing) {
            Table.Dispose();
            Table = null;
            Source = null;
            Destination = null;
            Database = null;
        }
        #endregion

        #region enumerator
        public IEnumerator<KeyValuePair<NumberPair, DataRow>> GetEnumerator() {
            return
                Table.Rows.Select(rel => new KeyValuePair<NumberPair, DataRow>((NumberPair)rel.Value[0], rel.Value))
                    .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        #endregion


        /// <summary>
        ///     Commit any changes in the B+ Tree
        /// </summary>
        public void Commit() {
            Table.Commit();
        }

        public void Drop() {
            Table.Drop();
        }


        #region Serialisation

        public void Serialize(BinaryWriter wrtr) {
            wrtr.Write((byte)RelationType);
            wrtr.Write(Name);

            wrtr.Write(Source.Name);

            wrtr.Write(Destination.Name);
        }

        public void Deserialize(BinaryReader rdr) {
            Name = rdr.ReadString();
            Source = Database[rdr.ReadString()];
            Destination = Database[rdr.ReadString()];
        }
        #endregion

        public abstract void Add(Number a, Number b, IDataType[] dat);

        // TODO why does this return one datarow instead of multiple?
        public abstract DataRow Get(Number a, Number b);



        public static Relation Create(RelationType type, string name, Table src, Table dest, DataColumn[] cols) {
            switch(type) {
                case RelationType.DirectedRelation: return new DirectedRelation(name, src, dest, cols);
                case RelationType.UndirectedRelation: return new UndirectedRelation(name, src, dest, cols);

            }

            throw new FormatException("Unsupported relation type");
        }

        public static Relation Create(Database db, BinaryReader rdr) {
            var type = (RelationType)rdr.ReadByte();
            switch(type) {
                case RelationType.DirectedRelation: return new DirectedRelation(db, rdr);
                case RelationType.UndirectedRelation: return new UndirectedRelation(db, rdr);

            }


            throw new FormatException("Unsupported relation type");
        }
    }



   
}