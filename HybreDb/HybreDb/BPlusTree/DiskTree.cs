using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybreDb.BPlusTree {
    public class DiskTree<T> : Tree<T>  {

        public string Filename { get; private set; }

        private FileStream strm;

        public DiskTree(string filename, int size) : base(size) {
            Filename = filename;

            strm = new FileStream(Filename, FileMode.Append);
        }


        public override BaseNode<T> CreateBaseNode() {
            return base.CreateBaseNode();
        }

        public override LeafNode<T> CreateLeafNode(LeafNode<T> prev = null, LeafNode<T> next = null) {
            return base.CreateLeafNode(prev, next);
        }

    }
}
