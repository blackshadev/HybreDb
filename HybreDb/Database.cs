using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using HybreDb.Relational;
using HybreDb.Storage;
using HybreDb.Tables;

namespace HybreDb {
    /// <summary>
    ///     Database class containing the tables of a given database
    /// </summary>
    public class Database : IByteSerializable, IEnumerable<Table> {
        /// <summary>
        ///     Regular expression used to validate user given identifiers
        /// </summary>
        public static readonly Regex ValidIdentifierRegex = new Regex("^[a-zA-Z0-9_]+$", RegexOptions.Compiled);


        protected Stream Stream;
        protected Dictionary<string, Table> Tables;

        /// <summary>
        ///     Constructor for a database
        /// </summary>
        /// <param name="name">Name of the database</param>
        /// <param name="clean">Whenever or not to remove any old database with the same name</param>
        public Database(string name, bool clean = false) {
            Name = name;

            string dir = GetPath("");
            if (clean && Directory.Exists(dir))
                Directory.Delete(dir, true);

            Directory.CreateDirectory(dir);

            string fname = GetPath(new[] {name, name + ".db.bin"});
            bool exists = (File.Exists(fname));

            Stream = DbFile.Open(fname);

            if (exists) {
                Read();
                return;
            }

            Tables = new Dictionary<string, Table>(StringComparer.OrdinalIgnoreCase);
            Write();
        }

        /// <summary>
        ///     Name of the database, used as directory name
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        ///     Number of tables in the database
        /// </summary>
        public int Count {
            get { return Tables.Count; }
        }

        /// <summary>
        ///     Named accessor for tables
        /// </summary>
        /// <param name="n">Table name</param>
        /// <returns>Table with given name</returns>
        public Table this[string n] {
            get {
                Table t;
                bool f = Tables.TryGetValue(n, out t);
                if (!f) throw new KeyNotFoundException("No such table");
                return t;
            }
        }

        public IEnumerator<Table> GetEnumerator() {
            return Tables.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #region Statics

        /// <summary>
        ///     Folder defined in the AppSettings, used to write data to
        /// </summary>
        public static string DataFolder {
            get { return ConfigurationManager.AppSettings["DataFolder"]; }
        }

        /// <summary>
        ///     Combines the given path parts Allong with the DataFolder
        /// </summary>
        public static string GetPath(string[] inner) {
            return Path.Combine(DataFolder, Path.Combine(inner));
        }

        public static void CheckUserIdentifier(string ident) {
            if (!ValidIdentifierRegex.IsMatch(ident))
                throw new ArgumentException("Illegal user identifier given `" + ident +
                                            "`. Only numeric, alphanumeric and underscore are allowed as characters.");
        }

        #endregion

        /// <summary>
        ///     Reverts all tables to their last commit
        /// </summary>
        public void Revert() {
            foreach (Table t in Tables.Values)
                t.Revert();
        }

        /// <summary>
        ///     Reads in the data from a file
        /// </summary>
        public void Read() {
            Stream.Seek(-8, SeekOrigin.End);
            var rdr = new BinaryReader(Stream);
            Stream.Position = rdr.ReadInt64();
            Deserialize(rdr);
        }


        /// <summary>
        ///     Writes changes to a file. Does not commit the tables
        /// </summary>
        public void Write() {
            Stream.Seek(0, SeekOrigin.End);

            long start = Stream.Position;
            var wrtr = new BinaryWriter(Stream);

            Serialize(wrtr);

            wrtr.Write(start);
            wrtr.Flush();

            foreach (Table t in Tables.Values)
                t.Write();
        }

        /// <summary>
        ///     Adds and creates a new table within the database
        /// </summary>
        /// <param name="name">Table name</param>
        /// <param name="cols">Column definitions</param>
        /// <returns></returns>
        public Table NewTable(string name, DataColumn[] cols) {
            CheckUserIdentifier(name);
            if (Tables.ContainsKey(name))
                throw new ArgumentException("Table named `" + name + "` already exists in database `" + Name + "`");

            foreach (DataColumn c in cols)
                CheckUserIdentifier(c.Name);

            var t = new Table(this, name, cols);
            Tables.Add(t.Name, t);

            return t;
        }

        /// <summary>
        ///     Deletes a table
        /// </summary>
        /// <param name="tableName"></param>
        public void DropTable(string tableName) {
            Table t = this[tableName];

            t.Drop();
            Tables.Remove(tableName);

            Write();
        }

        /// <summary>
        ///     Reopens a table by commiting and disposing the old one
        ///     and creating a new instance
        /// </summary>
        /// <param name="old">Table to reopen</param>
        /// <returns>New instance of the given table</returns>
        public Table Reopen(Table old) {
            bool contains = Tables.Remove(old.Name);

            if (!contains)
                throw new ArgumentException("Database doesnt contain given table");

            string n = old.Name;
            old.Commit();
            old.Dispose();


            Tables[n] = new Table(this, n);
            Tables[n].ReadRelations();

            return Tables[n];
        }

        /// <summary>
        ///     Adds a new relation between tables
        /// </summary>
        /// <param name="relName">Name of the new relation</param>
        /// <param name="srcTable">Name of the source table</param>
        /// <param name="destTable">Name of the destination table</param>
        /// <param name="attrs">Attribute definitions of each relation</param>
        /// <returns></returns>
        public Relation NewRelation(string relName, string srcTable, string destTable, DataColumn[] attrs) {
            CheckUserIdentifier(relName);

            foreach (DataColumn c in attrs)
                CheckUserIdentifier(c.Name);

            Table src = Tables[srcTable];
            Table dst = Tables[destTable];
            Relation r = src.Relations.Add(relName, src, dst, attrs);

            return r;
        }

        /// <summary>
        ///     Returns the path within the database ending on given fname
        /// </summary>
        /// <param name="fname">filename which is used at the end of the path</param>
        public string GetPath(string fname) {
            return GetPath(new[] {Name, fname});
        }

        #region serialisation

        public void Serialize(BinaryWriter wrtr) {
            wrtr.Write(Name);
            wrtr.Write(Tables.Count);

            foreach (Table t in Tables.Values)
                wrtr.Write(t.Name);
        }

        public void Deserialize(BinaryReader rdr) {
            Name = rdr.ReadString();
            int l = rdr.ReadInt32();
            Tables = new Dictionary<string, Table>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < l; i++) {
                string name = rdr.ReadString();
                Tables.Add(name, new Table(this, name));
            }

            foreach (Table t in Tables.Values)
                t.ReadRelations();
        }

        #endregion
    }
}