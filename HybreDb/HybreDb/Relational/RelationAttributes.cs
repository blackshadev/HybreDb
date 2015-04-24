using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Storage;

namespace HybreDb.Relational {
    /// <summary>
    /// Contains multiple RelationAttributes
    /// These can be accessed by name and index
    /// </summary>
    public class RelationAttributes : IByteSerializable, IEnumerable<RelationAttribute> {

        protected Relation Relation;
        protected RelationAttribute[] Attributes;
        protected Dictionary<string, int> ByName;

        public int Count {
            get { return Attributes.Length;  }
        }

        public RelationAttributes(Relation rel) {
            Relation = rel;
            ByName = new Dictionary<string, int>();
        }

        public RelationAttributes(Relation rel, BinaryReader rdr) : this(rel) {
            Deserialize(rdr);
        }

        public RelationAttributes(Relation rel, RelationAttribute[] attrs) : this(rel) {
            Attributes = attrs;

            for (var i = 0; i < attrs.Length; i++) {
                attrs[i].Relation = rel;
                ByName.Add(attrs[i].Name, i);
            }
        }


        /// <summary>
        /// Gets an attribute by name
        /// </summary>
        /// <param name="n">Name of the attribute</param>
        public RelationAttribute this[string n] {
            get { return this[ByName[n]]; }
        }


        /// <summary>
        /// Gets an attribute by index
        /// </summary>
        /// <param name="n">Index of the attribute</param>
        /// <remarks>Used in RelationData to map the actual data to thecorrect type</remarks>
        public RelationAttribute this[int n] {
            get { return Attributes[n]; }
        }

        #region Serialisation
        public void Serialize(BinaryWriter wrtr) {
            wrtr.Write(Attributes.Length);
            
            foreach(var a in Attributes)
                a.Serialize(wrtr);
        }

        public void Deserialize(BinaryReader rdr) {

            var l = rdr.ReadInt32();

            Attributes = new RelationAttribute[l];

            for (var i = 0; i < l; i++) {
                Attributes[i] = new RelationAttribute(Relation, rdr);
                ByName[Attributes[i].Name] = i;
            }

        }
        #endregion

        public IEnumerator<RelationAttribute> GetEnumerator() {
            return ((IEnumerable<RelationAttribute>) Attributes).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
