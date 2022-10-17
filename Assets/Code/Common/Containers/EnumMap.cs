using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;


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
        public IReadOnlyList<TKey> Keys => ExtractAddedKeys(_keys).ToArray();

        /* Get a copy of each added value in the set (in their key's enum defined order) */
        public IReadOnlyList<TValue> Values => ExtractAddedValues(_keys, _values).ToArray();

        /* Get a copy of each added {field, value} in the set (in their enum defined order) */
        public IReadOnlyList<(TKey key, TValue value)> Entries => ExtractAddedEntries(_keys, _values).ToArray();

        /* What are all the fields defined in the backing enum, in order? */
        public IReadOnlyList<TKey> EnumFields => _keys.EnumFields;


        public override string ToString() =>
            $"{typeof(EnumMap<TKey, TValue>).Name}<{typeof(TKey)}>" +
            $"{{ {string.Join(", ", ExtractAddedValues(_keys, _values))} }}";

        public EnumMap(in (TKey key, TValue value)[] entries=null)
        {
            _keys   = new EnumSet<TKey>();
            _values = new TValue[_keys.Size];

            if (entries == null)
            {
                return;
            }

            foreach ((TKey key, TValue value) in entries)
            {
                if (!TryAdd(key, value))
                {
                    throw new ArgumentException(
                        $"Cannot add undefined or duplicate keys - " +
                        $"possible keys include {{{string.Join(", ", EnumFields)}}} " +
                        $"yet received [{string.Join(", ", entries)}]");
                }
            }
        }

        /* Is key a valid enum? */
        [Pure]
        public bool IsDefined(TKey key)
        {
            return _keys.Contains(key);
        }

        /* Is key included in our set of keys? */
        [Pure]
        public bool Contains(TKey key)
        {
            return _keys.Contains(key);
        }

        /* Is the other set a equivalent OR a subset of ours? */
        [Pure]
        public bool Contains(in EnumSet<TKey> other)
        {
            return _keys.Contains(other);
        }

        /* If key is _already_ in our map, then fetch it's value. */
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

        /* If value is a valid enum that _is not_ already in our map, then add the entry. */
        public bool TryAdd(TKey key, TValue value)
        {
            if (!_keys.TryGetEnumOrdering(key, out int index) || !_keys.TryAdd(key))
            {
                return false;
            }
            _values[index] = value;
            return true;
        }

        /* If value is a valid enum that _is_ already in our map, then remove the entry. */
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

        private static IEnumerable<TKey> ExtractAddedKeys(EnumSet<TKey> keys)
        {
            for (int i = 0; i < keys.Size; i++)
            {
                if (keys.TryGetEnumField(i, out TKey key) && keys.Contains(key))
                {
                    yield return key;
                }
            }
        }

        private static IEnumerable<TValue> ExtractAddedValues(EnumSet<TKey> keys, TValue[] values)
        {
            for (int i = 0; i < keys.Size; i++)
            {
                if (keys.TryGetEnumField(i, out TKey key) && keys.Contains(key))
                {
                    yield return values[i];
                }
            }
        }

        private static IEnumerable<(TKey key, TValue value)> ExtractAddedEntries(EnumSet<TKey> keys, TValue[] values)
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
