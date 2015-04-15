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
    public class IndexTree : IDisposable {

        public string Name;
        public Type KeyType;
        public object Tree;

        private Type _treeType;
        private MethodInfo _read;
        private MethodInfo _get;
        private MethodInfo _getins;


        public IndexTree(string name, DataType.Types keyType) {
            KeyType = keyType.GetSystemType();
            Name = name;

            _treeType = typeof(DiskTree<,>).MakeGenericType(KeyType, typeof(Numbers));
            _get = _treeType.GetMethod("Get");
            _getins = _treeType.GetMethod("GetOrInsert");
            _read = _treeType.GetMethod("Read");


            Tree = Activator.CreateInstance(_treeType, new object[] { name + ".idx.bin", Table.BucketSize, Table.CacheSize });
        }

        protected void CheckArgument(object d) {
            if (KeyType != d.GetType())
                throw new ArgumentException("Type mismatch in index tree of type " + KeyType + " given " + d.GetType());
        }

        public Numbers Condition(object d) {
            CheckArgument(d);

            return (Numbers)_get.Invoke(Tree, new[] { d });
        }

        public void Add(object d, Number n) {
            CheckArgument(d);

            var nums = (Numbers) _getins.Invoke(Tree, new[] { d, new Numbers() });
            nums.Add(n);
        }

        public void Commit() {
            _treeType.GetMethod("Write").Invoke(Tree, new object[0]);
        }

        public void Read() {
            _read.Invoke(Tree, new object[0]);
        }

        public void Dispose() {
            _treeType.GetMethod("Dispose").Invoke(Tree, new object[0]);
        }

    }
}
