﻿using System;
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
        where TKey : IComparable, ITreeSerializable
        where TValue : ITreeSerializable 
    {

        protected SortedBuckets<TKey, TValue> _buckets; 
        public SortedBuckets<TKey, TValue> Buckets { 
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

        /// <summary>
        /// Pointer to the next leaf node
        /// </summary>
        public LeafNode<TKey, TValue> Next;

        public LeafNode<TKey, TValue> Prev;

        public LeafNode(Tree<TKey, TValue> t ) {
            _tree = t;
            _buckets = new SortedBuckets<TKey, TValue>(Tree.BucketSize);
        }

        #region Tree operations
        public virtual RemoveResult Remove(TKey k) {
            Buckets.Remove(k);
            Changed();

            return RemoveResult.None;
        }

        public virtual INode<TKey, TValue> Insert(TKey key, TValue data) {
            Buckets.Add(key, data);
            Changed();

            if (Buckets.Count == Capacity)
                return Split();
            return null;
        }


        public virtual TValue Get(TKey key) {
            return Buckets.TryGetValue(key);
        }
        #endregion

        #region Split/Merge
        public INode<TKey, TValue> Split() {
            var node = Tree.CreateLeafNode(this, Next);
            node._buckets = Buckets.SliceEnd(Capacity / 2);
            
            Next = node;

            Changed();
            node.Changed();
            
            return node;
        }

        public bool Borrow(INode<TKey, TValue> left, INode<TKey, TValue> right) {
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

        public bool Merge(INode<TKey, TValue> n) {
            if (!(n is LeafNode<TKey, TValue>)) return false;
            var _n = (LeafNode<TKey, TValue>)n;

            _n.Buckets.AddBegin(Buckets);
            
            Dispose();

            Changed();
            _n.Changed();
            
            return true;
        }
        #endregion

        public virtual void Dispose() {
            if (Next != null) Next.Prev = Prev;
            if (Prev != null) Prev.Next = Next;

            if(Buckets != null) Buckets.Dispose();
            Next = null;
        }

        public void Accessed() { }
        public void Changed() { }

        public void Serialize(BinaryWriter wrtr) {
            throw new NotImplementedException();
        }

        public void Deserialize(BinaryReader rdr) {
            throw new NotImplementedException();
        }


        public virtual IEnumerator<TValue> GetEnumerator() {
            var e = Buckets.Values.GetEnumerator();
            Accessed();
            return e;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
