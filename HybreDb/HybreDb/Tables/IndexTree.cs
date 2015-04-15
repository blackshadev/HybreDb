using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HybreDb.BPlusTree;
using HybreDb.BPlusTree.DataTypes;

namespace HybreDb.Tables {

    /// <summary>
    /// Wrapper which holds a DiskTree for a type
    /// </summary>
    public class IndexTree {

        public string Name;
        public Type KeyType;
        public object Tree;

        public MethodInfo _mi;

        public IndexTree(string name, DataType.Types keyType) {
            KeyType = keyType.GetSystemType();
            Name = name;

            var t = typeof(DiskTree<,>).MakeGenericType(KeyType, typeof(Numbers));
            _mi = typeof(DiskTree<,>).GetMethod("Get");

            Tree = Activator.CreateInstance(t, new object[] { name + ".idx.bin", Table.BucketSize, Table.CacheSize });
        }

        public Numbers Condition(object d) {
            if(KeyType != d.GetType())
                throw new ArgumentException("Type mismatch in index tree of type " + KeyType + " given " + d.GetType());

            return (Numbers)_mi.Invoke(Tree, new [] { d });
        }

    }
}
