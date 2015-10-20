using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HybreDb.Storage;
using HybreDb.Tables;
using HybreDb.Tables.Types;

namespace HybreDb.Relational {
    /// <summary>
    ///     Holds multiple relations of a table
    /// </summary>
    public class Relations : IByteSerializable, IEnumerable<Relation>, IDisposable {
        protected Dictionary<string, Relation> ByName;
        protected Dictionary<Table, List<string>> ByTable;
        protected List<Relation> ForeignRelations;
        protected Table SourceTable;


        public Relations(Table t) {
            SourceTable = t;
            ByTable = new Dictionary<Table, List<string>>();
            ByName = new Dictionary<string, Relation>(StringComparer.OrdinalIgnoreCase);
            ForeignRelations = new List<Relation>();
        }

        #region Serialization

        public void Serialize(BinaryWriter wrtr) {
            wrtr.Write(ByName.Count);

            foreach (var kvp in ByName)
                kvp.Value.Serialize(wrtr);
        }

        public void Deserialize(BinaryReader rdr) {
            int c = rdr.ReadInt32();

            for (int i = 0; i < c; i++) {
                var rel = Relation.Create(SourceTable.Database, rdr);

                AddRelation(rel);
            }
        }

        #endregion

        /// <summary>
        ///     Gets a relation by name
        /// </summary>
        /// <param name="n">Name of the relation</param>
        public Relation this[string n] {
            get {
                Relation r;
                if (!ByName.TryGetValue(n, out r))
                    throw new KeyNotFoundException("No such relation found");
                
                return r;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IEnumerator<Relation> GetEnumerator() {
            return ByName.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        protected void AddRelation(Relation rel) {
            ByName.Add(rel.Name, rel);

            List<string> relNames;
            bool f = ByTable.TryGetValue(rel.Source, out relNames);
            if (!f)
                relNames = ByTable[rel.Source] = new List<string>();

            relNames.Add(rel.Name);

            SourceTable.Database[rel.Destination.Name].Relations.ForeignRelations.Add(rel);
        }

        /// <summary>
        ///     Adds a new relation
        /// </summary>
        /// <param name="name">Name of the relation</param>
        /// <param name="s">Source table, must match the current source table</param>
        /// <param name="d">Destination table</param>
        /// <param name="attrs">Attributes of the relation</param>
        /// <returns></returns>
        public Relation Add(string name, Table s, Table d, DataColumn[] attrs) {
            if (ByName.ContainsKey(name))
                throw new ArgumentException("Relation with the same name already exists on this table");

            var r = Relation.Create(RelationType.MultiRelation, name, s, d, attrs);
            AddRelation(r);
            r.Commit();

            return r;
        }

        /// <summary>
        ///     Removes all relations with given number in the source table
        /// </summary>
        /// <param name="idx"></param>
        public void RemoveItem(Number idx) {
            foreach (Relation r in this) {
                Numbers nums = r.Table.FindKeys(new KeyValuePair<string, object>(".rel.src", idx));
                if (nums == null) continue;
                r.Table.RemoveAll(nums);
            }

            foreach (Relation r in ForeignRelations) {
                Numbers nums = r.Table.FindKeys(new KeyValuePair<string, object>(".rel.dest", idx));
                if (nums == null) continue;
                r.Table.RemoveAll(nums);
            }
        }


        protected virtual void Dispose(bool disposing) {
            foreach (Relation r in this) r.Dispose();
        }
    }
}