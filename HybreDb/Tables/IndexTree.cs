using System;
using System.Collections.Generic;
using System.Linq;
using HybreDb.BPlusTree;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Storage;
using HybreDb.Tables.Types;

namespace HybreDb.Tables {

    public static class IndexTree {
        public enum IndexType {
            None,
            Index,
            UniqueIndex
        }

        public static Type GetType(IndexType t) {
            switch (t) {
                case IndexType.Index: return typeof(IndexTree<>);
                case IndexType.UniqueIndex: return typeof(UniqueIndexTree<>);
                default: return null;
            }
        }
    }

    /// <summary>
    ///     Interface allowing interaction with a untyped genetic IndexedTree
    /// </summary>
    public interface IIndexTree : IDisposable {
        void Add(object d, Number n);

        void Remove(object d, Number n);

        void Init();

        void Init(IEnumerable<KeyValuePair<IDataType, Numbers>> d);

        void Commit();
        void Read();

        void Revert();

        void Clear();
        void Drop();

        Numbers Match(object k);
        
    }

    public abstract class AIndexTree<TKey, TValue> : IIndexTree
        where TKey : IDataType, new()
        where TValue : IByteSerializable, new() {
        
        /// <summary>
        ///     Name of the index tree, used as filename of the index tree
        /// </summary>
        public string Name;

        /// <summary>
        /// Actual Tree with keys
        /// </summary>
        public DiskTree<TKey, TValue> Tree; 

        protected AIndexTree(string name) {
            Name = name;
            Tree = new DiskTree<TKey, TValue>(name + ".idx.bin", Table.BucketSize, Table.CacheSize);
        }

        public abstract void Add(TKey d, Number n);
        public void Add(object d, Number n) => Add((TKey) d, n);

        public abstract void Remove(TKey d, Number n);
        public void Remove(object d, Number n) => Remove((TKey)d, n);


        public Numbers Match(object k) {
            if (k.GetType() != typeof(TKey))
                throw new ArgumentException("Data with wrong type given. Expected `" + typeof(TKey).Name + "` got `" +
                                            k.GetType().Name + "`");
            return Match((TKey)k);
        }
        public abstract Numbers Match(TKey k);

        public abstract void Init(IEnumerable<KeyValuePair<IDataType, Numbers>> d);

        public void Init() => Tree.Init();
        public void Commit() => Tree.Write();
        public void Read() => Tree.Read();
        public void Revert() => Tree.Revert();
        public void Clear() => Tree.Clear();
        public void Drop() => Tree.Drop();
        
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool all) => Tree.Dispose();
        
    }


    /// <summary>
    ///     Wrapper which holds a DiskTree for a specified type
    /// </summary>
    public class IndexTree<TKey> : AIndexTree<TKey, Numbers>
        where TKey : IDataType, new() {

        public IndexTree(string n) : base(n) {}
        

        public override void Init(IEnumerable<KeyValuePair<IDataType, Numbers>> data) {
            KeyValuePair<TKey, Numbers>[] typed =
                data.Select(e => new KeyValuePair<TKey, Numbers>((TKey) e.Key, e.Value)).ToArray();
            Tree.Init(typed);
        }

        
        /// <summary>
        ///     Give the numbers of indices matching the given key value
        /// </summary>
        /// <param name="d">Key value to match</param>
        public override Numbers Match(TKey d) {
            return Tree.Get(d) ?? new Numbers();
        }

        /// <summary>
        ///     Adds given key with number value to the index tree
        /// </summary>
        /// <param name="d"></param>
        /// <param name="num"></param>
        public override void Add(TKey d, Number num) {
            var nums = new Numbers {num};

            bool c = Tree.Update(d, (n, k, v) => {
                // Not yet present in the tree and not yet full
                if (v == null && n.Count < n.Capacity - 1) {
                    n.Buckets.Add(k, nums);
                    return true;
                }
                // Key already present so we can update the value
                if (v != null) {
                    v.Add(num);
                    return true;
                }

                return false;
            });
            if (c) return;

            // Update failed due to full node, so we need to insert manually
            Tree.Insert(d, nums);
        }

        public override void Remove(TKey d, Number num) {
            bool empty = false;
            bool c = Tree.Update(d, (l, k, v) => {
                if (v == null) return false;

                v.Remove(num);

                empty = v.Count == 0;

                return true;
            });


            if (empty) Tree.Remove(d);
        }
        
    }

    public class UniqueIndexTree<TKey> : AIndexTree<TKey, Number>
        where TKey : IDataType, new() {
        
        public UniqueIndexTree(string name) : base(name) {}

        
        public override void Init(IEnumerable<KeyValuePair<IDataType, Numbers>> data) {
            KeyValuePair<TKey, Number>[] typed =
                data.Select(e => {
                    if (e.Value.Nums.Count != 1) throw new InvalidOperationException("Duplicate keys");
                    return new KeyValuePair<TKey, Number>((TKey)e.Key, e.Value.First());
                }).ToArray();
            Tree.Init(typed);
        }

        public override Numbers Match(TKey k) {
            var num = Tree[k];
            var nums = new Numbers();
            if (num != null) nums.Add(num);
            return nums;
        }

        public override void Add(TKey d, Number n) {
            Tree.Insert(d, n);
        }

        public override void Remove(TKey d, Number n) {
            Tree.Remove(d);
        }
    }
}