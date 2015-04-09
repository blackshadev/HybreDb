﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Storage;

namespace HybreDb.BPlusTree {

    /// <summary>
    /// LeafNode wish can be read and written to disk
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TKey">Value type</typeparam>
    public class DiskLeafNode<TKey, TValue> : LeafNode<TKey, TValue>, IDiskNode<TKey, TValue>
        where TKey : ITreeSerializable, IComparable, new()
        where TValue : ITreeSerializable, new() 
    {
        public static int Total = 0;
        
        public SortedBuckets<TKey, TValue> Buckets {
            get {
                Read();
                return _buckets;
            }
        }

        public long FileOffset { get; private set; }
        public NodeState State { get; private set; }

        public INode<TKey, TValue> First { get { Read(); return this; } } 

        public DiskTree<TKey, TValue> DiskTree { get; private set; }

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
        public override TValue Get(TKey key) {
            Read();
            return base.Get(key);
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

      
        #endregion

        /// <summary>
        /// Free all resources hold bu this node
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

        /// <summary>
        /// Writes the data from the node to the given stream.
        /// </summary>
        public void Write(BinaryWriter wrtr) {
            if (State != NodeState.Changed) return;

            var next = ((DiskLeafNode<TKey, TValue>)Next);
            var prev = ((DiskLeafNode<TKey, TValue>)Prev);


            if (next != null)
                next.Write(wrtr);


            FileOffset = wrtr.BaseStream.Position;
            //if (Next != null)
            //    wrtr.Write(next.FileOffset);
            //else
            //    wrtr.Write(-1L);
            Buckets.Serialize(wrtr);
            

            Free();

            //if(prev != null) prev.UpdateNextFileOffset(wrtr);

        }

        /// <summary>
        /// Updates the Neighbour reference on disk.
        /// </summary>
        public void UpdateNextFileOffset(BinaryWriter wrtr) {
            if (State != NodeState.OnDisk) return;

            var offs = wrtr.BaseStream.Position;

            wrtr.BaseStream.Position = FileOffset;

            wrtr.Write( ((DiskLeafNode<TKey, TValue>)Next).FileOffset);

            wrtr.BaseStream.Position = offs;
        }

        /// <summary>
        /// Reads the data into the node with given reader
        /// </summary>
        public void Read() {
            if (State != NodeState.OnDisk) return;

            var rdr = new BinaryReader(DiskTree.Stream);
            rdr.BaseStream.Seek(FileOffset, SeekOrigin.Begin);

            //var offs = rdr.ReadInt64();
            _buckets = new SortedBuckets<TKey, TValue>(rdr,
                _rdr => { var v = new TKey(); v.Deserialize(_rdr); return v; },
                _rdr => { var v = new TValue(); v.Deserialize(_rdr); return v; }
            );
            //if (offs > -1) {
            //    Next = DiskTree.CreateLeafNode(offs);
            //    Next.Prev = this;
            //}

            State = NodeState.Loaded;
        }


        /// <summary>
        /// Serializes the node, the node is represented by it's node type and the offset of it's data within the file
        /// </summary>
        public void Serialize(BinaryWriter wrtr) {
            wrtr.Write((byte)Type);
            wrtr.Write(FileOffset);
            
            
        }

        /// <summary>
        /// Not implemeted because these nodes are created via DiskTree.Create
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
