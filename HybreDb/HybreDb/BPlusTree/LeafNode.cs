﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace HybreDb.BPlusTree {
    public class LeafNode<T> : INode<T> {
        public SortedBuckets<int, T> Data;

        public int Count { get { return Data.Count; } }

        public INode<T> First { get { return this; } }

        public Tree<T> Tree { get { return _tree; } }
        private Tree<T> _tree; 

        public int Capacity { get { return Data.Capacity; } }
        public int HighestKey { get { return Data.KeyAt(Data.Count - 1); } }
        public int LowestKey { get { return Data.KeyAt(0); } }

        /// <summary>
        /// Pointer to the next leaf node
        /// </summary>
        public LeafNode<T> Next;

        public LeafNode<T> Prev;

        public LeafNode(Tree<T> t ) {
            _tree = t;
            Data = new SortedBuckets<int, T>(t.BucketSize);
        } 

        public INode<T> Select(int k) {
            return this;
        }

        public RemoveResult Remove(int k) {
            Data.Remove(k);

            return RemoveResult.None;
        }

        public INode<T> Insert(int key, T data) {
            Data.Add(key, data);
            if (Data.Count == Capacity)
                return Split();
            return null;
        }


        public T Get(int key) {
            return Data.TryGetValue(key);
        }

        public INode<T> Split() {
            var node = Tree.CreateLeafNode(this, Next);
            node.Data = Data.SliceEnd(Capacity / 2);
            
            Next = node;

            return node;
        }


        public bool Borrow(INode<T> left, INode<T> right) {
            var l = left as LeafNode<T>;
            var r = right as LeafNode<T>;

            SortedBuckets<int, T> s;
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

        public bool Merge(INode<T> n) {
            if (!(n is LeafNode<T>)) return false;
            var _n = (LeafNode<T>)n;

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

    }
}
