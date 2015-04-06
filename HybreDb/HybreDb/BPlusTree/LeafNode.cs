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
        where TKey : IComparable, ITreeSerializable
        where TValue : ITreeSerializable
    {
        public SortedBuckets<TKey, TValue> Data;

        public int Count { get { return Data.Count; } }

        public INode<TKey, TValue> First { get { return this; } }

        public Tree<TKey, TValue> Tree { get { return _tree; } }
        private Tree<TKey, TValue> _tree; 

        public int Capacity { get { return Data.Capacity; } }
        public TKey HighestKey { get { return Data.KeyAt(Data.Count - 1); } }
        public TKey LowestKey { get { return Data.KeyAt(0); } }

        /// <summary>
        /// Pointer to the next leaf node
        /// </summary>
        public LeafNode<TKey, TValue> Next;

        public LeafNode<TKey, TValue> Prev;

        public LeafNode(Tree<TKey, TValue> t ) {
            _tree = t;
            Data = t.CreateLeafNodeBuckets();
        }

        public INode<TKey, TValue> Select(TKey k) {
            return this;
        }

        public RemoveResult Remove(TKey k) {
            Data.Remove(k);

            return RemoveResult.None;
        }

        public INode<TKey, TValue> Insert(TKey key, TValue data) {
            Data.Add(key, data);
            if (Data.Count == Capacity)
                return Split();
            return null;
        }


        public TValue Get(TKey key) {
            return Data.TryGetValue(key);
        }

        public INode<TKey, TValue> Split() {
            var node = Tree.CreateLeafNode(this, Next);
            node.Data = Data.SliceEnd(Capacity / 2);
            
            Next = node;

            return node;
        }


        public bool Borrow(INode<TKey, TValue> left, INode<TKey, TValue> right) {
            var l = left as LeafNode<TKey, TValue>;
            var r = right as LeafNode<TKey, TValue>;

            SortedBuckets<TKey, TValue> s;
            if (l != null && l.Count - 1 - l.Capacity / 4 > Capacity / 4) {
                s = l.Data.SliceEnd(l.Count - Capacity / 4);
                Data.AddBegin(s);

                return true;
            }
            if (r == null || r.Count - 1 - r.Capacity / 4 <= Capacity/4) return false;

            s = r.Data.SliceBegin(Capacity / 4);
            Data.AddEnd(s);

            return true;
        }

        public bool Merge(INode<TKey, TValue> n) {
            if (!(n is LeafNode<TKey, TValue>)) return false;
            var _n = (LeafNode<TKey, TValue>)n;

            _n.Data.AddBegin(Data);
            
            Dispose();
            
            return true;
        } 

        public void Dispose() {
            if (Next != null) Next.Prev = Prev;
            if (Prev != null) Prev.Next = Next;

            Data.Dispose();
            Next = null;
        }

        public void Serialize(BinaryWriter wrtr) {
            throw new NotImplementedException();
        }

        public void Deserialize(BinaryReader rdr) {
            throw new NotImplementedException();
        }
    }
}
