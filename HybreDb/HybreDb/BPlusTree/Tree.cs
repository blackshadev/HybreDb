using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybreDb.BPlusTree {
    public class Tree<T> : IEnumerable<T> {

        public int BucketSize { get; private set; }
        public INode<T> Root { get; private set; }

        public Tree() {
            BucketSize = 50;
            Root = new LeafNode<T>(BucketSize);
        }

        public Tree(int bucketSize) {
            BucketSize = bucketSize;
            if(BucketSize < 4) 
                throw new ArgumentException("The minimal allowed bucketsize is 4");
            Root = new LeafNode<T>(bucketSize);
        }

        /// <summary>
        /// Creates a new root node of 2 given nodes
        /// </summary>
        /// <param name="l">left (lower) node</param>
        /// <param name="r">right (upper) node</param>
        /// <returns>Newly created root node</returns>
        public INode<T> NewRootNode(INode<T> l, INode<T> r) {
            var n = new BaseNode<T>(BucketSize);
            n.InsertNode(l);
            n.InsertNode(r);

            return n;
        }

        public void Insert(object k, T val) {
            int key = k.GetHashCode();
            var n = Root.Insert(key, val);

            if (n != null)
                Root = NewRootNode(Root, n);
        }


        public T this[object k] {
            get { return Root.Get(k.GetHashCode()); }
            //set { Root.Insert(k.GetHashCode(), value); }
        }

        protected LeafNode<T> GetFirstLeaf() {
            var n = Root;

            while (!(n is LeafNode<T>))
                n = n.Select(n.LowestKey);
            
            return (LeafNode<T>)n;
        }

        public IEnumerator<T> GetEnumerator() {
            var n = GetFirstLeaf();

            do {
                foreach (var v in n.Data.Values)
                    yield return v;
            } while( (n = n.Next) != null );
            
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}