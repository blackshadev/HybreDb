using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Storage;
using HybreDb.Tables;

namespace HybreDb.Relational {
    public class Relation : IByteSerializable {

        public Database Database;
        public DataColumn Source;
        public DataColumn Destination;

        protected Relation(Database db) {
            Database = db;
        }

        public Relation(Database db, DataColumn src, DataColumn dest) : this(db) {
            Source = src;
            Destination = dest;
        }

        public Relation(Database db, BinaryReader rdr) :this(db) {
            Deserialize(rdr);
        }


        public void Serialize(BinaryWriter wrtr) {
            wrtr.Write(Source.Table.Name);
            wrtr.Write(Source.Name);

            wrtr.Write(Destination.Table.Name);
            wrtr.Write(Destination.Name);

        }

        public void Deserialize(BinaryReader rdr) {
            Source = Database[rdr.ReadString()].Columns[rdr.ReadString()];
            Destination = Database[rdr.ReadString()].Columns[rdr.ReadString()];
        }
    }
}
