using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Storage;

namespace HybreDb.BPlusTree {

    public class SortedBuckets<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IDisposable, ITreeSerializable
        where TKey : IComparable
        where TValue : ITreeSerializable
    {

        protected TKey[] _keys;
        protected TValue[] _values;

        public int Capacity;
        public int Count = 0;

        public IEnumerable<TKey> Keys {
            get {
                for (var i = 0; i < Count; i++)
                    yield return _keys[i];
            }
        }

        public IEnumerable<TValue> Values {
            get {
                for (var i = 0; i < Count; i++)
                    yield return _values[i];
            }
        }

        public SortedBuckets(int size) {
            Capacity = size;
            _keys = new TKey[size];
            _values = new TValue[size];

            var t = _keys.GetType();
            
        }


        public void Set(int idx, TKey key, TValue value) {
            _keys[idx] = key;
            _values[idx] = value;
        }

        public void Add(TKey key, TValue value) {
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

        public void Remove(TKey key) {
            int idx = Index(key);
            if(idx < 0) throw new ArgumentException("Key does not exist");

            RemoveIndex(idx);
        }

        public void RemoveIndex(int idx) {
            Array.Copy(_keys, idx + 1, _keys, idx, Count - idx);
            Array.Copy(_values, idx + 1, _values, idx, Count - idx);

            Count--;

            _keys[Count] = default(TKey);
            _values[Count] = default(TValue);
        }

        public TValue TryGetValue(TKey key) {
            TValue val = default(TValue);
            int idx = Index(key);
            if (idx < 0) return val;
            return _values[idx];
        }

        public TValue Get(TKey key) {
            int idx = Index(key);
            if (idx < 0) throw new KeyNotFoundException();
            return _values[idx];
        }

        public TValue ValueAt(int k) {
            return _values[k];
        }

        public TKey KeyAt(int k) {
            return _keys[k];
        }

        public int Index(TKey k) {
            return Array.BinarySearch(_keys, 0, Count, k);
        }

        /// <returns>The index of the key which is the given key or at first bigger than the key</returns>
        public int NearestIndex(TKey k) {
            int idx = Index(k);
            return idx > -1 ? idx : ~idx < Count ? ~idx : Count - 1;
        }

        /// <summary>
        /// Takes items from the beginning of the sorted buckets to the end
        /// </summary>
        /// <param name="end"></param>
        /// <returns></returns>
        public SortedBuckets<TKey, TValue> SliceBegin(int end) {
            var d = new SortedBuckets<TKey, TValue>(Capacity);

            Array.Copy(_keys, 0, d._keys, 0, end);
            Array.Copy(_values, 0, d._values, 0, end);

            Array.Copy(_keys, end, _keys, 0, Count - end);
            Array.Copy(_values, end, _values, 0, Count - end);

            for (var i = 0; i < end; i++) {
                _keys[Count - end + i] = default(TKey);
                _values[Count - end + i] = default(TValue);
            }


            d.Count = end;
            Count -= d.Count;

            return d;
        }

        /// <summary>
        ///  Takes items from the end of the sorted buckets beginning by start
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public SortedBuckets<TKey, TValue> SliceEnd(int start) {
            var d = new SortedBuckets<TKey, TValue>(Capacity);
            Array.Copy(_keys, start, d._keys, 0, Count - start);
            Array.Copy(_values, start, d._values, 0, Count - start);


            for (var i = 0; i < Count - start; i++) {
                _keys[start + i] = default(TKey);
                _values[start + i] = default(TValue);
            }

            d.Count = Count - start;
            Count -= d.Count;

            return d;
        }

        public void AddEnd(SortedBuckets<TKey, TValue> s) {
            if (Count + s.Count > Capacity)
                throw new ArgumentException("Resulting bucket is to big");

            Array.Copy(s._keys, 0, _keys, Count, s.Count);
            Array.Copy(s._values, 0, _values, Count, s.Count);

            Count += s.Count;
        }

        public void AddBegin(SortedBuckets<TKey, TValue> s) {
            if (Count + s.Count > Capacity)
                throw new ArgumentException("Resulting bucket is to big");

            // Make place
            Array.Copy(_keys, 0, _keys, s.Count, Count);
            Array.Copy(_values, 0, _values, s.Count, Count);

            Array.Copy(s._keys, 0, _keys, 0, s.Count);
            Array.Copy(s._values, 0, _values, 0, s.Count);

            Count += s.Count;

        }

        public void LoadSorted(IEnumerable<KeyValuePair<TKey, TValue>> data) {
            int iX = 0;
            foreach (var v in data) {
                _keys[iX] = v.Key;
                _values[iX] = v.Value;

                iX++;
            }

            Count = iX;
        }


        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            for (var i = 0; i < Count; i++)
                yield return new KeyValuePair<TKey, TValue>(_keys[i], _values[i]);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Dispose() {
            _keys = null;
            _values = null;
            Count = 0;
            Capacity = 0;
        }

        public void Serialize(BinaryWriter wrtr) {
            wrtr.Write(Count);

            for (int i = 0; i < Count; i++) {
               // _keys[i].Serialize(wrtr);
                _values[i].Serialize(wrtr);
            }

        }

        public void Deserialize(BinaryReader rdr) {
            
        }
    }
}
