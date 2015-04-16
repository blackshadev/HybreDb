using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HybreDb.BPlusTree;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Storage;

namespace HybreDb.Tables {

    public interface IndexTree : IDisposable {
        void Add(object d, Number n);

        void Commit();
        void Read();

        Numbers Find(object k);
    }

    /// <summary>
    /// Wrapper which holds a DiskTree for a type
    /// </summary>
    public class IndexTree<TKey> : IndexTree
        where TKey : IComparable, ITreeSerializable, new()
    {

        public string Name;
        public DiskTree<TKey, Numbers> Tree;


        public IndexTree(string name) {
            Name = name;
            Tree = new DiskTree<TKey, Numbers>(name + ".idx.bin", Table.BucketSize, Table.CacheSize);
        }
   

        public Numbers Find(object d) {
            if(d.GetType() != typeof(TKey))
                throw new ArgumentException("Data with wrong type given. Expected `" + typeof(TKey).Name + "` got `" + d.GetType().Name + "`");
            return Find((TKey) d);
        }

        public Numbers Find(TKey d) {
            return Tree.Get(d);
        }

        public static bool UpdHandler<TKey, TValue>(LeafNode<TKey, TValue> n, TKey k, TValue v) 
            where TKey: IComparable, ITreeSerializable
            where TValue : ITreeSerializable
        {
            if (v == null && n.Count < n.Capacity - 1) {
                n.Buckets.Add(k, v);
                return true;
            } 
            if (v != null) {

                return true;
            }

            return false;
        }

        public void Add(TKey d, Number num) {
            var nums = new Numbers();
            nums.Add(num);

            var c = Tree.Update(d, (n, k, v) => {
                if (v == null && n.Count < n.Capacity - 1) {
                    n.Buckets.Add(k, nums);
                    return true;
                }
                if (v != null) {
                    v.Add(num);
                    return true;
                }

                return false;
            });
            if (c) return;

            // Update failed, so we need to insert manually
            
            Tree.Insert(d, nums);
        }

        public void Add(object d, Number n) {
            Add((TKey)d, n);
        }

        public void Commit() {
            Tree.Write();
        }

        public void Read() {
            Tree.Read();
        }

        public void Dispose() {
            Tree.Dispose();
        }

    }
}
