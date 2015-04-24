using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Storage;
using HybreDb.Tables;

namespace HybreDb.Relational {
    public class Relations : IByteSerializable {

        protected Table SourceTable;
        protected Dictionary<Table, Relation> ByTable;
        

        public Relations(Table t) {
            SourceTable = t;
            ByTable = new Dictionary<Table, Relation>();
        }

        public void Serialize(BinaryWriter wrtr) {
            wrtr.Write(ByTable.Count);

            foreach (var kvp in ByTable) {
                wrtr.Write(kvp.Key.Name);
                kvp.Value.Serialize(wrtr);
            }
        }

        public void Deserialize(BinaryReader rdr) {
            var c = rdr.ReadInt32();
            var db = SourceTable.Database;

            for (var i = 0; i < c; i++) {
                var t = rdr.ReadString();
                ByTable.Add(db[t], new Relation(SourceTable.Database, rdr));
            }

        }

        public Relation Add(DataColumn s, DataColumn d) {
            if (s.Table != SourceTable)
                throw new ArgumentException("Given source column does not belong in current table");
            
            var r = new Relation(SourceTable.Database, s, d);
            ByTable.Add(d.Table, r);
            
            return r;
        }
    }
}
