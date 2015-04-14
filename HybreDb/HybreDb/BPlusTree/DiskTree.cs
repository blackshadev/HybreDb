using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.BPlusTree.Collections;
using HybreDb.Storage;

namespace HybreDb.BPlusTree {

    /// <summary>
    /// BTree extension which can be written to file
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class DiskTree<TKey, TValue> : Tree<TKey, TValue>, ITreeSerializable, IEnumerable<TValue>
        where TKey : IComparable, ITreeSerializable, new()
        where TValue : ITreeSerializable, new() 
    {

        public delegate void NodeDataRead(TValue v);

        /// <summary>
        /// Called before a data item is serialized
        /// </summary>
        public event NodeDataRead OnDataRead;

        /// <summary>
        /// FilName of the file to which all data of this tree is written
        /// </summary>
        public string Filename { get; private set; }
        
        /// <summary>
        /// Revision number of the tree
        /// </summary>
        public uint Revision { get; private set; }
        
        /// <summary>
        /// File offset to the previous revision
        /// </summary>
        public long PreviousRevision { get; private set; }

        /// <summary>
        /// File offset to the current revision
        /// </summary>
        public long FileOffset { get; private set; }

        public LRUCache<IDiskNode<TKey, TValue>> Cache;

        public IDiskNode<TKey, TValue> DiskRoot {
            get { return (IDiskNode<TKey, TValue>) Root; } 
        }

        public FileStream Stream { get; protected set; }

        public DiskTree(string filename, int bucketSize, int cacheSize) : base(bucketSize) {
            Filename = filename;

            Stream = new FileStream(Filename, FileMode.OpenOrCreate);
            CreateCache(cacheSize);
        }

        public DiskTree(string filename, int bucketSize, int cacheSize, KeyValuePair<TKey, TValue>[] dat) : base(bucketSize, dat) {
            Filename = filename;

            Stream = new FileStream(Filename, FileMode.OpenOrCreate);
            CreateCache(cacheSize);
        }

        protected void CreateCache(int s) {
            Cache = new LRUCache<IDiskNode<TKey, TValue>>(s);
            Cache.OnOutDated += node => { node.Write(); };
        }

        #region Creators overrides
        public override BaseNode<TKey, TValue> CreateBaseNode() {
            return new DiskBaseNode<TKey, TValue>(this);
        }

        public virtual BaseNode<TKey, TValue> CreateBaseNode(long pos) {
            return new DiskBaseNode<TKey, TValue>(this, pos);
        }

        public override LeafNode<TKey, TValue> CreateLeafNode(LeafNode<TKey, TValue> prev = null, LeafNode<TKey, TValue> next = null) {
            return new DiskLeafNode<TKey, TValue>(this) { Prev = prev, Next = next };
        }

        public virtual LeafNode<TKey, TValue> CreateLeafNode(long pos) {
            return new DiskLeafNode<TKey, TValue>(this, pos);
        }
        #endregion

        public void Write() {
            Stream.Seek(0, SeekOrigin.End);
            var bin = new BinaryWriter(Stream);
            ((IDiskNode<TKey, TValue>)Root).Write(bin);

            Serialize(bin);
        }

        public void Read() {
            var rdr = new BinaryReader(Stream);
            Deserialize(rdr);
        }

        public void Serialize(BinaryWriter wrtr) {
            PreviousRevision = FileOffset;
            FileOffset = Stream.Position;

            wrtr.Write(Revision++);
            wrtr.Write(PreviousRevision);

            DiskRoot.Serialize(wrtr);

            wrtr.Write(FileOffset);
        }

        public IEnumerator<TValue> GetEnumerator() {
            
            return Root.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Deserialize(BinaryReader rdr) {
            rdr.BaseStream.Seek(-8, SeekOrigin.End);

            FileOffset = rdr.ReadInt64();
            rdr.BaseStream.Seek(FileOffset, SeekOrigin.Begin);

            Revision = rdr.ReadUInt32();
            PreviousRevision = rdr.ReadInt64();

            Root = DiskNode<TKey, TValue>.Create(this, rdr);
        }

        public void Dispose() {
            Stream.Dispose();
            Root.Dispose();
        }

        /// <summary>
        /// Helper function to invoke the event
        /// </summary>
        /// <param name="v"></param>
        internal void DataRead(TValue v) {
            if (OnDataRead != null) OnDataRead(v);
        }
    }
}
