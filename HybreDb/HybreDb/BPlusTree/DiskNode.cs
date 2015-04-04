using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace HybreDb.BPlusTree {
    public interface IDiskNode<T> : INode<T> {
        /// <summary>
        /// Offset of the node within the file.
        /// </summary>
        int FileOffset { get; }
        
        /// <summary>
        /// State of the node: OnDisk, Changed, Loaded
        /// </summary>
        NodeState State { get; }

        /// <summary>
        /// Write the node to disk, set the FileOffset
        /// </summary>
        void Write();

        /// <summary>
        /// Reads the node from disk based on the FileOffset
        /// </summary>
        void Read();
    }

    public enum NodeState {
        Loaded = 0,
        Changed,
        OnDisk
    }
}
