using System;
using System.Linq;
using System.Collections.Generic;


namespace PQ._Experimental.Physics
{
    /*
    Low overhead container for mapping enum fields to values
    
    - Unlike the enum Flags attribute, the first/last value is NOT treated as a none and all field (and thus not needed in its declaration)
    */
    public sealed class EnumMap<TKey, TValue>
        where TKey : struct, Enum
    {
        private readonly EnumSet<TKey> _keys;
        private readonly TValue[]      _values;

        public Type KeyType   => typeof(TKey);
        public Type ValueType => typeof(TValue);
        public int  Count     => _keys.Count;
        
        public TValue this[TKey key]
        {
            get
            {
                if (!_keys.LookupIndex(key, out int index) || !_keys.Contains(key))
                {
                    throw new ArgumentException($"Failed to lookup value - key {key} is not found in {_keys}");
                }
                return _values[index];
            }
            set
            {
                if (!_keys.LookupIndex(key, out int index) || !_keys.Contains(key))
                {
                    throw new ArgumentException($"Failed to lookup value - key {key} is not found in {_keys}");
                }
                _values[index] = value;
            }
        }

        public IReadOnlyList<TKey>               EnumFields => _keys.EnumFields;
        public IReadOnlyList<TKey>               Keys       => ExtractKeys(_keys).ToArray();
        public IReadOnlyList<TValue>             Values     => ExtractValues(_keys, _values).ToArray();
        public IReadOnlyList<(TKey k, TValue v)> Entries    => ExtractEntries(_keys, _values).ToArray();

        public override string ToString() =>
            $"{typeof(EnumMap<TKey, TValue>).Name}<{typeof(TKey)},{typeof(TValue)}>" +
            $"{{ {string.Join(", ", ExtractValues(_keys, _values))} }}";


        public EnumMap()
        {
            _keys   = new EnumSet<TKey>();
            _values = new TValue[_keys.Size];
        }

        public EnumMap(in (TKey key, TValue value)[] entries) : this()
        {
            if (entries == null)
            {
                throw new ArgumentNullException($"Failed to add entries - cannot be null");
            }

            foreach ((TKey key, TValue value) in entries)
            {
                Add(key, value);
            }
        }

        public bool Contains(TKey key) => _keys.Contains(key);
        public bool Contains(in EnumSet<TKey> other) => _keys.Contains(other);

        public void Add(TKey key, TValue value)
        {
            if (!_keys.LookupIndex(key, out int index))
            {
                throw new ArgumentException($"Failed to add entry - key {key} is not a defined field of {typeof(TKey)}");
            }
            if (!_keys.TryAdd(key))
            {
                throw new ArgumentException($"Failed to add entry - values cannot be overriden, key {key} already found in {_keys}");
            }

            _values[index] = value;
        }

        public void Remove(TKey key)
        {
            if (!_keys.LookupIndex(key, out int index) || !_keys.TryRemove(key))
            {
                throw new ArgumentException($"Failed to remove entry - key {key} not found in {_keys}");
            }

            _values[index] = default;
        }


        public bool TryGetValue(TKey key, out TValue value)
        {
            if (!_keys.LookupIndex(key, out int index) || !_keys.Contains(key))
            {
                value = default;
                return false;
            }
            value = _values[index];
            return true;
        }

        public bool TryAdd(TKey key, TValue value)
        {
            if (!_keys.LookupIndex(key, out int index) || !_keys.TryAdd(key))
            {
                return false;
            }
            _values[index] = value;
            return true;
        }

        public bool TryRemove(TKey key)
        {
            if (!_keys.LookupIndex(key, out int index) || !_keys.TryRemove(key))
            {
                return false;
            }
            _values[index] = default;
            return true;
        }


        private static IEnumerable<TKey> ExtractKeys(EnumSet<TKey> keys)
        {
            for (int i = 0; i < keys.Size; i++)
            {
                if (keys.LookupKey(i, out TKey key) && keys.Contains(key))
                {
                    yield return key;
                }
            }
        }

        private static IEnumerable<TValue> ExtractValues(EnumSet<TKey> keys, TValue[] values)
        {
            for (int i = 0; i < keys.Size; i++)
            {
                if (keys.LookupKey(i, out TKey key) && keys.Contains(key))
                {
                    yield return values[i];
                }
            }
        }

        private static IEnumerable<(TKey key, TValue value)> ExtractEntries(EnumSet<TKey> keys, TValue[] values)
        {
            for (int i = 0; i < keys.Size; i++)
            {
                if (keys.LookupKey(i, out TKey key) && keys.Contains(key))
                {
                    yield return (key, values[i]);
                }
            }
        }
    }
}
