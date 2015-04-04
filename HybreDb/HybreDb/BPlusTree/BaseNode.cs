using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace HybreDb.BPlusTree {

    public class BaseNode<T> : INode<T> {

        public SortedBuckets<int, INode<T>> Buckets;
        public int Count { get { return Buckets.Count; } }
        public int Capacity { get { return Buckets.Capacity; } }
        public int HighestKey { get { return Buckets.KeyAt(Buckets.Count - 1); } }
        public int LowestKey { get { return Buckets.KeyAt(0); } }
        public INode<T> First { get { return Buckets.ValueAt(0); } }

        public Tree<T> Tree { get { return _tree; } }
        protected Tree<T> _tree;


        public BaseNode(Tree<T> t ) {
            _tree = t;
            Buckets = new SortedBuckets<int, INode<T>>(t.BucketSize);
        }


        public INode<T> Select(int key) {
            return Buckets.ValueAt(Buckets.NearestIndex(key));
        }

        public INode<T> InsertNode(INode<T> node) {
            Buckets.Add(node.HighestKey, node);
            if (Buckets.Count == Buckets.Capacity)
                return Split();
            return null;
        }

        public INode<T> RemoveNode(INode<T> node) {
            Buckets.Remove(node.HighestKey);

            return Count < Capacity / 4 ? this : null;
        } 

        public T Get(int key) {
            return Select(key).Get(key);
        }


        public INode<T> Insert(int key, T data) {
            var idx = Buckets.NearestIndex(key);
            var n = Buckets.ValueAt(idx);
            var _n = n.Insert(key, data);

            // Always update the index, faster than branching
            Buckets.Set(idx, n.HighestKey, n);

            if (_n != null)
                return InsertNode(_n);
            return null;
        }

        /// <summary>
        /// Recursively remove, if needed borrow, merge or remove the node.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public RemoveResult Remove(int key) {
            var idx = Buckets.NearestIndex(key);
            var node = Buckets.ValueAt(idx);

            var t = node.Remove(key);

            // Tries to flatten the tree by removing a useless node.
            if ( (t == RemoveResult.Merged || t == RemoveResult.Removed ) && node.Count == 1) {
                Buckets.RemoveIndex(idx);

                // Remove empty node
                if (node.First.Count == 0) {
                    node.First.Dispose();
                } else
                    InsertNode(node.First);
                return RemoveResult.Removed;
            }

            // Nothing to do node is big enough
            if (node.Count >= node.Capacity / 4) {
                Buckets.Set(idx, node.HighestKey, node);
                return RemoveResult.None;
            }

            // Get the left and right neighbour and tries to borrow keys
            var l = idx > 0 ? Buckets.ValueAt(idx - 1) : null;
            var r = idx < Capacity - 1 ? Buckets.ValueAt(idx + 1) : null;

            if (node.Borrow(l, r)) {
                Buckets.Set(idx, node.HighestKey, node);
                if(l != null) Buckets.Set(idx - 1, l.HighestKey, l);

                return RemoveResult.Borrowed;
            }

            // Borrow failed, try to merge to the right
            if (r != null && node.Merge(r)) {
                Buckets.RemoveIndex(idx);
                return RemoveResult.Merged;
            
            }

            // Cannot borrow but deleting the leafnode without neighbours will result in a broken tree
            if(node is LeafNode<T> && node.Count == 0 && l == null && r == null)
                return RemoveResult.Merged;

            // Remove empty base node or leafnode with neighbours
            if (node.Count == 0) {
                Buckets.RemoveIndex(idx);
                node.Dispose();
                return RemoveResult.Removed;
            }

            return RemoveResult.None;
        }

        public bool Merge(INode<T> n) {
            if (!(n is BaseNode<T>)) return false;

            var _n = (BaseNode<T>) n;
            
            _n.Buckets.AddBegin(Buckets);
            return true;
        } 

        public INode<T> Split() {
            var n = Tree.CreateBaseNode();
            n.Buckets = Buckets.SliceEnd(Capacity / 2);
            return n;
        }


        public bool Borrow(INode<T> left, INode<T> right) {
            var l = left as BaseNode<T>;
            var r = right as BaseNode<T>;

            SortedBuckets<int, INode<T>> s;
            if (l != null && l.Count - 1 - l.Capacity / 4 > Capacity / 4) {
                s = l.Buckets.SliceEnd(l.Buckets.Count - Capacity / 4);
                Buckets.AddBegin(s);

                return true;
            }
            if (r == null || r.Count - 1 - r.Capacity / 4 <= Capacity / 4) return false;

            s = r.Buckets.SliceBegin(Capacity / 4);
            Buckets.AddEnd(s);

            return true;
        }

        public void Dispose() {
            Buckets.Dispose();
        }


    }
}
