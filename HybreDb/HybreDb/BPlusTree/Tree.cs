using System;
using System.Collections.Generic;
using System.Linq;

namespace HybreDb.BPlusTree {
    public class Tree<T> : IEnumerable<T> {

        public int BucketSize { get; private set; }
        public INode<T> Root { get; private set; }

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

        public Tree(int size, KeyValuePair<int, T>[] data) : this(size) {
            Root = bulkInsert(data);
        }

        /// <summary>
        /// Creates a new base node bound to the tree
        /// </summary>
        public BaseNode<T> CreateBaseNode() { return new BaseNode<T>(this); }
        
        /// <summary>
        /// Creates a new leaf node bound to the tree
        /// </summary>
        public LeafNode<T> CreateLeafNode() { return new LeafNode<T>(this); } 

        /// <summary>
        /// Creates a new root node of 2 given nodes
        /// </summary>
        /// <param name="l">left (lower) node</param>
        /// <param name="r">right (upper) node</param>
        /// <returns>Newly created root node</returns>
        public INode<T> NewRootNode(INode<T> l, INode<T> r) {
            var n = CreateBaseNode();
            n.InsertNode(r);
            n.InsertNode(l);
            
            return n;
        }

        public void Insert(object k, T val) {
            int key = k.GetHashCode();
            var n = Root.Insert(key, val);

            if (n != null)
                Root = NewRootNode(Root, n);
        }


        public T this[object k] {
            get { return Root.Get(k.GetHashCode()); }
            //set { Root.Insert(k.GetHashCode(), value); }
        }

        public void Remove(object k) {
            int key = k.GetHashCode();
            var t = Root.Remove(key);

            if ((t == RemoveResult.Merged || t == RemoveResult.Removed) && Root.Count == 1)
                Root = Root.First;
            

        }

        protected LeafNode<T> GetFirstLeaf() {
            var n = Root;

            while (!(n is LeafNode<T>))
                n = n.First;
            
            return (LeafNode<T>)n;
        }

        protected INode<T> bulkInsert(KeyValuePair<int, T>[] sorted) {
            var nodes = new List<KeyValuePair<int, INode<T>>>();
            Array.Sort(sorted, (a, b) => a.Key.CompareTo(b.Key));

            // creating leafnodes
            int l = 0;
            LeafNode<T> prev = null;
            for (var i = 0; i < sorted.Length; i += BucketSize - 1) {
                l = Math.Min(BucketSize - 1, sorted.Length - i);

                var seg = new ArraySegment<KeyValuePair<int, T>>(sorted, i, l);
                var leaf = CreateLeafNode();
                leaf.Data.LoadSorted(seg);
                leaf.Prev = prev;
                
                if (prev != null) prev.Next = leaf;
                
                nodes.Add(new KeyValuePair<int, INode<T>>(leaf.HighestKey, leaf));
                
                prev = leaf;
            }

            // Create intermidiate nodes
            while (nodes.Count > 1) {
                var newNodes = new List<KeyValuePair<int, INode<T>>>();

                var a_nodes = nodes.ToArray();
                for (var i = 0; i < a_nodes.Length; i += BucketSize - 1) {
                    l = Math.Min(BucketSize - 1, a_nodes.Length - i);

                    var seg = new ArraySegment<KeyValuePair<int, INode<T>>>(a_nodes, i, l);
                    var node = CreateBaseNode();
                    node.Buckets.LoadSorted(seg);
                    newNodes.Add(new KeyValuePair<int, INode<T>>(node.HighestKey, node));
                }

                nodes = newNodes;
            }

            return nodes[0].Value;
        } 

        public IEnumerator<T> GetEnumerator() {
            var n = GetFirstLeaf();

            do {
                foreach (var v in n.Data.Values)
                    yield return v;
            } while( (n = n.Next) != null );
            
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}