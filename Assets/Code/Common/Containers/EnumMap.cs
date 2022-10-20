using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;


namespace PQ.Common.Containers
{
    /*
    Low overhead container for mapping enum fields to values.
    
    Overview
    - Essentially a layer on top of plain enums that allow constant time array-like lookups but with the convenience and
      type safety of a dictionary-like API, without any need for custom enum flags or values (error prone and fragile to change!),
      or the overhead of using a hashmap and constantly validating enums

    Properties
    - plain enums only      : all enum fields default to unique integers from 0 to size-1
    - intrinsically ordered : keys and corresponding map values are stored in the order defined by the enum
    - low memory footprint  : garbage free type conversions, up-front allocation, and one-time only enum validation/caching
    - scalable complexity   : constant time contains/get/add/remove, linear memory usage

    Notes
    - C# collection interfaces intentionally avoided as this is a very specialized container, and we want to avoid extra boxing/virtual calls
    - while the 64 max enum size restriction _could_ be lifted, though possibly a bad ROI as we shouldn't have such huge enums anyways
    - unlike the enum Flags attribute, the first/last value is NOT treated as a none and all field (and thus not needed in its declaration)
    */
    public sealed class EnumMap<TKey, TValue>
        where TKey : struct, Enum
    {
        private readonly EnumSet<TKey> _keys;
        private readonly TValue[]      _values;

        public Type KeyType   => typeof(TKey);
        public Type ValueType => typeof(TValue);
        public int  Count     => _keys.Count;


        // todo: look into returning cached enumerators instead...

        /* Get a copy of each added field in the set (in their enum defined order) */
        public IReadOnlyList<TKey> Keys => ExtractKeys(_keys).ToArray();

        /* Get a copy of each added value in the set (in their key's enum defined order) */
        public IReadOnlyList<TValue> Values => ExtractValues(_keys, _values).ToArray();

        /* Get a copy of each added {field, value} in the set (in their enum defined order) */
        public IReadOnlyList<(TKey key, TValue value)> Entries => ExtractEntries(_keys, _values).ToArray();

        /* What are all the fields defined in the backing enum, in order? */
        public IReadOnlyList<TKey> EnumFields => _keys.EnumFields;



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


        public override string ToString()
        {
            return $"{typeof(EnumMap<TKey, TValue>).Name}<{typeof(TKey)},{typeof(TValue)}>" +
                   $"{{ {string.Join(", ", ExtractValues(_keys, _values))} }}";
        }

        /* Is key included in our set of keys? */
        [Pure]
        public bool Contains(TKey key)
        {
            return _keys.Contains(key);
        }

        /* Is the other set a equivalent OR a subset of our keys? */
        [Pure]
        public bool Contains(in EnumSet<TKey> other)
        {
            return _keys.Contains(other);
        }

        /* Key-based indexer with bound checks and disabled value overwriting (ie readonly). */
        [Pure]
        public TValue this[TKey key]
        {
            get
            {
                if (!_keys.TryGetEnumOrdering(key, out int index) || !_keys.Contains(key))
                {
                    throw new ArgumentException($"Failed to lookup value - key {key} is not found in {_keys}");
                }
                return _values[index];
            }
        }

        /* If key is both valid enum and not found add it - otherwise throw. */
        public void Add(TKey key, TValue value)
        {
            // note that we only explicitly signal bad enum values here as that's the only place it can be added,
            // as everywhere else an undefined enum value is treated the same as a missing key
            if (!_keys.TryGetEnumOrdering(key, out int index))
            {
                throw new ArgumentException($"Failed to add entry - key {key} is not a defined field of {typeof(TKey)}");
            }
            if (!_keys.TryAdd(key))
            {
                throw new ArgumentException($"Failed to add entry - values cannot be overriden, key {key} already found in {_keys}");
            }

            _values[index] = value;
        }

        /* If key found remove entry - otherwise throw. */
        public void Remove(TKey key)
        {
            if (!_keys.TryGetEnumOrdering(key, out int index) || !_keys.TryRemove(key))
            {
                throw new ArgumentException($"Failed to remove entry - key {key} not found in {_keys}");
            }

            _values[index] = default;
        }


        /* If key is already in our map, then fetch it's value (exception free alternative to index lookup). */
        [Pure]
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (!_keys.TryGetEnumOrdering(key, out int index) || !_keys.Contains(key))
            {
                value = default;
                return false;
            }

            value = _values[index];
            return true;
        }

        /* If key is both defined and not found add it (exception free alternative to add). */
        public bool TryAdd(TKey key, TValue value)
        {
            if (!_keys.TryGetEnumOrdering(key, out int index) || !_keys.TryAdd(key))
            {
                return false;
            }

            _values[index] = value;
            return true;
        }

        /* If key found remove entry - otherwise throw (exception free alternative to remove). */
        public bool TryRemove(TKey key)
        {
            if (!_keys.TryGetEnumOrdering(key, out int index) || !_keys.TryRemove(key))
            {
                return false;
            }

            _values[index] = default;
            return true;
        }



        // todo: investigate just how much garbage these are creating, and consider replacing with list style enumerator struct

        private static IEnumerable<TKey> ExtractKeys(EnumSet<TKey> keys)
        {
            for (int i = 0; i < keys.Size; i++)
            {
                if (keys.TryGetEnumField(i, out TKey key) && keys.Contains(key))
                {
                    yield return key;
                }
            }
        }

        private static IEnumerable<TValue> ExtractValues(EnumSet<TKey> keys, TValue[] values)
        {
            for (int i = 0; i < keys.Size; i++)
            {
                if (keys.TryGetEnumField(i, out TKey key) && keys.Contains(key))
                {
                    yield return values[i];
                }
            }
        }

        private static IEnumerable<(TKey key, TValue value)> ExtractEntries(EnumSet<TKey> keys, TValue[] values)
        {
            for (int i = 0; i < keys.Size; i++)
            {
                if (keys.TryGetEnumField(i, out TKey key) && keys.Contains(key))
                {
                    yield return (key, values[i]);
                }
            }
        }
    }
}
