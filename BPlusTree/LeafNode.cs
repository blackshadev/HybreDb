using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Storage;

namespace HybreDb.BPlusTree {
    public class LeafNode<TKey, TValue> : INode<TKey, TValue>
        where TKey : IComparable, IByteSerializable
        where TValue : IByteSerializable 
    {

        protected SortedBuckets<TKey, TValue> _buckets; 
        public virtual SortedBuckets<TKey, TValue> Buckets { 
            get { return _buckets; }
        }

        public int Count { get { return Buckets.Count; } }

        public INode<TKey, TValue> First { get { return this; } }
        public LeafNode<TKey, TValue> FirstLeaf { get { return this; } } 

        public NodeTypes Type { get { return NodeTypes.Leaf; } }

        public Tree<TKey, TValue> Tree { get { return _tree; } }
        private Tree<TKey, TValue> _tree; 

        public int Capacity { get { return Buckets.Capacity; } }
        public TKey HighestKey { get { return Buckets.KeyAt(Buckets.Count - 1); } }
        public TKey LowestKey { get { return Buckets.KeyAt(0); } }


        public LeafNode(Tree<TKey, TValue> t ) {
            _tree = t;
            _buckets = new SortedBuckets<TKey, TValue>(Tree.BucketSize);
        }

        #region Tree operations
        public virtual RemoveResult Remove(TKey k) {
            Buckets.Remove(k);
            
            return RemoveResult.None;
        }

        public virtual LeafNode<TKey, TValue> GetLeaf(TKey k) {
            return this;
        }

        public virtual bool Update(TKey k, NodeUpdateHandler<TKey, TValue> h) {
            TValue v;
            Buckets.TryGetValue(k, out v);
            return h(this, k, v);
        }

        public virtual INode<TKey, TValue> Insert(TKey key, TValue data) {
            Buckets.Add(key, data);
           
            return Buckets.Count == Capacity ? Split() : null;
        }


        public virtual bool TryGet(TKey key, out TValue val) {
            return Buckets.TryGetValue(key, out val);
        }

        #endregion

        #region Split/Merge
        public virtual INode<TKey, TValue> Split() {
            var node = Tree.CreateLeafNode();
            node._buckets = Buckets.SliceEnd(Capacity / 2);
            
            return node;
        }

        public virtual bool Borrow(INode<TKey, TValue> left, INode<TKey, TValue> right) {
            var l = left as LeafNode<TKey, TValue>;
            var r = right as LeafNode<TKey, TValue>;

            SortedBuckets<TKey, TValue> s;
            if (l != null && l.Count - 1 - l.Capacity / 4 > Capacity / 4) {
                s = l.Buckets.SliceEnd(l.Count - Capacity / 4);
                Buckets.AddBegin(s);

                return true;
            }
            if (r == null || r.Count - 1 - r.Capacity / 4 <= Capacity/4) return false;

            s = r.Buckets.SliceBegin(Capacity / 4);
            Buckets.AddEnd(s);

            return true;
        }

        public virtual bool Merge(INode<TKey, TValue> n) {
            if (!(n is LeafNode<TKey, TValue>)) return false;
            var _n = (LeafNode<TKey, TValue>)n;

            _n.Buckets.AddBegin(Buckets);
            
            Dispose();
            
            return true;
        }
        #endregion


        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (_buckets != null) _buckets.Dispose();
            _buckets = null;
            _tree = null;
        }

        public void Serialize(BinaryWriter wrtr) {
            throw new NotImplementedException();
        }

        public void Deserialize(BinaryReader rdr) {
            throw new NotImplementedException();
        }

        public virtual void BeginAccess() { }
        public virtual void EndAccess(bool isChanged = false) { }


        public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            return Buckets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
