using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybreDb.BPlusTree {
    /// <summary>
    /// Node interface for all nodes in BPlusTree
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface INode<T> : IDisposable {
        
        /// <summary>
        /// Current items in node
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Max items in node
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// First child node
        /// </summary>
        INode<T> First { get;  }

        Tree<T> Tree { get; } 

        /// <summary>
        /// Highest key present in the node
        /// </summary>
        int HighestKey { get; }
        /// <summary>
        /// Lowest key present in the node
        /// </summary>
        int LowestKey { get; }

        /// <summary>
        /// Selects a node in within the current node based on the given key
        /// </summary>
        /// <param name="key">Key to find the node of</param>
        /// <returns>Returns the node</returns>
        INode<T> Select(int k);

        /// <summary>
        /// Gets a data item from the tree
        /// </summary>
        /// <param name="k">Key to lookup</param>
        /// <returns>Data item bound to given key</returns>
        T Get(int k);

        /// <summary>
        /// Inserts given data with given key in the tree
        /// </summary>
        /// <param name="key">Key to insert with</param>
        /// <param name="data">Data to associate with given key</param>
        /// <returns>A newly created node when the bucket is full</returns>
        INode<T> Insert(int key, T data);

        /// <summary>
        /// Deletes a item with given key
        /// </summary>
        /// <param name="key">key to delete</param>
        /// <returns>null if nothing special to do, else the node which needs to merge</returns>
        RemoveResult Remove(int key);
        INode<T> Split();

        bool Merge(INode<T> n); 
        

        bool Borrow(INode<T> l, INode<T> r);
    }

    /// <summary>
    /// Degined to propengate which merge action to use
    /// </summary>
    public enum RemoveResult {
        None,
        Borrowed,
        Merged,
        Removed
    };

}
