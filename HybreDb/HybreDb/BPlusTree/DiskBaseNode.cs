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

        public SortedBuckets<TKey, INode<TKey, TValue>> Buckets {
            get { 
                Read();
                return _buckets;
            }
        }

        public long FileOffset { get; private set; }
        public NodeState State { get; private set; }

        public INode<TKey, TValue> First { get { Read(); return Buckets.ValueAt(0); } }

        public LeafNode<TKey, TValue> FirstLeaf {
            get {
                var n = First;
                var _n = n.FirstLeaf;
                n.Accessed();

                return _n;
            }
        }

        public DiskTree<TKey, TValue> DiskTree { get; private set; }

        public DiskBaseNode(DiskTree<TKey, TValue> t, long offset)
            : this(t) {
                DiskTree = t;
                FileOffset = offset;
                State = NodeState.OnDisk;
            
                Free();
        }

        public DiskBaseNode(DiskTree<TKey, TValue> t)
            : base(t) {
                DiskTree = t;
                State = NodeState.Changed;
        }

        #region Tree operations
        public override bool TryGet(TKey key, out TValue val) {
            Read();

            State = NodeState.Changed;

            return base.TryGet(key, out val);
        }

        public override INode<TKey, TValue> Insert(TKey key, TValue data) {
            Read();

            State = NodeState.Changed;

            return base.Insert(key, data);
        }

        public override RemoveResult Remove(TKey k) {
            Read();

            State = NodeState.Changed;

            return base.Remove(k);
        }

        public override LeafNode<TKey, TValue> GetLeaf(TKey k) {
            Read();

            return base.GetLeaf(k);
        }
        #endregion

        /// <summary>
        /// Frees the resources from the node.
        /// </summary>
        public void Free() {
            _buckets.Dispose();
            _buckets = null;
            State = NodeState.OnDisk;
        }

        #region Reading/Writing

        public void Write() {
            DiskTree.Stream.Seek(0, SeekOrigin.End);
            Write(new BinaryWriter(DiskTree.Stream));
        }

        public void Write(BinaryWriter wrtr) {
            if (State != NodeState.Changed) return;

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

            DiskTree.Stream.Position = FileOffset;
            var rdr = new BinaryReader(DiskTree.Stream);
            _buckets = new SortedBuckets<TKey, INode<TKey, TValue>>(rdr, 
                _rdr => { var v = new TKey(); v.Deserialize(_rdr); return v; },
                _rdr => DiskNode<TKey, TValue>.Create(DiskTree, rdr)
            );

            State = NodeState.Loaded;
        }

        /// <summary>
        /// Serializes the node  with its type and the offset of the data bucket within the file
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


        /// <summary>
        /// Upon a access update the cache
        /// </summary>
        public void Accessed() {
            DiskTree.Cache.Update(this);
        }

        public void Changed() {
            State = NodeState.Changed;
        }

        public override void Dispose() {
            DiskTree.Cache.Remove(this);

            base.Dispose();
        }

        public override IEnumerator<TValue> GetEnumerator() {
            Read();

            return base.GetEnumerator();
        }


    }
}
