using System;
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

namespace HybreDb.Relational {
    /// <summary>
    /// Class implementing a relation between two tables
    /// It consists both of the definition and the actual data.
    /// </summary>
    /// <remarks>
    /// All Relations in HybreDb are directional. 
    /// Undirectional relations are just relations who insert and delete data for both directions
    /// </remarks>
    public class Relation : IByteSerializable, IEnumerable<KeyValuePair<NumberPair, RelationData>>, IDisposable {
        
        /// <summary>
        /// Name of the relation
        /// </summary>
        public string Name;

        /// <summary>
        /// Table from which the relation originates
        /// </summary>
        public Table Source;

        /// <summary>
        /// Relation to which the relation goes
        /// </summary>
        public Table Destination;

        /// <summary>
        /// 
        /// </summary>
        public RelationAttributes Attributes { get; protected set; }
        /// <summary>
        /// Data of the relations with as the key a NumberPair of the records forming the relation and the data additional data belonging to this relation
        /// </summary>
        public DiskTree<NumberPair, RelationData> Rows;

        private Database Database;


        /// <summary>
        /// Creates a new relation between given tables
        /// </summary>
        /// <param name="name">Name of the relation</param>
        /// <param name="src">Source table of the relation</param>
        /// <param name="dest">Destination table of the relation</param>
        /// <param name="attrs">Data attributes which each item in the relation holds</param>
        public Relation(string name, Table src, Table dest, RelationAttribute[] attrs) {
            if(src.Database != dest.Database)
                throw new ArgumentException("Tables are not of the same database");
            
            Name = name;
            Database = src.Database;

            Source = src;
            Destination = dest;
            Attributes = new RelationAttributes(this, attrs);

            CreateTree();
            Commit();
        }

        /// <summary>
        /// Creates a already existsing relation by reading it in from the BinaryReader
        /// </summary>
        public Relation(Database db, BinaryReader rdr) {
            Database = db;
            Deserialize(rdr);

            CreateTree();
            Rows.Read();
        }


        private void CreateTree() {
            Rows = new DiskTree<NumberPair, RelationData>(RelationNameFormat(this));
            Rows.OnDataRead += v => { v.Relation = this; };
        }


        /// <summary>
        /// Adds a new item to the relation.
        /// </summary>
        /// <param name="a">Primary key in the source table</param>
        /// <param name="b">Primary key in the destinayion table</param>
        /// <param name="dat">Data belonging to the relation, must match relation's attributes</param>
        public void Add(Number a, Number b, RelationData dat) {
            var nums = new NumberPair(a, b);

            Rows.Insert(nums, dat);
        }

        /// <summary>
        /// Gets the relation data of the relation between two given records
        /// </summary>
        /// <param name="a">Primary key in the source table</param>
        /// <param name="b">Primary key in the destination table</param>
        public RelationData Get(Number a, Number b) {
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

            Attributes.Serialize(wrtr);

        }

        public void Deserialize(BinaryReader rdr) {
            Name = rdr.ReadString();
            Source = Database[rdr.ReadString()];
            Destination = Database[rdr.ReadString()];
            Attributes = new RelationAttributes(this, rdr);

        }
        #endregion

        protected static string RelationNameFormat(Relation r) {
            return  r.Database.GetPath(r.Source.Name + "." + r.Name + "." + r.Destination.Name + ".idx.bin");
        }

        /// <summary>
        /// Iterates over the relations returning a tuple of the source row, destination row and the relation's data 
        /// </summary>
        public IEnumerable<Tuple<DataRow, DataRow, RelationData>> Data {
            get {
                return this.Select(kvp => new Tuple<DataRow, DataRow, RelationData>(Source[kvp.Key.A], Destination[kvp.Key.B], kvp.Value));
            }
        }

        public IEnumerator<KeyValuePair<NumberPair, RelationData>> GetEnumerator() {
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
