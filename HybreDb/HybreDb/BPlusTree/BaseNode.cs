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
        public int Size;

        public int HighestKey {
            get { return Buckets.KeyAt(Buckets.Count - 1); }
        }

        public int LowestKey {
            get { return Buckets.KeyAt(0); }
        }


        public BaseNode(int size) {
            Size = size;
            Buckets = new SortedBuckets<int, INode<T>>(size);
        }

        /// <summary>
        /// Iterate over the bucket to find the next node
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public INode<T> Select(int key) {
            return Buckets.ValueAt(Buckets.NearestIndex(key));
        }

        public INode<T> InsertNode(INode<T> node) {
            Buckets.Add(node.HighestKey, node);
            if (Buckets.Count == Buckets.Capacity)
                return Split();
            return null;
        }

        public T Get(int key) {
            return Select(key).Get(key);
        }


        public INode<T> Insert(int key, T data) {
            var idx = Buckets.NearestIndex(key);
            var n = Buckets.ValueAt(idx);
            var _n = n.Insert(key, data);

            Buckets.Set(idx, n.HighestKey, n);

            // TODO upon changing the highest value, parent need to change

            if (_n != null)
                return InsertNode(_n);
            return null;
        }

        public INode<T> Delete(int key) {
            return Select(key).Delete(key);
            // TODO need merge here?
        }


        public INode<T> Split() {
            return new BaseNode<T>(Size) { Buckets = Buckets.Slice(Size/2) };
        }

        public INode<T> Merge() {
            throw new NotImplementedException();
        }
    }


}
