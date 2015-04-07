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
        where TKey : ITreeSerializable, IComparable, new()
        where TValue : ITreeSerializable, new()
    {

        public long FileOffset { get; private set; }
        public NodeState State { get; private set; }

        public INode<TKey, TValue> First { get { Read(); return Buckets.ValueAt(0); } }
        
        private DiskTree<TKey, TValue> _tree;

        public DiskBaseNode(DiskTree<TKey, TValue> t, long offset)
            : this(t) {
                _tree = t;
            FileOffset = offset;
                State = NodeState.OnDisk;
            
                Free();
        }

        public DiskBaseNode(DiskTree<TKey, TValue> t)
            : base(t) {
                _tree = t;
                State = NodeState.Changed;
        }

        public void Free() {
            Buckets.Dispose();
            Buckets = null;
            State = NodeState.OnDisk;
        }


        public void Write(BinaryWriter wrtr) {
            if (State == NodeState.OnDisk) return;

            // First make sure all children are written to file
            foreach (var n in Buckets)
                ((IDiskNode<TKey, TValue>)n.Value).Write(wrtr);

            FileOffset = wrtr.BaseStream.Position;
            Buckets.Serialize(wrtr);

            Free();
        }

        public void Read() {
            if (State != NodeState.OnDisk) return;

            _tree.Stream.Position = FileOffset;
            var rdr = new BinaryReader(_tree.Stream);
            Buckets = new SortedBuckets<TKey, INode<TKey, TValue>>(rdr, 
                _rdr => { var v = new TKey(); v.Deserialize(_rdr); return v; },
                _rdr => DiskNode<TKey, TValue>.Create(_tree, rdr)
            );
        }

        public void Serialize(BinaryWriter wrtr) {
            wrtr.Write((byte)Type);
            wrtr.Write(FileOffset);
        }

        public void Deserialize(BinaryReader rdr) {
            rdr.BaseStream.Position += 1;
            FileOffset = rdr.ReadInt64();
        }
    }
}
