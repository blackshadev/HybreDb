using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Tables;
using HybreDb.Tables.Types;
using System.IO;

namespace HybreDb.Relational {
    public class UndirectedRelation : Relation {
        public override RelationType RelationType {
            get {
                return RelationType.UndirectedRelation;
            }
        }

        public UndirectedRelation(string name, Table src, Table dest, DataColumn[] attrs) : base(name, src, dest) {
            var cols = new DataColumn[attrs.Length + 3];
            Array.Copy(attrs, 0, cols, 3, attrs.Length);

            cols[0] = new DataColumn(".rel", DataTypes.Types.OrderedNumberPair, true, true);
            cols[1] = new DataColumn(".rel.src", DataTypes.Types.Number, true);
            cols[2] = new DataColumn(".rel.dest", DataTypes.Types.Number, true);


            Table = new Table(Database, Source.Name + "." + Name + "." + Destination.Name, cols);
        }

        /// <summary>
        ///     Creates a already existsing relation by reading it in from the BinaryReader
        /// </summary>
        public UndirectedRelation(Database db, BinaryReader rdr) : base(db, rdr) { }

        

        public override void Add(Number a, Number b, IDataType[] dat) {
            var d = new IDataType[dat.Length + 3];
            Array.Copy(dat, 0, d, 3, dat.Length);

            d[0] = new OrderedNumberPair(a, b);
            d[1] = a;
            d[2] = b;

            Table.Insert(d);
        }

        public override DataRow Get(Number a, Number b) {
            var nums = new OrderedNumberPair(a, b);
            return Table.FindRows(new KeyValuePair<string, object>(".rel", nums)).First();
        }
    }
}
