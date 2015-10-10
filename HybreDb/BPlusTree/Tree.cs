using System;
using System.Collections;
using System.Collections.Generic;
using HybreDb.Storage;

namespace HybreDb.BPlusTree {
    public class Tree<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IDisposable
        where TKey : IByteSerializable, IComparable
        where TValue : IByteSerializable {
        public Tree() {
            BucketSize = 50;
        }

        public Tree(int bucketSize) {
            BucketSize = bucketSize;
            if (BucketSize < 4)
                throw new ArgumentException("The minimal allowed bucketsize is 4");
        }

        #region Create functions

        /// <summary>
        ///     Creates a new base node bound to the tree
        /// </summary>
        public virtual BaseNode<TKey, TValue> CreateBaseNode() {
            return new BaseNode<TKey, TValue>(this);
        }

        /// <summary>
        ///     Creates a new leaf node bound to the tree
        /// </summary>
        public virtual LeafNode<TKey, TValue> CreateLeafNode() {
            return new LeafNode<TKey, TValue>(this);
        }

        #endregion

        public int BucketSize { get; protected set; }
        public INode<TKey, TValue> Root { get; protected set; }

        /// <summary>
        /// Sets and gets the a item with the given key.
        /// </summary>
        /// <remarks>The setter wil call the Insert value, this will fail when the key value already existed</remarks>
        /// <param name="k">The key of the item</param>
        /// <returns>The item bound to given key</returns>
        public TValue this[TKey k] {
            get { return Get(k); }
            set { Insert(k, value); }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            return Root.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /// <summary>
        ///     Creates a new root node of 2 given nodes
        /// </summary>
        /// <param name="l">left (lower) node</param>
        /// <param name="r">right (upper) node</param>
        /// <returns>Newly created root node</returns>
        public INode<TKey, TValue> NewRootNode(INode<TKey, TValue> l, INode<TKey, TValue> r) {
            BaseNode<TKey, TValue> n = CreateBaseNode();
            n.InsertNode(r);
            n.InsertNode(l);

            return n;
        }

        public bool Update(TKey k, NodeUpdateHandler<TKey, TValue> h) {
            return Root.Update(k, h);
        }

        public void Insert(TKey k, TValue val) {
            INode<TKey, TValue> n = Root.Insert(k, val);

            if (n != null)
                Root = NewRootNode(Root, n);
        }

        public TValue Get(TKey k) {
            TValue v;
            Root.TryGet(k, out v);
            return v;
        }

        public TValue GetOrInsert(TKey k, TValue v) {
            TValue _v;
            bool f = Root.TryGet(k, out _v);

            if (f) return _v;

            Root.Insert(k, v);
            return v;
        }

        public void Remove(TKey k) {
            RemoveResult t = Root.Remove(k);

            if ((t == RemoveResult.Merged || t == RemoveResult.Removed) && Root.Count == 1)
                Root = Root.First;
        }

        public void Init() {
            Root = CreateLeafNode();
        }

        public void Init(KeyValuePair<TKey, TValue>[] sorted) {
            if (sorted.Length == 0) {
                Init();
                return;
            }
            Root = bulkInsert(sorted);
        }

        protected INode<TKey, TValue> bulkInsert(KeyValuePair<TKey, TValue>[] sorted) {
            var nodes = new List<KeyValuePair<TKey, INode<TKey, TValue>>>();
            Array.Sort(sorted, (a, b) => a.Key.CompareTo(b.Key));

            // creating leafnodes
            int l = 0;
            for (int i = 0; i < sorted.Length; i += BucketSize - 1) {
                l = Math.Min(BucketSize - 1, sorted.Length - i);

                var seg = new ArraySegment<KeyValuePair<TKey, TValue>>(sorted, i, l);
                LeafNode<TKey, TValue> leaf = CreateLeafNode();
                leaf.Buckets.LoadSorted(seg);


                nodes.Add(new KeyValuePair<TKey, INode<TKey, TValue>>(leaf.HighestKey, leaf));
            }

            // Create intermidiate nodes
            while (nodes.Count > 1) {
                var newNodes = new List<KeyValuePair<TKey, INode<TKey, TValue>>>();

                KeyValuePair<TKey, INode<TKey, TValue>>[] a_nodes = nodes.ToArray();
                for (int i = 0; i < a_nodes.Length; i += BucketSize - 1) {
                    l = Math.Min(BucketSize - 1, a_nodes.Length - i);

                    var seg = new ArraySegment<KeyValuePair<TKey, INode<TKey, TValue>>>(a_nodes, i, l);
                    BaseNode<TKey, TValue> node = CreateBaseNode();
                    node.Buckets.LoadSorted(seg);
                    newNodes.Add(new KeyValuePair<TKey, INode<TKey, TValue>>(node.HighestKey, node));
                }

                nodes = newNodes;
            }

            return nodes[0].Value;
        }

        public void Clear() { Init(); }


        protected virtual void Dispose(bool disposing) {
            Root.Dispose();
            Root = null;
        }
    }
}