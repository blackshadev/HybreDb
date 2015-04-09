using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.BPlusTree.Collections;
using HybreDb.Storage;

namespace HybreDb.BPlusTree.Collections {
    public class LRUCache<T> {

        public delegate void RemoveHandler(T dat);

        public event RemoveHandler OnRemoved;

        public int Capacity { get; private set; }
        public int Count { get; private set; }


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

        protected void UpdateExisting(LinkedNode<T> n) {
            n.Unlink();
            List.AddHead(n);
        }

        protected void AddNew(T k) {
            var l = (Count == Capacity) ? List.RemoveTail() : null;

            if (l != null) {
                Lookup.Remove(l.Data);
                if (OnRemoved != null) OnRemoved(l.Data);
            }

            l = List.AddHead(k);
            Lookup.Add(k, l);
        }
    }
}
