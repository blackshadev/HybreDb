using System;
using System.Collections.Generic;
using System.Linq;
using HybreDb.BPlusTree;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Storage;
using HybreDb.Tables.Types;

namespace HybreDb.Tables {
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
        }

        public abstract void Add(TKey d, Number n);
        public void Add(object d, Number n) => Add((TKey) d, n);

        public abstract void Remove(TKey d, Number n);
        public void Remove(object d, Number n) => Add((TKey)d, n);

        
        public abstract void Init(IEnumerable<KeyValuePair<IDataType, Numbers>> d);

        public void Init() => Tree.Init();
        public void Commit() => Tree.Write();
        public void Read() => Tree.Read();
        public void Revert() => Tree.Revert();
        public void Drop() => Tree.Drop();

        public abstract Numbers Match(object k);

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool all);
    }


    /// <summary>
    ///     Wrapper which holds a DiskTree for a specified type
    /// </summary>
    public class IndexTree<TKey> : AIndexTree<TKey, Numbers>
        where TKey : IDataType, new() {
        

        public IndexTree(string name) : base(name) {
            Tree = new DiskTree<TKey, Numbers>(name + ".idx.bin", Table.BucketSize, Table.CacheSize);
        }

        /// <summary>
        ///     Give the numbers of indices matching the given object.
        /// </summary>
        /// <param name="d">Object to match</param>
        public override Numbers Match(object d) {
            if (d.GetType() != typeof (TKey))
                throw new ArgumentException("Data with wrong type given. Expected `" + typeof (TKey).Name + "` got `" +
                                            d.GetType().Name + "`");
            return Match((TKey) d);
        }
        
        

        public override void Init(IEnumerable<KeyValuePair<IDataType, Numbers>> data) {
            KeyValuePair<TKey, Numbers>[] typed =
                data.Select(e => new KeyValuePair<TKey, Numbers>((TKey) e.Key, e.Value)).ToArray();
            Tree.Init(typed);
        }

        
        /// <summary>
        ///     Give the numbers of indices matching the given key value
        /// </summary>
        /// <param name="d">Key value to match</param>
        public Numbers Match(TKey d) {
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

        /// <summary>
        ///     Release all resources held by the tree
        /// </summary>
        protected override void Dispose(bool disposing) {
            Tree.Dispose();
        }
    }
    
}