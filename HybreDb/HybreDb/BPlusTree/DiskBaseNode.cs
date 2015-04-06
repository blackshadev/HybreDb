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
    class DiskBaseNode<TKey, TValue> : BaseNode<TKey, TValue>, IDiskNode<TKey, TValue>
        where TKey : ITreeSerializable, IComparable
        where TValue : ITreeSerializable
    {

        public long FileOffset { get; private set; }
        public NodeState State { get; private set; }

        public DiskBaseNode(DiskTree<TKey, TValue> t, int offset)
            : this(t) {
            FileOffset = offset;
            State = NodeState.OnDisk;
            
            Free();
        }

        public DiskBaseNode(DiskTree<TKey, TValue> t)
            : base(t) {
            State = NodeState.Changed;
        }

        public void Free() {
            Buckets.Dispose();
            Buckets = null;
            State = NodeState.OnDisk;
        }


        public void Write(BinaryWriter wrtr) {

            // First make sure all children are written to file
            foreach (var n in Buckets)
                ((IDiskNode<TKey, TValue>)n.Value).Write(wrtr);

            FileOffset = wrtr.BaseStream.Position;
            Buckets.Serialize(wrtr);
        }

        public void Read(Stream strm) {
            
        }

        public void Serialize(BinaryWriter wrtr) {
            wrtr.Write((byte)DiskNode.Types.BaseNode);
            wrtr.Write(FileOffset);
        }

        public void Deserialize(BinaryReader rdr) {
            throw new NotImplementedException();
        }
    }
}
