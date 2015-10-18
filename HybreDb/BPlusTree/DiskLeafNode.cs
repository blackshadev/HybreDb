using System;
using System.Collections.Generic;
using System.IO;
using HybreDb.BPlusTree.Collections;
using HybreDb.Storage;

namespace HybreDb.BPlusTree {
    /// <summary>
    ///     LeafNode wish can be read and written to disk
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TKey">Value type</typeparam>
    public class DiskLeafNode<TKey, TValue> : LeafNode<TKey, TValue>, IDiskNode<TKey, TValue>
        where TKey : IByteSerializable, IComparable, new()
        where TValue : IByteSerializable, new() {
        public static int Total = 0;
        protected NodeState OldState;

        protected int accesses = 0;

        public DiskLeafNode(DiskTree<TKey, TValue> t, long offset)
            : this(t) {
            FileOffset = offset;
            State = NodeState.OnDisk;

            Free();
        }

        public DiskLeafNode(DiskTree<TKey, TValue> t)
            : base(t) {
            DiskTree = t;
            State = NodeState.Changed;
            Total += 1;
        }

        #region Tree operations

        public override bool TryGet(TKey key, out TValue val) {
            BeginAccess();
            bool r = base.TryGet(key, out val);
            EndAccess();
            return r;
        }

        public override bool Update(TKey k, NodeUpdateHandler<TKey, TValue> h) {
            BeginAccess();
            bool r = base.Update(k, h);
            EndAccess(r);
            return r;
        }


        public override INode<TKey, TValue> Insert(TKey key, TValue data) {
            BeginAccess();
            INode<TKey, TValue> r = base.Insert(key, data);
            EndAccess(true);
            return r;
        }

        public override RemoveResult Remove(TKey k) {
            BeginAccess();
            RemoveResult r = base.Remove(k);
            EndAccess(true);
            return r;
        }

        public override LeafNode<TKey, TValue> GetLeaf(TKey k) {
            BeginAccess();
            LeafNode<TKey, TValue> r = base.GetLeaf(k);
            EndAccess();
            return r;
        }

        #endregion

        #region Split/Merge

        public override INode<TKey, TValue> Split() {
            BeginAccess();
            INode<TKey, TValue> n = base.Split();
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

        #region Reading/Writing/// <summary>

        /// Free all resources hold bu this node
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

        /// <summary>
        ///     Writes the data from the node to the given stream.
        /// </summary>
        public void Write(BinaryWriter wrtr) {
            if (IsBusy || State != NodeState.Changed) return;

            DiskTree.Writes++;

            FileOffset = wrtr.BaseStream.Position;
            _buckets.Serialize(wrtr);

            Free();
        }


        /// <summary>
        ///     Reads the data into the node with given reader
        /// </summary>
        public void Read() {
            if (State != NodeState.OnDisk) return;

            DiskTree.Reads++;

            var rdr = new BinaryReader(DiskTree.Stream);
            rdr.BaseStream.Seek(FileOffset, SeekOrigin.Begin);

            //var offs = rdr.ReadInt64();
            _buckets = new SortedBuckets<TKey, TValue>(rdr,
                _rdr => {
                    var v = new TKey();
                    v.Deserialize(_rdr);
                    return v;
                },
                _rdr => {
                    var v = new TValue();
                    DiskTree.DataRead(v);
                    v.Deserialize(_rdr);
                    return v;
                }
                );
            //if (offs > -1) {
            //    Next = DiskTree.CreateLeafNode(offs);
            //    Next.Prev = this;
            //}

            State = NodeState.Loaded;
        }


        /// <summary>
        ///     Serializes the node, the node is represented by it's node type and the offset of it's data within the file
        /// </summary>
        public new void Serialize(BinaryWriter wrtr) {
            wrtr.Write((byte) Type);
            wrtr.Write(FileOffset);
        }

        /// <summary>
        ///     Not implemeted because these nodes are created via DiskTree.Create
        /// </summary>
        public new void Deserialize(BinaryReader rdr) {
            throw new NotImplementedException();
        }

        #endregion

        public override SortedBuckets<TKey, TValue> Buckets {
            get { return _buckets; }
        }

        public bool IsBusy {
            get { return accesses > 0; }
        }

        public long FileOffset { get; private set; }
        public NodeState State { get; private set; }

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