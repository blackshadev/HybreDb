using System;
using System.Collections.Generic;
using System.Linq;
using HybreDb.BPlusTree;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Storage;
using HybreDb.Tables.Types;
using Newtonsoft.Json.Bson;

namespace HybreDb.Tables {
    
    /// <summary>
    /// Interface allowing interaction with a untyped genetic IndexedTree
    /// </summary>
    public interface IIndexTree : IDisposable {
        void Add(object d, Number n);

        void Remove(object d, Number n);

        void Init();

        void Commit();
        void Read();

        void Revert();


        Numbers Match(object k);
    }

    /// <summary>
    /// Wrapper which holds a DiskTree for a specified type
    /// </summary>
    public class IndexTree<TKey> : IIndexTree
        where TKey : IComparable, IByteSerializable, new()
    {
        /// <summary>
        /// Name of the index tree, used as filename of the index tree
        /// </summary>
        public string Name;
        public DiskTree<TKey, Numbers> Tree;


        public IndexTree(string name) {
            Name = name;
            Tree = new DiskTree<TKey, Numbers>(name + ".idx.bin", Table.BucketSize, Table.CacheSize);
        }

        public void Revert() {
            Tree.Revert();
        }

        /// <summary>
        /// Give the numbers of indices matching the given object.
        /// </summary>
        /// <param name="d">Object to match</param>
        public Numbers Match(object d) {
            if(d.GetType() != typeof(TKey))
                throw new ArgumentException("Data with wrong type given. Expected `" + typeof(TKey).Name + "` got `" + d.GetType().Name + "`");
            return Match((TKey) d);
        }

        /// <summary>
        /// Give the numbers of indices matching the given key value
        /// </summary>
        /// <param name="d">Key value to match</param>
        public Numbers Match(TKey d) {
            return Tree.Get(d);
        }

        /// <summary>
        /// Adds given key with number value to the index tree 
        /// </summary>
        /// <param name="d"></param>
        /// <param name="num"></param>
        public void Add(TKey d, Number num) {
            var nums = new Numbers {num};

            var c = Tree.Update(d, (n, k, v) => {
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

        /// <summary>
        /// Untyped Add operation, see Add(TKey, Number)
        /// </summary>
        public void Add(object d, Number n) {
            Add((TKey)d, n);
        }

        public void Remove(TKey d, Number num) {

            var empty = false;
            var c = Tree.Update(d, (l, k, v) => {
                if (v == null) return false;

                v.Remove(num);

                empty = v.Count == 0;

                return true;
            });
            

            if(empty) Tree.Remove(d);
        }

        public void Remove(object d, Number n) {
            Remove((TKey)d, n);
        }

        public void Init() {
            Tree.Init();
        }

        /// <summary>
        /// Write the changes in the IndexTree to file
        /// </summary>
        public void Commit() {
            Tree.Write();
        }

        /// <summary>
        /// Read the index tree from file
        /// </summary>
        public void Read() {
            Tree.Read();
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Release all resources held by the tree
        /// </summary>
        protected virtual void Dispose(bool disposing) {
            Tree.Dispose();
        }

    }
}
