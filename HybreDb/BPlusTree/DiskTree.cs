using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.BPlusTree.Collections;
using HybreDb.Storage;

namespace HybreDb.BPlusTree {

    public class NodeReadEventArgs<TValue> : EventArgs {
        public TValue Data;
    }

    /// <summary>
    /// BTree extension which can be written to file
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class DiskTree<TKey, TValue> : Tree<TKey, TValue>, IByteSerializable
        where TKey : IComparable, IByteSerializable, new()
        where TValue : IByteSerializable, new() 
    {

        public int Reads = 0;
        public int Writes = 0;
        public int Freed = 0;

        public delegate void NodeDataRead(object sender, NodeReadEventArgs<TValue> e);

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

        /// <summary>
        /// Cache which contains the nodes
        /// </summary>
        public LRUCache<IDiskNode<TKey, TValue>> Cache { get; protected set; }

        /// <summary>
        /// Size of the cache, 
        /// </summary>
        public int CacheSize { get; protected set; }

        /// <summary>
        /// Reference to the root of the tree
        /// </summary>
        public IDiskNode<TKey, TValue> DiskRoot {
            get { return (IDiskNode<TKey, TValue>) Root; } 
        }

        public FileStream Stream { get; protected set; }

        /// <summary>
        /// Creates a new Diskbased B+ tree. When file already exists it reads in the data from the file.
        /// </summary>
        /// <param name="filename">Filename to use for read and writing</param>
        /// <param name="bucketSize">Size used in the buckets of the tree</param>
        /// <param name="cacheSize">Size of the cash in nodes</param>
        public DiskTree(string filename, int bucketSize=64, int cacheSize=64) : base(bucketSize) {
            Filename = filename;
            CacheSize = cacheSize;

            var exists = File.Exists(Filename);
            Stream = DbFile.Open(Filename);

            if (exists) Read();
            CreateCache(CacheSize);
        }

        protected void CreateCache(int size) {
            Cache = new LRUCache<IDiskNode<TKey, TValue>>(size);
            Cache.OnOutDated += (s, e) => e.Data.Write();
        }

        #region Creators overrides
        public override BaseNode<TKey, TValue> CreateBaseNode() {
            return new DiskBaseNode<TKey, TValue>(this);
        }

        public virtual BaseNode<TKey, TValue> CreateBaseNode(long pos) {
            return new DiskBaseNode<TKey, TValue>(this, pos);
        }

        public override LeafNode<TKey, TValue> CreateLeafNode() {
            return new DiskLeafNode<TKey, TValue>(this);
        }

        public virtual LeafNode<TKey, TValue> CreateLeafNode(long pos) {
            return new DiskLeafNode<TKey, TValue>(this, pos);
        }
        #endregion

        /// <summary>
        /// Writes the tree to file
        /// </summary>
        public void Write() {
            Stream.Seek(0, SeekOrigin.End);
            var bin = new BinaryWriter(Stream);
            ((IDiskNode<TKey, TValue>)Root).Write(bin);

            Serialize(bin);

            Stream.Flush();
        }


        /// <summary>
        /// Reads in the tree
        /// </summary>
        public void Read() {
            var rdr = new BinaryReader(Stream);
            Stream.Seek(-8, SeekOrigin.End);
            Stream.Position = rdr.ReadInt64();
            Deserialize(rdr);
        }

        public void Revert() {
            Root.Dispose();
            Read();
        }

        #region Serialisation
        public void Serialize(BinaryWriter wrtr) {
            PreviousRevision = FileOffset;
            FileOffset = Stream.Position;

            wrtr.Write(BucketSize);
            wrtr.Write(CacheSize);

            wrtr.Write(Revision++);
            wrtr.Write(PreviousRevision);
            

            DiskRoot.Serialize(wrtr);

            wrtr.Write(FileOffset);
        }


        public void Deserialize(BinaryReader rdr) {
            FileOffset = rdr.BaseStream.Position;


            BucketSize = rdr.ReadInt32();
            CacheSize = rdr.ReadInt32();

            Revision = rdr.ReadUInt32();
            PreviousRevision = rdr.ReadInt64();

            Root = DiskNode<TKey, TValue>.Create(this, rdr);
        }
        #endregion


        protected override void Dispose(bool disposing = false) {
            Stream.Dispose();
            Root.Dispose();
        }

        /// <summary>
        /// Helper function to invoke the event
        /// </summary>
        /// <param name="v"></param>
        internal void DataRead(TValue v) {
            if (OnDataRead != null) OnDataRead(this, new NodeReadEventArgs<TValue> {Data = v});
        }

        public void Drop() {
            Dispose();
            File.Delete(Filename);
        }
    }
}
