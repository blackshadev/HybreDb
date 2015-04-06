using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Storage;

namespace HybreDb.BPlusTree {
    public class DiskTree<TKey, TValue> : Tree<TKey, TValue>
        where TKey : IComparable, ITreeSerializable
        where TValue : ITreeSerializable
    {

        public string Filename { get; private set; }

        private FileStream strm;

        public DiskTree(string filename, int size) : base(size) {
            Filename = filename;

            strm = new FileStream(Filename, FileMode.Append);
        }

        #region Creators overrides
        public override BaseNode<TKey, TValue> CreateBaseNode() {
            return new DiskBaseNode<TKey, TValue>(this);
        }

        public override LeafNode<TKey, TValue> CreateLeafNode(LeafNode<TKey, TValue> prev = null, LeafNode<TKey, TValue> next = null) {
            return new DiskLeafNode<TKey, TValue>(this) { Prev = prev, Next = next };
        }
        
        #endregion
    }
}
