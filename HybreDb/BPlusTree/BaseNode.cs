using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HybreDb.BPlusTree.Collections;
using HybreDb.Storage;

namespace HybreDb.BPlusTree {
    public class BaseNode<TKey, TValue> : INode<TKey, TValue>
        where TKey : IComparable, IByteSerializable
        where TValue : IByteSerializable {
        protected SortedBuckets<TKey, INode<TKey, TValue>> _buckets;
        protected Tree<TKey, TValue> _tree;


        public BaseNode(Tree<TKey, TValue> t) {
            _tree = t;
            _buckets = new SortedBuckets<TKey, INode<TKey, TValue>>(t.BucketSize);
        }

        #region Internal inserts

        /// <summary>
        ///     Inserts a node into the bucket of the current node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public INode<TKey, TValue> InsertNode(INode<TKey, TValue> node) {
            Buckets.Add(node.HighestKey, node);
            if (Buckets.Count == Buckets.Capacity)
                return Split();
            return null;
        }

        /// <summary>
        ///     Removes a node from the bucket of current node
        /// </summary>
        public INode<TKey, TValue> RemoveNode(INode<TKey, TValue> node) {
            Buckets.Remove(node.HighestKey);

            return Count < Capacity/4 ? this : null;
        }

        #endregion

        #region Tree operations

        /// <summary>
        ///     Gets a value with given key
        /// </summary>
        public virtual bool TryGet(TKey key, out TValue val) {
            INode<TKey, TValue> n = Buckets.ValueAt(Buckets.NearestIndex(key));
            bool found = n.TryGet(key, out val);

            return found;
        }

        public virtual LeafNode<TKey, TValue> GetLeaf(TKey key) {
            INode<TKey, TValue> n = Buckets.ValueAt(Buckets.NearestIndex(key));
            LeafNode<TKey, TValue> v = n.GetLeaf(key);

            return v;
        }


        public virtual bool Update(TKey key, NodeUpdateHandler<TKey, TValue> h) {
            int idx = Buckets.NearestIndex(key);
            INode<TKey, TValue> n = Buckets.ValueAt(idx);
            bool c = n.Update(key, h);

            return c;
        }


        /// <summary>
        ///     Inserts data with given key
        /// </summary>
        /// <returns>Newly created ndoe due to splitting</returns>
        public virtual INode<TKey, TValue> Insert(TKey key, TValue data) {
            int idx = Buckets.NearestIndex(key);
            INode<TKey, TValue> n = Buckets.ValueAt(idx);
            INode<TKey, TValue> _n = n.Insert(key, data);

            // Always update the index, faster than branching
            Buckets.Set(idx, n.HighestKey, n);

            if (_n == null) return null;


            _n.BeginAccess();
            INode<TKey, TValue> res = InsertNode(_n);
            _n.EndAccess();

            return res;
        }

        /// <summary>
        ///     Recursively remove, if needed borrow, merge or remove the node.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual RemoveResult Remove(TKey key) {
            int idx = Buckets.NearestIndex(key);
            INode<TKey, TValue> node = Buckets.ValueAt(idx);

            RemoveResult t = node.Remove(key);

            // Tries to flatten the tree by removing a useless node.
            if ((t == RemoveResult.Merged || t == RemoveResult.Removed) && node.Count == 1) {
                Buckets.RemoveIndex(idx);

                // Remove empty node
                if (node.First.Count == 0) {
                    node.First.Dispose();
                }
                else
                    InsertNode(node.First);

                node.Dispose();
                return RemoveResult.Removed;
            }

            // Nothing to do node is big enough
            if (node.Count >= node.Capacity/4) {
                Buckets.Set(idx, node.HighestKey, node);

                return RemoveResult.None;
            }

            // Get the left and right neighbour and tries to borrow keys
            INode<TKey, TValue> l = idx > 0 ? Buckets.ValueAt(idx - 1) : null;
            INode<TKey, TValue> r = idx < Capacity - 1 ? Buckets.ValueAt(idx + 1) : null;

            if (node.Borrow(l, r)) {
                Buckets.Set(idx, node.HighestKey, node);
                if (l != null) Buckets.Set(idx - 1, l.HighestKey, l);

                return RemoveResult.Borrowed;
            }

            // Borrow failed, try to merge to the right
            if (r != null && node.Merge(r)) {
                Buckets.RemoveIndex(idx);
                node.Dispose();
                return RemoveResult.Merged;
            }

            // Cannot borrow but deleting the leafnode without neighbours will result in a broken tree
            if (node is LeafNode<TKey, TValue> && node.Count == 0 && l == null && r == null) {
                return RemoveResult.Merged;
            }

            // Remove empty base node or leafnode with neighbours
            if (node.Count == 0) {
                Buckets.RemoveIndex(idx);
                node.Dispose();
                return RemoveResult.Removed;
            }

            return RemoveResult.None;
        }

        #endregion

        #region Split/Merge

        public virtual INode<TKey, TValue> Split() {
            BaseNode<TKey, TValue> n = Tree.CreateBaseNode();
            n._buckets = Buckets.SliceEnd(Capacity/2);

            return n;
        }

        public virtual bool Merge(INode<TKey, TValue> n) {
            var _n =  n as BaseNode<TKey, TValue>;
            if (_n == null) return false;

            _n.Buckets.AddBegin(Buckets);

            return true;
        }

        public virtual bool Borrow(INode<TKey, TValue> left, INode<TKey, TValue> right) {
            var l = left as BaseNode<TKey, TValue>;
            var r = right as BaseNode<TKey, TValue>;

            SortedBuckets<TKey, INode<TKey, TValue>> s;
            if (l != null && l.Count - 1 - l.Capacity/4 > Capacity/4) {
                s = l.Buckets.SliceEnd(l.Buckets.Count - Capacity/4);
                Buckets.AddBegin(s);

                return true;
            }
            if (r == null || r.Count - 1 - r.Capacity/4 <= Capacity/4) return false;

            s = r.Buckets.SliceBegin(Capacity/4);
            Buckets.AddEnd(s);

            return true;
        }

        #endregion

        public virtual SortedBuckets<TKey, INode<TKey, TValue>> Buckets {
            get { return _buckets; }
        }

        public int Count {
            get { return Buckets.Count; }
        }

        public int Capacity {
            get { return Tree.BucketSize; }
        }

        public TKey HighestKey {
            get { return Buckets.KeyAt(Buckets.Count - 1); }
        }

        public TKey LowestKey {
            get { return Buckets.KeyAt(0); }
        }

        public INode<TKey, TValue> First {
            get { return Buckets.ValueAt(0); }
        }

        public LeafNode<TKey, TValue> FirstLeaf {
            get {
                INode<TKey, TValue> n = First;
                return n.FirstLeaf;
            }
        }

        public NodeTypes Type {
            get { return NodeTypes.Base; }
        }

        public Tree<TKey, TValue> Tree {
            get { return _tree; }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void BeginAccess() {}
        public virtual void EndAccess(bool isChanged = false) {}

        public void Serialize(BinaryWriter wrtr) {
            throw new NotImplementedException();
        }

        public void Deserialize(BinaryReader rdr) {
            throw new NotImplementedException();
        }

        public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            return Buckets.Values.SelectMany(n => n).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        protected virtual void Dispose(bool disposing) {
            if (_buckets != null) _buckets.Dispose();
            _buckets = null;
            _tree = null;
        }
    }
}