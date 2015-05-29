﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.BPlusTree.Collections;
using HybreDb.Storage;

namespace HybreDb.BPlusTree.Collections {

    public class LRUCacheEventArgs<T> : EventArgs {
        public T Data;
    }

    public class LRUCache<T> {

        public delegate void OutDatedHandler(object sender, LRUCacheEventArgs<T> e);

        public event OutDatedHandler OnOutDated;

        public int Capacity { get; private set; }
        public int Count { get { return Lookup.Count; } }


        protected DoubleLinkedList<T> List;
        protected Dictionary<T, LinkedNode<T>> Lookup;

        public LRUCache(int size) {
            List = new DoubleLinkedList<T>();
            Lookup = new Dictionary<T, LinkedNode<T>>(size);
            Capacity = size;
        }

        public void Update(T n) {
            LinkedNode<T> l;
            var exists = Lookup.TryGetValue(n, out l);

            if(exists)
                UpdateExisting(l);
            else
                AddNew(n);

        }

        public void Remove(T d) {
            LinkedNode<T> l;

            if (!Lookup.TryGetValue(d, out l)) return;

            Lookup.Remove(d);
            List.Unlink(l);
        }

        protected void UpdateExisting(LinkedNode<T> n) {
            
            List.Unlink(n);

            List.AddHead(n);
        }

        protected void AddNew(T k) {
            var l = (Count >= Capacity) ? List.RemoveTail() : null;

            if (l != null) {
                Lookup.Remove(l.Data);
                if (OnOutDated != null) OnOutDated(this, new LRUCacheEventArgs<T> {Data = l.Data});
            }

            l = List.AddHead(k);
            Lookup.Add(k, l);
        }
    }
}
