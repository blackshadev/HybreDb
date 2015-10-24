using HybreDb.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Storage;
using HybreDb.Tables.Types;
using System.IO;

namespace HybreDb.Relational {
    /// <summary>
    ///     Class implementing a directed relation between two tables
    ///     It consists both of the definition and the actual data.
    /// </summary>
    public class DirectedRelation : Relation {

        public override RelationType RelationType { get { return RelationType.DirectedRelation; } }

        /// <summary>
        ///     Creates a new relation between given tables
        /// </summary>
        /// <param name="name">Name of the relation</param>
        /// <param name="src">Source table of the relation</param>
        /// <param name="dest">Destination table of the relation</param>
        /// <param name="attrs">Data attributes which each item in the relation holds</param>
        public DirectedRelation(string name, Table src, Table dest, DataColumn[] attrs) : base(name, src, dest) {
            var cols = new DataColumn[attrs.Length + 3];
            Array.Copy(attrs, 0, cols, 3, attrs.Length);

            cols[0] = new DataColumn(".rel", DataTypes.Types.NumberPair, true, true);
            cols[1] = new DataColumn(".rel.src", DataTypes.Types.Number, true);
            cols[2] = new DataColumn(".rel.dest", DataTypes.Types.Number, true);


            Table = new Table(Database, Source.Name + "." + Name + "." + Destination.Name, cols);
        }

        /// <summary>
        ///     Creates a already existsing relation by reading it in from the BinaryReader
        /// </summary>
        public DirectedRelation(Database db, BinaryReader rdr) : base(db, rdr) { }


        /// <summary>
        ///     Adds a new item to the relation.
        /// </summary>
        /// <param name="a">Primary key in the source table</param>
        /// <param name="b">Primary key in the destinayion table</param>
        /// <param name="dat">Data belonging to the relation, must match relation's attributes</param>
        public override void Add(Number a, Number b, IDataType[] dat) {
            var d = new IDataType[dat.Length + 3];
            Array.Copy(dat, 0, d, 3, dat.Length);

            d[0] = new NumberPair(a, b);
            d[1] = a;
            d[2] = b;

            Table.Insert(d);
        }

        /// <summary>
        ///     Gets the relation data of the relation between two given records
        /// </summary>
        /// <param name="a">Primary key in the source table</param>
        /// <param name="b">Primary key in the destination table</param>
        public override DataRow Get(Number a, Number b) {
            var nums = new NumberPair(a, b);

            return Table.FindRows(new KeyValuePair<string, object>(".rel", nums)).First();
        }

    }
}
