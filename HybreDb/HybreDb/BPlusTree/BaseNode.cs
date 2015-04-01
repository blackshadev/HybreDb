using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace HybreDb.BPlusTree {

    public interface INode<T> {

        int HighestKey { get; }
        int LowestKey { get; }

        /// <summary>
        /// Selects a node in within the current node based on the given key
        /// </summary>
        /// <param name="key">Key to find the node of</param>
        /// <returns>Returns the node</returns>
        INode<T> Select(int k);

        /// <summary>
        /// Gets a data item from the tree
        /// </summary>
        /// <param name="k">Key to lookup</param>
        /// <returns>Data item bound to given key</returns>
        T Get(int k);

        /// <summary>
        /// Inserts given data with given key in the tree
        /// </summary>
        /// <param name="key">Key to insert with</param>
        /// <param name="data">Data to associate with given key</param>
        /// <returns>A newly created node when the bucket is full</returns>
        INode<T> Insert(int key, T data);
    }

    public class BaseNode<T> : INode<T> {

        public SortedBuckets<int, INode<T>> Buckets;
        public int Size;

        public int HighestKey {
            get { return Buckets.KeyAt(Buckets.Count - 1); }
        }

        public int LowestKey {
            get { return Buckets.KeyAt(0); }
        }


        public BaseNode(int size) {
            Size = size;
            Buckets = new SortedBuckets<int, INode<T>>(size);
        }

        /// <summary>
        /// Iterate over the bucket to find the next node
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public INode<T> Select(int key) {
            return Buckets.ValueAt(Buckets.NearestIndex(key));
        }

        public T Get(int key) {
            return Select(key).Get(key);
        }

        public INode<T> InsertNode(INode<T> node) {
            Buckets.Add(node.HighestKey, node);
            if (Buckets.Count == Buckets.Capacity)
                return Split();
            return null;
        }

        public INode<T> Insert(int key, T data) {
            var idx = Buckets.NearestIndex(key);
            var n = Buckets.ValueAt(idx);
            var _n = n.Insert(key, data);

            Buckets.Set(idx, n.HighestKey, n);

            if (_n != null)
                return InsertNode(_n);
            return null;
        }

        public INode<T> Split() {
            var node = new BaseNode<T>(Size);
            node.Buckets = Buckets.Slice(Size / 2);

            return node;
        }

    }


}
