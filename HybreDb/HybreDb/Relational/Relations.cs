using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Storage;
using HybreDb.Tables;
using HybreDb.Tables.Types;


namespace HybreDb.Relational {
    public class Relations : IByteSerializable, IEnumerable<Relation>, IDisposable {

        protected Table SourceTable;
        protected Dictionary<Table, string> ByTable;
        protected Dictionary<string, Relation> ByName; 
        

        public Relations(Table t) {
            SourceTable = t;
            ByTable = new Dictionary<Table, string>();
            ByName = new Dictionary<string, Relation>();
        }

        public void Serialize(BinaryWriter wrtr) {
            wrtr.Write(ByTable.Count);

            foreach (var kvp in ByName) 
                kvp.Value.Serialize(wrtr);
        }

        public void Deserialize(BinaryReader rdr) {
            var c = rdr.ReadInt32();

            for (var i = 0; i < c; i++) {
                var rel = new Relation(SourceTable.Database, rdr);

                ByName.Add(rel.Name, rel);
                ByTable.Add(rel.Source, rel.Name);
            }
        }

        public Relation this[string n] {
            get { return ByName[n]; }
        }


        public Relation Add(string name, Table s, Table d) {
            if(ByName.ContainsKey(name))
                throw new ArgumentException("Relation with the same name already exists on this table");

            var r = new Relation(name, s, d);

            ByName.Add(name, r);
            ByTable.Add(d, name);
            
            return r;
        }

        public IEnumerator<Relation> GetEnumerator() {
            return ByName.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Dispose() {
            foreach (var r in this)
                r.Dispose();
        }
    }
}
