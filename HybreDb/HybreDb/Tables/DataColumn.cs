using System;   
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Storage;

namespace HybreDb.Tables {
    public class DataColumn : ITreeSerializable, IDisposable {
        public Type Type {
            get { return DataType.GetSystemType(); }
        }

        public DataType.Types DataType;
        public string Name;
        public bool HasIndex;
        public IndexTree Index { get; protected set; }

        public Table Table;

        public DataColumn() {}

        public DataColumn(Table t, BinaryReader rdr) {
            Table = t;
            Deserialize(rdr);
        }

        public DataColumn(string name, DataType.Types t, bool idx = false) {
            Name = name;
            DataType = t;
            HasIndex = idx;
        }

        public void CreateIndex() {
            if (!HasIndex) return;

            var t = typeof (IndexTree<>).MakeGenericType(new[] {DataType.GetSystemType()});
            Index = (IndexTree)Activator.CreateInstance(t, new object[] { Table.Name + "_" + Name });
        }

        public void Serialize(BinaryWriter wrtr) {
            wrtr.Write((byte)DataType);
            wrtr.Write(Name);
            wrtr.Write(HasIndex);
        }

        public void Deserialize(BinaryReader rdr) {
            DataType = (DataType.Types) rdr.ReadByte();
            Name = rdr.ReadString();

            HasIndex = rdr.ReadBoolean();
        }

        public void Commit() {
            if (HasIndex) Index.Commit();
        }

        public bool CheckType(object o) {
            return DataType.GetSystemType() == o.GetType();
        }

        public Numbers Find(object obj) {
            if (!CheckType(obj)) {
                throw new ArgumentException("Invalid data type. Expected `" + DataType.GetSystemType().Name + "` got `" +
                                            obj.GetType().Name + "`");
            }

            return HasIndex ? FindIndexed(obj) : FindIterate(obj);
        }

        protected Numbers FindIndexed(object obj) {
            return Index.Find(obj);
        }

        protected Numbers FindIterate(object obj) {
            var nums = new Numbers();

            foreach (var kvp in Table.Rows) {
                if(kvp.Value.Data[1].CompareTo(obj) == 0)
                    nums.Add(kvp.Key);
            }

            return nums;
        }

        public void Dispose() {
            if(HasIndex) Index.Dispose();
        }

    }
}
