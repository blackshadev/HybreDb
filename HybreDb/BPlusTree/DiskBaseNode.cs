using System;
using System.Collections.Generic;
using System.IO;
using HybreDb.BPlusTree.Collections;
using HybreDb.Storage;

namespace HybreDb.BPlusTree {
    internal class DiskBaseNode<TKey, TValue> : BaseNode<TKey, TValue>, IDiskNode<TKey, TValue>
        where TKey : IByteSerializable, IComparable, new()
        where TValue : IByteSerializable, new() {
        protected int accesses = 0;

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
            BeginAccess();
            bool r = base.TryGet(key, out val);
            EndAccess();

            return r;
        }

        public override bool Update(TKey key, NodeUpdateHandler<TKey, TValue> h) {
            BeginAccess();
            bool r = base.Update(key, h);
            EndAccess(r);

            return r;
        }

        public override INode<TKey, TValue> Insert(TKey key, TValue data) {
            BeginAccess();
            INode<TKey, TValue> r = base.Insert(key, data);
            EndAccess(true);

            return r;
        }

        public override RemoveResult Remove(TKey key) {
            BeginAccess();

            RemoveResult r = base.Remove(key);
            EndAccess(true);
            return r;
        }

        public override LeafNode<TKey, TValue> GetLeaf(TKey key) {
            BeginAccess();
            LeafNode<TKey, TValue> r = base.GetLeaf(key);
            EndAccess();
            return r;
        }

        #endregion

        #region Split/Merge

        public override INode<TKey, TValue> Split() {
            BeginAccess();
            INode<TKey, TValue> n = base.Split();

            n.BeginAccess();
            n.EndAccess(true);
            EndAccess(true);

            return n;
        }

        public override bool Merge(INode<TKey, TValue> n) {
            BeginAccess();
            n.BeginAccess();
            bool r = base.Merge(n);
            n.EndAccess(r);
            EndAccess(r);
            return r;
        }

        public override bool Borrow(INode<TKey, TValue> left, INode<TKey, TValue> right) {
            BeginAccess();
            left.BeginAccess();
            right.BeginAccess();
            bool n = base.Borrow(left, right);
            right.EndAccess(n);
            left.EndAccess(n);
            EndAccess(n);
            return n;
        }

        #endregion

        #region Reading/Writing

        /// <summary>
        ///     Frees the resources from the node.
        /// </summary>
        public void Free() {
            DiskTree.Freed++;

            _buckets.Dispose();
            _buckets = null;
            State = NodeState.OnDisk;
        }


        public void Write() {
            if (IsBusy || State != NodeState.Changed) return;

            DiskTree.Stream.Seek(0, SeekOrigin.End);
            Write(new BinaryWriter(DiskTree.Stream));
        }

        public void Write(BinaryWriter wrtr) {
            if (IsBusy || State != NodeState.Changed) return;

            DiskTree.Writes++;

            // First make sure all children are written to file
            foreach (var n in Buckets)
                ((IDiskNode<TKey, TValue>) n.Value).Write(wrtr);

            FileOffset = wrtr.BaseStream.Position;
            Buckets.Serialize(wrtr);

            Free();
        }


        /// <summary>
        ///     Reads the data into the node with the given offset within the file.
        /// </summary>
        public void Read() {
            if (State != NodeState.OnDisk) return;

            DiskTree.Reads++;

            DiskTree.Stream.Position = FileOffset;
            var rdr = new BinaryReader(DiskTree.Stream);
            _buckets = new SortedBuckets<TKey, INode<TKey, TValue>>(rdr,
                _rdr => {
                    var v = new TKey();
                    v.Deserialize(_rdr);
                    return v;
                },
                _rdr => DiskNode<TKey, TValue>.Create(DiskTree, rdr)
                );

            State = NodeState.Loaded;
        }

        /// <summary>
        ///     Serializes the node  with its type and the offset of the data bucket within the file
        /// </summary>
        public new void Serialize(BinaryWriter wrtr) {
            wrtr.Write((byte) Type);
            wrtr.Write(FileOffset);
        }

        /// <summary>
        ///     Not implemeted because these nodes are created via DiskNode.Create
        /// </summary>
        public new void Deserialize(BinaryReader rdr) {
            throw new NotImplementedException();
        }

        #endregion

        public override SortedBuckets<TKey, INode<TKey, TValue>> Buckets {
            get { return _buckets; }
        }

        public bool IsBusy {
            get { return accesses > 0; }
        }

        public long FileOffset { get; private set; }
        public NodeState State { get; private set; }


        public new LeafNode<TKey, TValue> FirstLeaf {
            get {
                INode<TKey, TValue> n = First;
                LeafNode<TKey, TValue> _n = n.FirstLeaf;

                return _n;
            }
        }

        public DiskTree<TKey, TValue> DiskTree { get; private set; }

        public override void BeginAccess() {
            Read();
            accesses++;
        }

        public override void EndAccess(bool isChanged = false) {
            if (isChanged) Changed();
            accesses--;

            DiskTree.Cache.Update(this);
        }

        public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            BeginAccess();
            IEnumerator<KeyValuePair<TKey, TValue>> r = base.GetEnumerator();
            EndAccess();

            return r;
        }

        /// <summary>
        ///     Upon a access update the cache
        /// </summary>
        public void Accessed() {
            DiskTree.Cache.Update(this);
        }

        public void Changed() {
            State = NodeState.Changed;
        }

        protected override void Dispose(bool disposing) {
            DiskTree.Cache.Remove(this);
            DiskTree = null;

            base.Dispose(disposing);
        }
    }
}