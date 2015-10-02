using System;
using System.IO;
using System.Linq;
using HybreDb.Storage;
using HybreDb.Tables.Types;

namespace HybreDb.Tables {
    public class DataColumn : IByteSerializable, IDisposable {
        /// <summary>
        ///     Internal data type, used in serialization and creation
        /// </summary>
        public DataTypes.Types DataType;

        /// <summary>
        ///     Whenever this column has an index associated with the data
        /// </summary>
        public IndexType IndexType;
        

        /// <summary>
        ///     Whenever the column is hidden to the user
        /// </summary>
        public bool Hidden;

        /// <summary>
        ///     Name of the DataColumn
        /// </summary>
        public string Name;

        /// <summary>
        ///     Reference to the table containing this column
        /// </summary>
        internal Table Table;
        
        
        /// <summary>
        ///     System Type of the data in this column
        /// </summary>
        public Type Type => DataType.GetSystemType();

        /// <summary>
        /// Gets the index of the column within the table
        /// </summary>
        public int DataIndex => Table.Columns.IndexOf(Name);

        /// <summary>
        ///     Index tree
        /// </summary>
        public IIndexTree Index { get; protected set; }


        public DataColumn() { }

        /// <summary>
        ///     Creates a existing DataColumn based on a BinaryStream and bound to a given table
        /// </summary>
        public DataColumn(Table t, BinaryReader rdr) {
            Table = t;
            Deserialize(rdr);
        }

        /// <summary>
        ///     Creates a definition of a DataColumn which is not yet bound to a table
        /// </summary>
        public DataColumn(string name, DataTypes.Types type, bool idx = false, bool hidden = false, bool unique = false) {
            Name = name;
            DataType = type;
            HasIndex = idx;
            Hidden = hidden;
        }


        /// <summary>
        ///     Serializes the DataColumn, this does not serialize the IndexTree, use Commit() for that purpose.
        /// </summary>
        public void Serialize(BinaryWriter wrtr) {
            wrtr.Write((byte) DataType);
            wrtr.Write(Name);
            wrtr.Write(HasIndex);
            wrtr.Write(Hidden);
        }


        /// <summary>
        ///     Deserializes the DataColumn, this does not create the IndexTree, use CreateIndex() fot that.
        /// </summary>
        public void Deserialize(BinaryReader rdr) {
            DataType = (DataTypes.Types) rdr.ReadByte();
            Name = rdr.ReadString();

            HasIndex = rdr.ReadBoolean();
            Hidden = rdr.ReadBoolean();
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Creates the index tree with given DataTypes as generic key type
        /// </summary>
        public void CreateIndex() {
            if (!HasIndex) return;

            Type t = typeof (IndexTree<>).MakeGenericType(new[] {DataType.GetSystemType()});
            Index =
                (IIndexTree) Activator.CreateInstance(t, new object[] {Table.Database.GetPath(Table.Name + "_" + Name)});
        }

        /// <summary>
        /// </summary>
        public void Commit() {
            if (HasIndex) Index.Commit();
        }


        /// <summary>
        ///     Performs a type check with the ColumnData type and given object data type
        /// </summary>
        public bool CheckType(object o) {
            return DataType.GetSystemType() == o.GetType();
        }

        /// <summary>
        ///     Match given numbers belonging to given object
        /// </summary>
        /// <param name="obj">Value to match with column</param>
        /// <returns>Numbers containing the indices of rows satisfying the match condition</returns>
        public Numbers Match(object obj) {
            if (!CheckType(obj)) {
                throw new ArgumentException("Invalid data type. Expected `" + DataType.GetSystemType().Name + "` got `" +
                                            obj.GetType().Name + "`");
            }

            return HasIndex ? MatchIndexed(obj) : MatchIterate(obj);
        }

        /// <summary>
        ///     Match with the index tree
        /// </summary>
        protected Numbers MatchIndexed(object obj) {
            return Index.Match(obj);
        }

        /// <summary>
        ///     Match by iterating over all tree nodes
        /// </summary>
        protected Numbers MatchIterate(object obj) {
            var nums = new Numbers();

            foreach (var kvp in Table.Rows.Where(kvp => kvp.Value[DataIndex].CompareTo(obj) == 0)) {
                nums.Add(kvp.Key);
            }

            return nums;
        }

        /// <summary>
        ///     Dispose all resources held by the DataColumn
        /// </summary>
        protected virtual void Dispose(bool disposing) {
            if (HasIndex) Index.Dispose();
        }

        /// <summary>
        ///     Drops the columns index
        /// </summary>
        public void Drop() {
            if (HasIndex) Index.Drop();
            HasIndex = false;
        }
    }
}