using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;


namespace PQ.Common.Containers
{
    /*
    Low overhead container for mapping enum fields to values.

    Note that we intentionally don't extend any of C#'s collection interfaces, as this is a very specialized container,
    and we want to avoid the overhead of any virtual calls/additionally boxing.

    Properties
    - fixed size            : each enum field is unique and has a corresponding value
    - plain enums only      : all enum fields default to unique integers from 0 to size-1
    - intrinsically ordered : values are stored in the order defined by the enum
    - fast enum indexing    : constant time lookups via enum keys
    - cache friendly        : values stored contiguously
    - low memory footprint  : garbage free type conversions, up-front allocation, and one-time only enum validation/caching
    */
    public sealed class EnumMap<TKey, TValue>
        where TKey : struct, Enum
    {
        // note that this could be made thread safe using C#'s Lazy feature
        private static EnumMetadata<TKey> EnumFields { get; set; }
        static EnumMap()
        {
            EnumFields = new EnumMetadata<TKey>();
        }


        private readonly EnumSet<TKey> _keys;
        private readonly TValue[]      _values;

        public Type KeyType   => typeof(TKey);
        public Type ValueType => typeof(TValue);
        public int  Count     => _keys.Count;
        public override string ToString() =>
            $"{typeof(EnumMap<TKey, TValue>).Name}<{typeof(TKey)}>{{ {string.Join(", ", Entries())} }}";

        public EnumMap(in (TKey key, TValue value)[] entries=null)
        {
            _keys   = new EnumSet<TKey>();
            _values = new TValue[EnumFields.Size];

            if (entries == null)
            {
                return;
            }

            foreach ((TKey key, TValue value) in entries)
            {
                if (!Add(key, value))
                {
                    throw new ArgumentException(
                        $"Cannot add undefined or duplicate keys - " +
                        $"possible keys include {{{string.Join(", ", EnumFields.Names)}}} " +
                        $"yet received [{string.Join(", ", entries)}]");
                }
            }
        }

        /* What are the enum fields included in our set, in order? */
        public IEnumerable<(TKey key, TValue value)> Entries()
        {
            // todo: investigate just how much garbage this is creating, and consider replacing with list style enumerator struct
            foreach (TKey key in _keys.Entries())
            {
                if (TryGetValue(key, out TValue value))
                {
                    yield return (key, value);
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
            if (!_keys.Contains(key))
            {
                value = default;
                return false;
            }
            value = _values[EnumFields.AsValue<int>(key)];
            return true;
        }

        /* If value is a valid enum that _is not_ already in our map, then add the entry. */
        public bool Add(TKey key, TValue value)
        {
            if (!_keys.Add(key))
            {
                return false;
            }
            _values[EnumFields.AsValue<int>(key)] = value;
            return true;
        }

        /* If value is a valid enum that _is_ already in our map, then remove the entry. */
        public bool Remove(TKey key)
        {
            if (!_keys.Remove(key))
            {
                return false;
            }
            _values[EnumFields.AsValue<int>(key)] = default;
            return true;
        }
    }
}
