using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using HybreDb.Relational;
using HybreDb.Storage;
using HybreDb.Tables;

namespace HybreDb {
    /// <summary>
    /// Database class containing the tables of a given database
    /// </summary>
    public class Database : IByteSerializable, IEnumerable<Table> {

        /// <summary>
        /// Name of the database, used as directory name
        /// </summary>
        public string Name { get; protected set; }

        protected Dictionary<string, Table> Tables;
        protected Stream Stream;

        /// <summary>
        /// Constructor for a database
        /// </summary>
        /// <param name="name">Name of the database</param>
        /// <param name="clean">Whenever or not to remove any old database with the same name</param>
        public Database(string name, bool clean=false) {
            Name = name;

            var dir = GetPath("");
            if (clean && Directory.Exists(dir))
                Directory.Delete(dir, true);

            Directory.CreateDirectory(dir);

            var fname = GetPath(new[] {name, name + ".db.bin"});
            var exists = (File.Exists(fname));

            Stream = DbFile.Open(fname);

            if (exists) {
                Read();
                return;
            }

            Tables = new Dictionary<string, Table>();
            Write();
        }


        /// <summary>
        /// Reads in the data from a file
        /// </summary>
        public void Read() {
            Stream.Seek(-8, SeekOrigin.End);
            var rdr = new BinaryReader(Stream);
            Stream.Position = rdr.ReadInt64();
            Deserialize(rdr);
        }


        /// <summary>
        /// Writes changes to a file. Does not commit the tables
        /// </summary>
        public void Write() {
            Stream.Seek(0, SeekOrigin.End);
            
            var start = Stream.Position;
            var wrtr = new BinaryWriter(Stream);

            Serialize(wrtr);

            wrtr.Write(start);
        }

        /// <summary>
        /// Adds and creates a new table within the database
        /// </summary>
        /// <param name="name">Table name</param>
        /// <param name="cols">Column definitions</param>
        /// <returns></returns>
        public Table NewTable(string name, DataColumn[] cols) {
            if(Tables.ContainsKey(name))
                throw new ArgumentException("Table named `" + name + "` already exists in database `" + Name + "`");

            var t = new Table(this, name, cols);
            Tables.Add(t.Name, t);

            Write();
            
            return t;
        }

        public Table Reopen(Table old) {
            var contains = Tables.Remove(old.Name);

            if(!contains)
                throw new ArgumentException("Database doesnt contain given table");

            var n = old.Name;
            old.Commit();
            old.Dispose();

            
            Tables[n] = new Table(this, n);
            Tables[n].ReadRelations();

            return Tables[n];
        }

        public Relation AddRelation(string relName, string srcTable, string destTable, RelationAttribute[] attrs) {

            var src = Tables[srcTable];
            var r = src.Relations.Add(relName, src, Tables[destTable], attrs);
            src.Write();
            
            return r;
        }

        #region serialisation
        public void Serialize(BinaryWriter wrtr) {
            wrtr.Write(Name);
            wrtr.Write(Tables.Count);

            foreach(var t in Tables.Values)
                wrtr.Write(t.Name);
        }

        public void Deserialize(BinaryReader rdr) {

            Name = rdr.ReadString();
            var l = rdr.ReadInt32();
            Tables = new Dictionary<string, Table>();

            for (var i = 0; i < l; i++) {
                var name = rdr.ReadString();
                Tables.Add(name, new Table(this, name));
            }

            foreach(var t in Tables.Values)
                t.ReadRelations();
        }
        #endregion


        /// <summary>
        /// Returns the path within the database ending on given fname
        /// </summary>
        /// <param name="fname">filename which is used at the end of the path</param>
        public string GetPath(string fname) {
            return GetPath(new[] { Name, fname });
        }

        /// <summary>
        /// Named accessor for tables
        /// </summary>
        /// <param name="n">Table name</param>
        /// <returns>Table with given name</returns>
        public Table this[string n] {
            get { return Tables[n]; }
        }

        public IEnumerator<Table> GetEnumerator() {
            return Tables.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #region Statics
        /// <summary>
        /// Folder defined in the AppSettings, used to write data to
        /// </summary>
        public static string DataFolder {
            get { return ConfigurationManager.AppSettings["DataFolder"]; }
        }

        /// <summary>
        /// Combines the given path parts Allong with the DataFolder
        /// </summary>
        public static string GetPath(string[] inner) {
            return Path.Combine(DataFolder, Path.Combine(inner));
        }
        #endregion
        
    }
}
