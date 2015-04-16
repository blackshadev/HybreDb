using System;   
using System.Collections.Generic;
using System.Diagnostics;
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

        public void Serialize(System.IO.BinaryWriter wrtr) {
            wrtr.Write((byte)DataType);
            wrtr.Write(Name);
            wrtr.Write(HasIndex);
        }

        public void Deserialize(System.IO.BinaryReader rdr) {
            DataType = (DataType.Types) rdr.ReadByte();
            Name = rdr.ReadString();

            HasIndex = rdr.ReadBoolean();
        }

        public void Commit() {
            if (HasIndex) Index.Commit();
        }

        public void Dispose() {
            if(HasIndex) Index.Dispose();
        }

    }
}
