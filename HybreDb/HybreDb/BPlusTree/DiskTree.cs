using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Storage;

namespace HybreDb.BPlusTree {
    public class DiskTree<T> : Tree<T>
        where T : ITreeSerializable
    {

        public string Filename { get; private set; }

        private FileStream strm;

        public DiskTree(string filename, int size) : base(size) {
            Filename = filename;

            strm = new FileStream(Filename, FileMode.Append);
        }

        #region Creators overrides
        public override BaseNode<T> CreateBaseNode() {
            return new DiskBaseNode<T>(this);
        }

        public override LeafNode<T> CreateLeafNode(LeafNode<T> prev = null, LeafNode<T> next = null) {
            return new DiskLeafNode<T>(this) { Prev =  prev, Next = next };
        }
        
        #endregion
    }
}
