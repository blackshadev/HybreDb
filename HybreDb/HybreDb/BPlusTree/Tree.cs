﻿using System;
using System.Collections.Generic;
using System.Linq;
using HybreDb.Storage;

namespace HybreDb.BPlusTree {
    public class Tree<TKey, TValue> : IEnumerable<TValue> , IDisposable
        where TKey : ITreeSerializable, IComparable
        where TValue : ITreeSerializable
    {

        public int BucketSize { get; protected set; }
        public INode<TKey, TValue> Root { get; protected set; }

        public Tree() {
            BucketSize = 50;
            Root = CreateLeafNode();
        }

        public Tree(int bucketSize) {
            BucketSize = bucketSize;
            if(BucketSize < 4) 
                throw new ArgumentException("The minimal allowed bucketsize is 4");
            Root = CreateLeafNode();
        }

        public Tree(int bucketSize, KeyValuePair<TKey, TValue>[] data) : this(bucketSize) {
            Root = bulkInsert(data);
        }


        #region Create functions
        /// <summary>
        /// Creates a new base node bound to the tree
        /// </summary>
        public virtual BaseNode<TKey, TValue> CreateBaseNode() { return new BaseNode<TKey, TValue>(this); }

        /// <summary>
        /// Creates a new leaf node bound to the tree
        /// </summary>
        public virtual LeafNode<TKey, TValue> CreateLeafNode(LeafNode<TKey, TValue> prev = null, LeafNode<TKey, TValue> next = null) {
            return new LeafNode<TKey, TValue>(this) {
                Prev = prev,
                Next = next
            };
        }


        #endregion

        /// <summary>
        /// Creates a new root node of 2 given nodes
        /// </summary>
        /// <param name="l">left (lower) node</param>
        /// <param name="r">right (upper) node</param>
        /// <returns>Newly created root node</returns>
        public INode<TKey, TValue> NewRootNode(INode<TKey, TValue> l, INode<TKey, TValue> r) {
            var n = CreateBaseNode();
            n.InsertNode(r);
            n.InsertNode(l);

            n.Changed();
            
            return n;
        }

        public void Insert(TKey k, TValue val) {
            var n = Root.Insert(k, val);

            if (n != null)
                Root = NewRootNode(Root, n);
            Root.Accessed();
        }


        public TValue this[TKey k] {
            get { return Root.Get(k); }
            //set { Root.Insert(k.GetHashCode(), value); }
        }

        public void Remove(TKey k) {
            var t = Root.Remove(k);

            if ((t == RemoveResult.Merged || t == RemoveResult.Removed) && Root.Count == 1)
                Root = Root.First;
            

        }


        protected INode<TKey, TValue> bulkInsert(KeyValuePair<TKey, TValue>[] sorted) {
            var nodes = new List<KeyValuePair<TKey, INode<TKey, TValue>>>();
            Array.Sort(sorted, (a, b) => a.Key.CompareTo(b.Key));

            // creating leafnodes
            int l = 0;
            LeafNode<TKey, TValue> prev = null;
            for (var i = 0; i < sorted.Length; i += BucketSize - 1) {
                l = Math.Min(BucketSize - 1, sorted.Length - i);

                var seg = new ArraySegment<KeyValuePair<TKey, TValue>>(sorted, i, l);
                var leaf = CreateLeafNode(prev);
                leaf.Buckets.LoadSorted(seg);
                
                if (prev != null) prev.Next = leaf;

                nodes.Add(new KeyValuePair<TKey, INode<TKey, TValue>>(leaf.HighestKey, leaf));
                
                prev = leaf;
            }

            // Create intermidiate nodes
            while (nodes.Count > 1) {
                var newNodes = new List<KeyValuePair<TKey, INode<TKey, TValue>>>();

                var a_nodes = nodes.ToArray();
                for (var i = 0; i < a_nodes.Length; i += BucketSize - 1) {
                    l = Math.Min(BucketSize - 1, a_nodes.Length - i);

                    var seg = new ArraySegment<KeyValuePair<TKey, INode<TKey, TValue>>>(a_nodes, i, l);
                    var node = CreateBaseNode();
                    node.Buckets.LoadSorted(seg);
                    newNodes.Add(new KeyValuePair<TKey, INode<TKey, TValue>>(node.HighestKey, node));
                }

                nodes = newNodes;
            }

            return nodes[0].Value;
        }

        public IEnumerator<TValue> GetEnumerator() {
            return Root.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Dispose() {
            Root.Dispose();
            Root = null;
        }
    }
}