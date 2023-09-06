using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;


namespace PQ._Experimental.Physics
{
    /*
    Simple ordered sequence of enums with set and flag operations.

    Overview
    - Essentially a layer on top of plain enums that allow toggling of flags without the need of declaring it for that.
      In other words, rather than writing [System.Flags] and assigning manual values (error prone and fragile to change!),
      this takes care of all the bit-shifting and validation needed to work with subsets of enums
    */
    public sealed class EnumSet<TKey>
        where TKey : struct, Enum
    {
        public const int MinSize = 1;
        public const int MaxSize = 64;

        // only validate once per enum since defined at compile time
        private static TKey[] CachedEnumValues;
        static EnumSet()
        {
            var type = typeof(TKey);
            var values = (TKey[])Enum.GetValues(type);
            var underlying = Enum.GetUnderlyingType(type);
            if (values.Length is < MinSize or > MaxSize)
            {
                throw new ArgumentException($"Bitset size must be in range [{MinSize}, {MaxSize}] - received {values.Length}");
            }
            for (int index = 0; index < values.Length; index++)
            {
                if (UnsafeUtility.EnumToInt(values[index]) != index)
                {
                    var signature = $"enum {type} : {underlying}";
                    var fields = string.Join(", ", values.Select(f => $"{f}={UnsafeUtility.EnumToInt(f)}"));
                    throw new ArgumentException($"Enum values must match declaration order - received enum {signature} {{ {fields} }}");
                }
            }
            CachedEnumValues = values;
        }

        // lookup by index - functionality we don't want outside the assembly, as clients should use the fields directly
        internal bool LookupKey(int index, out TKey key)
        {
            key = UnsafeUtility.As<int, TKey>(ref index);
            return index >= 0 && index < _size;
        }

        // reverse lookup by index - functionality we don't want outside the assembly, as clients should use the fields directly
        internal bool LookupIndex(TKey key, out int index)
        {
            index = UnsafeUtility.As<TKey, int>(ref key);
            return index >= 0 && index < _size;
        }

        private long         _flags;
        private int          _count;
        private readonly int _size;

        public Type Type  => typeof(TKey);
        public int  Count => _count;
        public int  Size  => _size;


        // todo: look into returning cached enumerators instead...

        /* Get a copy of each added field in the set (in their enum defined order) */
        public IReadOnlyList<TKey> Entries => ExtractEntries(_flags, _size).ToArray();

        /* What are all the fields defined in the backing enum, in order? */
        public IReadOnlyList<TKey> EnumFields => CachedEnumValues;
        

        public EnumSet(bool startFull = false)
        {
            if (startFull)
            {
                _flags = ~0;
                _count = CachedEnumValues.Length;
                _size  = CachedEnumValues.Length;
            }
            else
            {
                _flags = 0;
                _count = 0;
                _size  = CachedEnumValues.Length;
            }
        }

        public EnumSet(in TKey[] entries) : this(startFull: false)
        {
            if (entries == null)
            {
                return;
            }

            foreach (TKey entry in entries)
            {
                Add(entry);
            }
        }

        public override string ToString()
        {
            return $"{GetType().Name}<{typeof(TKey)}>" +
                   $"{{ {string.Join(", ", ExtractEntries(_flags, _size))} }}";
        }

        /* Is key included in our set? */
        [Pure]
        public bool Contains(TKey key)
        {
            LookupIndex(key, out int index);
            long mask = 1L << index;
            return (_flags & mask) != 0;
        }

        /* Is the other set a equivalent OR a subset of ours? */
        [Pure]
        public bool Contains(in EnumSet<TKey> other)
        {
            return (_flags & other._flags) == other._flags;
        }
        
        /* If key is both valid enum and not found add it - otherwise throw. */
        public void Add(TKey key)
        {
            LookupIndex(key, out int index);
            if (index < 0 || index >= _size)
            {
                throw new ArgumentException($"Failed to add entry - key {key} is not a defined field of {typeof(TKey)}");
            }

            long mask = 1L << index;
            if ((_flags & mask) != 0)
            {
                throw new ArgumentException($"Failed to add entry - values cannot be overriden, key {key} already found in {this}");
            }

            _flags |= mask;
            _count++;
        }

        /* If key found remove entry - otherwise throw. */
        public void Remove(TKey key)
        {
            LookupIndex(key, out int index);
            long mask = 1L << index;
            if (index < 0 || index >= _size || (_flags & mask) == 0)
            {
                throw new ArgumentException($"Failed to remove entry - key {key} not found in {this}");
            }

            _flags &= mask;
            --_count;
        }


        /* If key is both defined and not found add it - otherwise throw (exception free alternative to add). */
        public bool TryAdd(TKey key)
        {
            LookupIndex(key, out int index);
            long mask = 1L << index;
            if (index < 0 || index >= _size || (_flags & mask) != 0)
            {
                return false;
            }

            _flags |= mask;
            ++_count;
            return true;
        }

        /* If key found remove entry - otherwise throw (exception free alternative to remove). */
        public bool TryRemove(TKey key)
        {
            int index = UnsafeUtility.As<TKey, int>(ref key);
            long mask = 1L << index;
            if (index < 0 || index >= _size || (_flags & mask) == 0)
            {
                return false;
            }

            _flags &= mask;
            --_count;
            return true;
        }


        private static IEnumerable<TKey> ExtractEntries(long flags, int size)
        {
            for (int index = 0; index < size; index++)
            {
                if ((flags & (1L << index)) != 0)
                {
                    yield return CachedEnumValues[index];
                }
            }
        }
    }
}
