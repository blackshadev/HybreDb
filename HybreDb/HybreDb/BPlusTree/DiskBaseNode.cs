using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybreDb.BPlusTree {
    class DiskBaseNode<T> : BaseNode<T>, IDiskNode<T> {


        public int FileOffset { get; private set; }
        public NodeState State { get; private set; }

        private DiskTree<T> _tree; 

        public DiskBaseNode(DiskTree<T> t) : base(t) {
            _tree = t;
        }


        public void Write() {
            throw new NotImplementedException();
        }

        public void Read() {
            throw new NotImplementedException();
        }
    }
}
