﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybreDb.BPlusTree {

    public class SortedBuckets<K, V> : IEnumerable<KeyValuePair<K,V>> {

        private K[] _keys;
        private V[] _values;

        public int Capacity;
        public int Count = 0;

        public IEnumerable<K> Keys {
            get {
                for (var i = 0; i < Count; i++)
                    yield return _keys[i];
            }
        }

        public IEnumerable<V> Values {
            get {
                for (var i = 0; i < Count; i++)
                    yield return _values[i];
            }
        }

        public SortedBuckets(int size) {
            Capacity = size;
            _keys = new K[size];
            _values = new V[size];
        }


        public void Set(int idx, K key, V value) {
            _keys[idx] = key;
            _values[idx] = value;
        }

        public void Add(K key, V value) {
            if (Count == Capacity)
                throw new ArgumentException("Dictionary is full");

            int idx = Index(key);
            if (idx > -1) throw new ArgumentException("Key with the same key already exists");
            idx = ~idx;



            if (idx < Count) {
                Array.Copy(_keys, idx, _keys, idx + 1, Count - idx);
                Array.Copy(_values, idx, _values, idx + 1, Count - idx);
            }


            _keys[idx] = key;
            _values[idx] = value;

            Count++;
        }

        public void Remove(K key) {
            int idx = Index(key);
            if(idx < 0) throw new ArgumentException("Key does not exist");

            Array.Copy(_keys, idx + 1, _keys, idx, Count - idx);
            Array.Copy(_values, idx + 1, _values, idx, Count - idx);
            
            Count--;

            _keys[Count] = default(K);
            _values[Count] = default(V);
        }

        public V TryGetValue(K key) {
            V val = default(V);
            int idx = Index(key);
            if (idx < 0) return val;
            return _values[idx];
        }

        public V Get(K key) {
            int idx = Index(key);
            if (idx < 0) throw new KeyNotFoundException();
            return _values[idx];
        }

        public V ValueAt(int k) {
            return _values[k];
        }

        public K KeyAt(int k) {
            return _keys[k];
        }

        public int Index(K k) {
            return Array.BinarySearch(_keys, 0, Count, k);
        }

        /// <returns>The index of the key which is the given key or at first bigger than the key</returns>
        public int NearestIndex(K k) {
            int idx = Index(k);
            return idx > -1 ? idx : ~idx < Count ? ~idx : Count - 1;
        }

        public SortedBuckets<K, V> Slice(int start) {
            var d = new SortedBuckets<K, V>(Capacity);
            Array.Copy(_keys, start, d._keys, 0, Count - start);
            Array.Copy(_values, start, d._values, 0, Count - start);


            for (var i = 0; i < Count - start; i++) {
                _keys[start + i] = default(K);
                _values[start + i] = default(V);
            }

            d.Count = Count - start;
            Count -= d.Count;

            return d;
        }


        public IEnumerator<KeyValuePair<K, V>> GetEnumerator() {
            for (var i = 0; i < Count; i++)
                yield return new KeyValuePair<K, V>(_keys[i], _values[i]);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
