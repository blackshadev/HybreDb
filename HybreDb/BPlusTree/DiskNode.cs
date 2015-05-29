﻿using System;
using System.IO;
using HybreDb.Storage;

namespace HybreDb.BPlusTree {
    public interface IDiskNode<TKey, TValue> : INode<TKey, TValue>
        where TKey : IComparable, IByteSerializable, new()
        where TValue : IByteSerializable, new() {
        bool IsBusy { get; }

        /// <summary>
        ///     Offset of the node within the file.
        /// </summary>
        long FileOffset { get; }

        /// <summary>
        ///     State of the node: OnDisk, Changed, Loaded
        /// </summary>
        NodeState State { get; }

        DiskTree<TKey, TValue> DiskTree { get; }

        /// <summary>
        ///     Frees the data of the node.
        /// </summary>
        void Free();

        /// <summary>
        ///     Write the node to disk, set the FileOffset
        /// </summary>
        void Write(BinaryWriter wrtr);

        /// <summary>
        ///     Writes the node to disk, set the FileOffset, uses Write(BinaryWriter)
        /// </summary>
        void Write();

        /// <summary>
        ///     Read the node in from disk, uses the FileOffset
        /// </summary>
        void Read();
    }

    public enum NodeState {
        Loaded = 0,
        OnDisk,
        Changed
    }

    public static class DiskNode<TKey, TValue>
        where TKey : IComparable, IByteSerializable, new()
        where TValue : IByteSerializable, new() {
        public static INode<TKey, TValue> Create(DiskTree<TKey, TValue> tree, BinaryReader rdr) {
            var t = (NodeTypes) rdr.ReadByte();
            long offs = rdr.ReadInt64();
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