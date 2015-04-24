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
        where TKey : IByteSerializable, IComparable, new()
        where TValue : IByteSerializable, new() 
    {
        protected int accesses = 0;
        public bool IsBusy { get {return accesses > 0; }  }

        public override SortedBuckets<TKey, INode<TKey, TValue>> Buckets {
            get { 
                return _buckets;
            }
        }

        public long FileOffset { get; private set; }
        public NodeState State { get; private set; }

        public INode<TKey, TValue> First { get { return Buckets.ValueAt(0); } }

        public LeafNode<TKey, TValue> FirstLeaf {
            get {
                var n = First;
                var _n = n.FirstLeaf;

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
            BeginAccess();
            var r = base.TryGet(key, out val);
            EndAccess();
            
            return r;
        }

        public override bool Update(TKey k, NodeUpdateHandler<TKey, TValue> h) {
            
            BeginAccess();
            var r = base.Update(k, h);
            EndAccess(r);
            
            return r;

        }

        public override INode<TKey, TValue> Insert(TKey key, TValue data) {

            BeginAccess();
            var r = base.Insert(key, data);
            EndAccess(true);

            return r;
        }

        public override RemoveResult Remove(TKey k) {
            BeginAccess();

            var r = base.Remove(k);
            EndAccess(true);
            return r;
        }

        public override LeafNode<TKey, TValue> GetLeaf(TKey k) {

            BeginAccess();
            var r = base.GetLeaf(k);
            EndAccess();
            return r;
        }
        #endregion


        #region Split/Merge
        public override INode<TKey, TValue> Split() {
            BeginAccess();
            var n = base.Split();
            
            n.BeginAccess();
            n.EndAccess(true);
            EndAccess(true);

            return n;
        }

        public bool Merge(INode<TKey, TValue> n) {
            BeginAccess();
            n.BeginAccess();
            var r = base.Merge(n);
            n.EndAccess(r);
            EndAccess(r);
            return r;
        }

        public bool Borrow(INode<TKey, TValue> l, INode<TKey, TValue> r) {
            BeginAccess();
            l.BeginAccess();
            r.BeginAccess();
            var n = base.Borrow(l, r);
            r.EndAccess(n);
            l.EndAccess(n);
            EndAccess(n);
            return n;
        }

        #endregion

        #region Reading/Writing

        /// <summary>
        /// Frees the resources from the node.
        /// </summary>
        public void Free() {
            _buckets.Dispose();
            _buckets = null;
            State = NodeState.OnDisk;
        }



        public void Write() {
            if (State != NodeState.Changed || IsBusy) return;
            DiskTree.Stream.Seek(0, SeekOrigin.End);
            Write(new BinaryWriter(DiskTree.Stream));
        }

        public void Write(BinaryWriter wrtr) {
            if (State != NodeState.Changed || IsBusy) return;


            DiskTree.OpenNodes--;

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

            DiskTree.OpenNodes++;

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
        
        public override void BeginAccess() {
            Read();
            accesses++;
            DiskTree.BusyNodes++;
        }

        public override void EndAccess(bool isChanged = false) {
            if (isChanged) Changed();
            accesses--;
            DiskTree.BusyNodes--;

            DiskTree.Cache.Update(this);
        }

        public void Changed() {
            State = NodeState.Changed;
        }

        public override void Dispose() {
            DiskTree.Cache.Remove(this);

            base.Dispose();
        }

        public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            BeginAccess();
            var r = base.GetEnumerator();
            EndAccess();
            
            return r;
        }


    }
}
