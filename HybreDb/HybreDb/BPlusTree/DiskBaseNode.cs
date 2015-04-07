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

        #region Tree operations
        public override TValue Get(TKey key) {
            Read();
            return base.Get(key);
        }
        public override INode<TKey, TValue> Insert(TKey key, TValue data) {
            Read();
            return base.Insert(key, data);
        }
        public override RemoveResult Remove(TKey k) {
            Read();
            return base.Remove(k);
        }
        #endregion

        /// <summary>
        /// Frees the resources from the node.
        /// </summary>
        public void Free() {
            Buckets.Dispose();
            Buckets = null;
            State = NodeState.OnDisk;
        }

        #region Reading/Writing
        public void Write(BinaryWriter wrtr) {
            if (State == NodeState.OnDisk) return;

            // First make sure all children are written to file
            foreach (var n in Buckets)
                ((IDiskNode<TKey, TValue>)n.Value).Write(wrtr);

            FileOffset = wrtr.BaseStream.Position;
            Buckets.Serialize(wrtr);

            Free();
        }


        /// <summary>
        /// Reads the data into the node with the given offset within the file.
        /// </summary>
        public void Read() {
            if (State != NodeState.OnDisk) return;

            _tree.Stream.Position = FileOffset;
            var rdr = new BinaryReader(_tree.Stream);
            Buckets = new SortedBuckets<TKey, INode<TKey, TValue>>(rdr, 
                _rdr => { var v = new TKey(); v.Deserialize(_rdr); return v; },
                _rdr => DiskNode<TKey, TValue>.Create(_tree, rdr)
            );

            State = NodeState.Loaded;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Serialize(BinaryWriter wrtr) {
            wrtr.Write((byte)Type);
            wrtr.Write(FileOffset);
        }

        /// <summary>
        /// Not implemeted because these nodes are created via DiskNode.Create
        /// </summary>
        public void Deserialize(BinaryReader rdr) {
            throw new NotImplementedException();
        }
        #endregion
    }
}
