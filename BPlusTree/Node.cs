﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Storage;

namespace HybreDb.BPlusTree {
    
    /// <summary>
    /// Used to update a data item of a leafnode
    /// </summary>
    /// <param name="node">Node which should hold given key</param>
    /// <param name="value">Value bound to given key, null if value is not present</param>
    /// <returns>Whenever the node has been changed or not</returns>
    public delegate bool NodeUpdateHandler<TKey, TValue>(LeafNode<TKey, TValue> node, TKey k, TValue value)
        where TKey : IComparable, IByteSerializable
        where TValue : IByteSerializable;

    /// <summary>
    /// Node interface for all nodes in BPlusTree
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public interface INode<TKey, TValue> : IDisposable, IByteSerializable, IEnumerable<KeyValuePair<TKey, TValue>> 
        where TKey : IComparable, IByteSerializable
        where TValue : IByteSerializable 
    {

        /// <summary>
        /// Current items in node
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Max items in node, equal to Tree.BucketSize
        /// </summary>
        int Capacity { get; }

        
            /// <summary>
        /// First child node
        /// </summary>
        INode<TKey, TValue> First { get;  }

        /// <summary>
        /// Gets the first leaf with recursion
        /// </summary>
        LeafNode<TKey, TValue> FirstLeaf { get; } 
        
        /// <summary>
        /// Tree the node belongs to
        /// </summary>
        Tree<TKey, TValue> Tree { get; }

        /// <summary>
        /// Returns the Type of the node
        /// </summary>
        NodeTypes Type { get; }

        /// <summary>
        /// Highest key present in the node
        /// </summary>
        TKey HighestKey { get; }

        /// <summary>
        /// Lowest key present in the node
        /// </summary>
        TKey LowestKey { get; }

        /// <summary>
        /// Gets a data item from the tree, uses Select recursively
        /// </summary>
        /// <param name="k">Key to lookup</param>
        /// <returns>Data item bound to given key</returns>
        bool TryGet(TKey k, out TValue v);


        /// <summary>
        /// Returns the leaf responsable for given key
        /// </summary>
        LeafNode<TKey, TValue> GetLeaf(TKey k);

        /// <summary>
        /// Updates a given node
        /// </summary>
        /// <param name="k">Key value of the item to update</param>
        /// <param name="h">Handler which updates given item and node, returns true if node has been changed</param>
        /// <returns>returns true if node has been changed</returns>
        bool Update(TKey k, NodeUpdateHandler<TKey, TValue> h);

        /// <summary>
        /// Inserts given data with given key in the tree
        /// </summary>
        /// <param name="key">Key to insert with</param>
        /// <param name="data">Data to associate with given key</param>
        /// <returns>A newly created node when the bucket is full</returns>
        INode<TKey, TValue> Insert(TKey key, TValue data);

        /// <summary>
        /// Deletes a item with given key
        /// </summary>
        /// <param name="key">key to delete</param>
        /// <returns>The action the previous remove performed</returns>
        RemoveResult Remove(TKey key);
        
        /// <summary>
        /// Splits a node in two. Moves the upper half of the keys to a new node
        /// </summary>
        /// <returns>A new node with the upper halve of the keys</returns>
        INode<TKey, TValue> Split();

        /// <summary>
        /// Merges the two nodes. Moves the keys to the given node.
        /// </summary>
        /// <returns>Whenever the nodes could be merged. When false is returned, nothing was done.</returns>
        bool Merge(INode<TKey, TValue> n); 
        
        /// <summary>
        /// Tries to borrow keys from the left and right neighbours.
        /// Tries to move the upper keys from the left neighbour and lower keys from the right neighbour.
        /// </summary>
        /// <param name="l">Left neighbour with lower keys</param>
        /// <param name="r">Right neighbour with upper keys</param>
        /// <returns>Whenever enough items could be borrowed. When false is returned, nothing was done.</returns>
        bool Borrow(INode<TKey, TValue> l, INode<TKey, TValue> r);


        /// <summary>
        /// Starts an access, reads in the data and increments the access field
        /// </summary>
        /// <remarks>Begin- and EndAccess are implemented in the INode interface because methods like Split, Merge, Borrow need to use these</remarks>
        void BeginAccess();

        /// <summary>
        /// Ends an access, decrement the access field and updates the node in the LRU cache
        /// </summary>
        /// <param name="isChanged">Set when the data of the node (or one of it's children) has been changed</param>
        /// <remarks>Begin- and EndAccess are implemented in the INode interface because methods like Split, Merge, Borrow need to use these</remarks>
        void EndAccess(bool isChanged = false);
    }

    /// <summary>
    /// Degined to propengate which merge action was done
    /// </summary>
    public enum RemoveResult {
        None,
        Borrowed,
        Merged,
        Removed
    };


    public enum NodeTypes {
        Base,
        Leaf
    }
}
