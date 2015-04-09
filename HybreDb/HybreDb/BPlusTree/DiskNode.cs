using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Storage;

namespace HybreDb.BPlusTree {
    public interface IDiskNode<TKey, TValue> : INode<TKey, TValue>
        where TKey : IComparable, ITreeSerializable, new()
        where TValue : ITreeSerializable, new() 
    {
        /// <summary>
        /// Offset of the node within the file.
        /// </summary>
        long FileOffset { get; }
        
        /// <summary>
        /// State of the node: OnDisk, Changed, Loaded
        /// </summary>
        NodeState State { get; }

        DiskTree<TKey, TValue> DiskTree { get; } 

        /// <summary>
        /// Frees the data of the node.
        /// </summary>
        void Free();

        /// <summary>
        /// Write the node to disk, set the FileOffset
        /// </summary>
        void Write(BinaryWriter wrtr);

        /// <summary>
        /// Write the node to disk, set the FileOffset, uses Write(BinaryWriter)
        /// </summary>
        void Write();

        /// <summary>
        /// Reads the node from disk based on the FileOffset
        /// </summary>
        //void Read(Stream Stream);
    }

    public enum NodeState {
        Loaded = 0,
        OnDisk,
        Changed
    }

    public static class DiskNode<TKey, TValue> 
        where TKey : IComparable, ITreeSerializable, new()
        where TValue : ITreeSerializable, new()
    {
        public static INode<TKey, TValue> Create(DiskTree<TKey, TValue> tree, BinaryReader rdr) {
            var t = (NodeTypes)rdr.ReadByte();
            var offs = rdr.ReadInt64();
            switch (t) {
                case NodeTypes.Leaf:
                    return tree.CreateLeafNode(offs);
                case NodeTypes.Base:
                    return tree.CreateBaseNode(offs);
            }

            return null;
        }
    }
}
