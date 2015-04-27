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
using HybreDb.BPlusTree.DataTypes;
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
    public class Relation : IByteSerializable, IEnumerable<KeyValuePair<NumberPair, DataRow>>, IDisposable {
        
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
        /// Data of the relations with as the key a NumberPair of the records forming the relation and the data additional data belonging to this relation
        /// </summary>
        public Table Table;

        private Database Database;


        /// <summary>
        /// Creates a new relation between given tables
        /// </summary>
        /// <param name="name">Name of the relation</param>
        /// <param name="src">Source table of the relation</param>
        /// <param name="dest">Destination table of the relation</param>
        /// <param name="attrs">Data attributes which each item in the relation holds</param>
        public Relation(string name, Table src, Table dest, DataColumn[] attrs) {
            if(src.Database != dest.Database)
                throw new ArgumentException("Tables are not of the same database");
            
            Name = name;
            Database = src.Database;

            Source = src;
            Destination = dest;

            var cols = new DataColumn[attrs.Length + 3];
            Array.Copy(attrs, 0, cols, 3, attrs.Length);

            cols[0] = new DataColumn(".rel", DataTypes.Types.NumberPair, true);
            cols[1] = new DataColumn(".rel.src", DataTypes.Types.Number, true);
            cols[2] = new DataColumn(".rel.dest", DataTypes.Types.Number, true);


            Table = new Table(Database, Source.Name + "." + Name + "." + Destination.Name, cols);

            Commit();
        }

        /// <summary>
        /// Creates a already existsing relation by reading it in from the BinaryReader
        /// </summary>
        public Relation(Database db, BinaryReader rdr) {
            Database = db;
            Deserialize(rdr);

            Table = new Table(Database, Source.Name + "." + Name + "." + Destination.Name);
            Table.Read();
        }

        /// <summary>
        /// Adds a new item to the relation.
        /// </summary>
        /// <param name="a">Primary key in the source table</param>
        /// <param name="b">Primary key in the destinayion table</param>
        /// <param name="dat">Data belonging to the relation, must match relation's attributes</param>
        public void Add(Number a, Number b, IDataType[] dat) {
            var d = new IDataType[dat.Length + 3];
            Array.Copy(dat, 0, d, 3, dat.Length);

            d[0] = new NumberPair(a, b);
            d[1] = a;
            d[2] = b;

            Table.Insert(d);
        }

        /// <summary>
        /// Gets the relation data of the relation between two given records
        /// </summary>
        /// <param name="a">Primary key in the source table</param>
        /// <param name="b">Primary key in the destination table</param>
        public DataRow Get(Number a, Number b) {
            var nums = new NumberPair(a, b);

            return Table.Find(new KeyValuePair<string, object>(".rel", nums)).First();
        }

        /// <summary>
        /// Commit any changes in the B+ Tree
        /// </summary>
        public void Commit() { Table.Commit(); }

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

        }
        #endregion

        protected static string RelationNameFormat(Relation r) {
            return  r.Database.GetPath(r.Source.Name + "." + r.Name + "." + r.Destination.Name + ".idx.bin");
        }

        /// <summary>
        /// Iterates over the relations returning a tuple of the source row, destination row and the relation's data 
        /// </summary>
        public IEnumerable<Tuple<DataRow, DataRow, DataRow>> Data {
            get {
                return this.Select(kvp => new Tuple<DataRow, DataRow, DataRow>(Source[((Number)kvp.Value[1])], Destination[((Number)kvp.Value[2])], kvp.Value));
            }
        }

        public IEnumerator<KeyValuePair<NumberPair, DataRow>> GetEnumerator() {
            return Table.Rows.Select(rel => new KeyValuePair<NumberPair, DataRow>((NumberPair)rel.Value[0], rel.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Dispose() {
            
            Table.Dispose();
            Table = null;
            Source = null;
            Destination = null;
            Database = null;
        }
    }
}
