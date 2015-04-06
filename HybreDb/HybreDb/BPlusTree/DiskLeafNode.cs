using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Storage;

namespace HybreDb.BPlusTree {
    public class DiskLeafNode<TKey, TValue> : LeafNode<TKey, TValue>, IDiskNode<TKey, TValue>
        where TKey : ITreeSerializable, IComparable
        where TValue : ITreeSerializable
    {
        
        public long FileOffset { get; private set; }
        public NodeState State { get; private set; }

        private DiskTree<TKey, TValue> _tree;

        public DiskLeafNode(DiskTree<TKey, TValue> t, int offset)
            : this(t) {
            FileOffset = offset;
            State = NodeState.OnDisk;

            Free();
        }

        public DiskLeafNode(DiskTree<TKey, TValue> t)
            : base(t) {
            _tree = t;
        }

        public void Free() {
            Data.Dispose();
            Data = null;
            State = NodeState.OnDisk;
        }

        public void Write(BinaryWriter wrtr) {
            FileOffset = wrtr.BaseStream.Position;
            Data.Serialize(wrtr);
        }

        public void Read(Stream strm) {

        }


        public void Serialize(BinaryWriter wrtr) {
            wrtr.Write((byte)DiskNode.Types.LeafNode);
            wrtr.Write(FileOffset);
        }

        public void Deserialize(BinaryReader rdr) {
            throw new NotImplementedException();
        }
    }
}
