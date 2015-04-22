using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using HybreDb.Storage;
using HybreDb.Tables;

namespace HybreDb {
    public class Database : ITreeSerializable, IEnumerable<Table> {
        public string Name { get; protected set; }

        protected Dictionary<string, Table> Tables;
        protected Stream Stream;

        public Database(string name, bool flush=false) {
            Name = name;

            var dir = GetPath("");
            if (flush && Directory.Exists(dir))
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

        public void Read() {
            Stream.Seek(-8, SeekOrigin.End);
            var rdr = new BinaryReader(Stream);
            Stream.Position = rdr.ReadInt64();
            Deserialize(rdr);
        }

        public void Write() {
            Stream.Seek(0, SeekOrigin.End);
            
            var start = Stream.Position;
            var wrtr = new BinaryWriter(Stream);

            Serialize(wrtr);

            wrtr.Write(start);
        }

        public Table NewTable(string name, DataColumn[] cols) {
            if(Tables.ContainsKey(name))
                throw new ArgumentException("Table named `" + name + "` already exists in database `" + Name + "`");

            var t = new Table(this, name, cols);
            Tables.Add(t.Name, t);

            Write();
            
            return t;
        }
            
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

        }

        public string GetPath(string fname) {
            return GetPath(new[] {Name, fname});
        }

        public static string DataFolder {
            get { return ConfigurationManager.AppSettings["DataFolder"]; }
        }

        public static string GetPath(string[] inner) {
            return Path.Combine(DataFolder, Path.Combine(inner));
        }

        public Table this[string n] {
            get { return Tables[n]; }
        }

        public IEnumerator<Table> GetEnumerator() {
            return Tables.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
