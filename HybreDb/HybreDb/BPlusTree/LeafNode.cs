using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace HybreDb.BPlusTree {
    public class LeafNode<T> : INode<T> {
        /// <summary>
        /// Contains the actual stored data
        /// </summary>
        public SortedBuckets<int, T> Data;
        /// <summary>
        /// Max size, equal to Data.Capacity
        /// </summary>
        public int Size;
        /// <summary>
        /// Highest key in Data
        /// </summary>
        public int HighestKey {
            get { return Data.KeyAt(Data.Count - 1); }
        }
        public int LowestKey {
            get { return Data.KeyAt(0); }
        }

        /// <summary>
        /// Pointer to the next leaf node
        /// </summary>
        public LeafNode<T> Next; 

        public LeafNode(int size) {
            Size = size;
            Data = new SortedBuckets<int, T>(size);
        } 

        public INode<T> Select(int k) {
            return this;
        }

        public INode<T> Delete(int k) {
            Data.Remove(k);

            if (Data.Count < Data.Capacity/3)
                return Merge();
            return null;
        }

        public INode<T> Insert(int key, T data) {
            Data.Add(key, data);
            if (Data.Count == Size)
                return Split();
            return null;
        }


        public T Get(int key) {
            return Data.TryGetValue(key);
        }

        public INode<T> Merge() {
            throw new NotImplementedException();    
        } 

        public INode<T> Split() {
            var node = new LeafNode<T>(Size);
            node.Data = Data.Slice(Size / 2);

            node.Next = Next;
            Next = node;

            return node;
        }

    }
}
