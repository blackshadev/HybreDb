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
    public interface IDiskNode<T> : INode<T>
        where T : ITreeSerializable {
        /// <summary>
        /// Offset of the node within the file.
        /// </summary>
        long FileOffset { get; }
        
        /// <summary>
        /// State of the node: OnDisk, Changed, Loaded
        /// </summary>
        NodeState State { get; }

        /// <summary>
        /// Frees the data of the node.
        /// </summary>
        void Free();

        /// <summary>
        /// Write the node to disk, set the FileOffset
        /// </summary>
        void Write(BinaryWriter wrtr);

        /// <summary>
        /// Reads the node from disk based on the FileOffset
        /// </summary>
        //void Read(Stream strm);
    }

    public enum NodeState {
        Loaded = 0,
        OnDisk,
        Changed
    }

    public static class DiskNode {
        public enum Types {
            BaseNode,
            LeafNode
        }
    }
}
