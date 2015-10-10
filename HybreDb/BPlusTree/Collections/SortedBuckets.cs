using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Data;
using HybreDb.Storage;

namespace HybreDb.BPlusTree.Collections {
    public class SortedBuckets<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IDisposable, IByteSerializable
        where TKey : IComparable, IByteSerializable
        where TValue : IByteSerializable {
        public int Capacity;
        public int Count = 0;
        protected TKey[] _keys;
        protected TValue[] _values;

        public SortedBuckets(int size) {
            Capacity = size;
            _keys = new TKey[size];
            _values = new TValue[size];

            Type t = _keys.GetType();
        }

        public SortedBuckets(BinaryReader rdr, Func<BinaryReader, TKey> kConstructor,
            Func<BinaryReader, TValue> vConstructor) {
            Capacity = rdr.ReadInt32();
            Count = rdr.ReadInt32();

            _keys = new TKey[Capacity];
            _values = new TValue[Capacity];


            for (int i = 0; i < Count; i++) {
                _keys[i] = kConstructor(rdr);
                _values[i] = vConstructor(rdr);
            }
        }

        public IEnumerable<TKey> Keys {
            get {
                for (int i = 0; i < Count; i++)
                    yield return _keys[i];
            }
        }

        public IEnumerable<TValue> Values {
            get {
                for (int i = 0; i < Count; i++)
                    yield return _values[i];
            }
        }

        public void Serialize(BinaryWriter wrtr) {
            wrtr.Write(Capacity);
            wrtr.Write(Count);

            for (int i = 0; i < Count; i++) {
                _keys[i].Serialize(wrtr);
                _values[i].Serialize(wrtr);
            }
        }

        public void Deserialize(BinaryReader rdr) {
            throw new NotImplementedException();
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            for (int i = 0; i < Count; i++)
                yield return new KeyValuePair<TKey, TValue>(_keys[i], _values[i]);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }


        public void Set(int idx, TKey key, TValue value) {
            _keys[idx] = key;
            _values[idx] = value;
        }

        public void Add(TKey key, TValue value) {
            if (Count == Capacity)
                throw new InvalidOperationException("Dictionary is full");

            int idx = Index(key);
            if (idx > -1) throw new InvalidOperationException("Key " + key.ToString() + " already exists");
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
            if (idx < 0) throw new KeyNotFoundException();

            RemoveIndex(idx);
        }

        public void RemoveIndex(int idx) {
            Array.Copy(_keys, idx + 1, _keys, idx, Count - idx);
            Array.Copy(_values, idx + 1, _values, idx, Count - idx);

            Count--;

            _keys[Count] = default(TKey);
            _values[Count] = default(TValue);
        }

        public bool TryGetValue(TKey key, out TValue result) {
            int idx = Index(key);
            bool found = idx > -1 && _keys[idx] != null && _keys[idx].CompareTo(key) == 0;
            result = found ? _values[idx] : default(TValue);
            return found;
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
        
        //public int Index(TKey k) {
        //    //return Array.BinarySearch(_keys, 0, Count, k);
        //    int i = 0;
        //    for (; i < Count; i++) {
        //        if (k.CompareTo(_keys[i]) < 1)
        //            return i;
        //    }
        //    return i;
        //}

        public int Index(TKey k) {
            int i = 0;
            for (; i < Count; i++) {
                int c = k.CompareTo(_keys[i]);
                if (c < 1)
                    return c == 0 ? i: ~i;
            }

            return ~i;
        }

        /// <returns>The index of the key which is the given key or at first bigger than the key</returns>
        public int NearestIndex(TKey k) {
            int idx = Index(k);
            idx = idx < 0 ? ~idx : idx;
            return idx < Count ? idx : Count - 1;
        }

        /// <summary>
        ///     Takes items from the beginning of the sorted buckets to the end
        /// </summary>
        /// <param name="end"></param>
        /// <returns></returns>
        public SortedBuckets<TKey, TValue> SliceBegin(int end) {
            var d = new SortedBuckets<TKey, TValue>(Capacity);

            Array.Copy(_keys, 0, d._keys, 0, end);
            Array.Copy(_values, 0, d._values, 0, end);

            Array.Copy(_keys, end, _keys, 0, Count - end);
            Array.Copy(_values, end, _values, 0, Count - end);

            for (int i = 0; i < end; i++) {
                _keys[Count - end + i] = default(TKey);
                _values[Count - end + i] = default(TValue);
            }


            d.Count = end;
            Count -= d.Count;

            return d;
        }

        /// <summary>
        ///     Takes items from the end of the sorted buckets beginning by start
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public SortedBuckets<TKey, TValue> SliceEnd(int start) {
            var d = new SortedBuckets<TKey, TValue>(Capacity);
            Array.Copy(_keys, start, d._keys, 0, Count - start);
            Array.Copy(_values, start, d._values, 0, Count - start);


            for (int i = 0; i < Count - start; i++) {
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


        protected virtual void Dispose(bool disposing) {
            _keys = null;
            _values = null;
            Count = 0;
            Capacity = 0;
        }
    }
}