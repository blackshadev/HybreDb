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
    public class DiskTree<TKey, TValue> : Tree<TKey, TValue>, ITreeSerializable
        where TKey : IComparable, ITreeSerializable, new()
        where TValue : ITreeSerializable, new() 
    {
        public int OpenNodes = 0;
        public int BusyNodes = 0;

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

        /// <summary>
        /// Creates the tree, bulk inserts the data and writes the data.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="bucketSize"></param>
        /// <param name="cacheSize"></param>
        /// <param name="dat"></param>
        public DiskTree(string filename, int bucketSize, int cacheSize, KeyValuePair<TKey, TValue>[] dat) : base(bucketSize, dat) {
            Filename = filename;

            Stream = DbFile.Open(filename);
            CreateCache(cacheSize);
        
            Write();
        }

        protected void CreateCache(int s) {
            Cache = new LRUCache<IDiskNode<TKey, TValue>>(s);
            Cache.OnOutDated += node => node.Write();
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
