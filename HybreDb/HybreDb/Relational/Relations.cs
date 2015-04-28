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

    /// <summary>
    /// Holds multiple relations of a table
    /// </summary>
    public class Relations : IByteSerializable, IEnumerable<Relation>, IDisposable {

        protected Table SourceTable;
        protected Dictionary<Table, string> ByTable;
        protected Dictionary<string, Relation> ByName;
        protected Dictionary<Table, Relation> ForeignRelations;
        

        public Relations(Table t) {
            SourceTable = t;
            ByTable = new Dictionary<Table, string>();
            ByName = new Dictionary<string, Relation>();
            ForeignRelations = new Dictionary<Table, Relation>();
        }

        #region Serialization
        public void Serialize(BinaryWriter wrtr) {
            wrtr.Write(ByTable.Count);

            foreach (var kvp in ByName) 
                kvp.Value.Serialize(wrtr);
        }

        public void Deserialize(BinaryReader rdr) {
            var c = rdr.ReadInt32();

            for (var i = 0; i < c; i++) {
                var rel = new Relation(SourceTable.Database, rdr);

                AddRelation(rel);
            }
        }
        #endregion

        protected void AddRelation(Relation rel) {
            ByName.Add(rel.Name, rel);
            ByTable.Add(rel.Source, rel.Name);
            SourceTable.Database[rel.Destination.Name].Relations.ForeignRelations.Add(SourceTable, rel);
        }

        /// <summary>
        /// Gets a relation by name
        /// </summary>
        /// <param name="n">Name of the relation</param>
        public Relation this[string n] {
            get { return ByName[n]; }
        }

        /// <summary>
        /// Adds a new relation
        /// </summary>
        /// <param name="name">Name of the relation</param>
        /// <param name="s">Source table, must match the current source table</param>
        /// <param name="d">Destination table</param>
        /// <param name="attrs">Attributes of the relation</param>
        /// <returns></returns>
        public Relation Add(string name, Table s, Table d, DataColumn[] attrs) {
            if(ByName.ContainsKey(name))
                throw new ArgumentException("Relation with the same name already exists on this table");

            var r = new Relation(name, s, d, attrs);

            AddRelation(r);
            
            return r;
        }

        /// <summary>
        /// Removes all relations with given number in the source table
        /// </summary>
        /// <param name="idx"></param>
        public void RemoveItem(Number idx) {
            
            foreach (var r in this) {
                var nums = r.Table.FindRows(new KeyValuePair<string, object>(".rel.src", idx));
                if (nums == null) continue;
                r.Table.RemoveAll(nums);
            }

            foreach (var r in ForeignRelations) {
                var nums = r.Value.Table.FindRows(new KeyValuePair<string, object>(".rel.dest", idx));
                if (nums == null) continue;
                r.Value.Table.RemoveAll(nums);
            }

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
