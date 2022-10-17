using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;


namespace PQ.Common.Containers
{
    /*
    Simple ordered sequence of enums with set and flag operations.

    Overview
    - Essentially a layer on top of plain enums that allow toggling of flags without the need of declaring it for that.
      In other words, rather than writing [System.Flags] and assigning manual values (error prone and fragile to change!),
      this takes care of all the bit-shifting and validation needed to work with subsets of enums

    Properties
    - plain enums only      : all enum fields default to unique integers from 0 to size-1
    - intrinsically ordered : keys are stored in the order defined by the enum
    - low memory footprint  : garbage free type conversions, up-front allocation, and one-time only enum validation/caching
    - scalable              : constant time contains/get/add/remove, constant memory usage

    Notes
    - C# collection interfaces intentionally avoided as this is a very specialized container, and we want to avoid extra boxing/virtual calls
    - while the 64 max enum size restriction _could_ be lifted, though possibly a bad ROI as we shouldn't have such huge enums anyways
    - unlike the enum Flags attribute, the first/last value is NOT treated as a none and all field (and thus not needed in its declaration)
    */
    public sealed class EnumSet<TKey>
        where TKey : struct, Enum
    {
        public const int MinSize = 1;
        public const int MaxSize = 64;

        // only validate once per enum since defined at compile time
        // note that could be made thread safe using C#'s Lazy feature
        private static EnumMetadata<TKey> EnumFieldData { get; set; }
        static EnumSet()
        {
            EnumFieldData = new EnumMetadata<TKey>();
            if (EnumFieldData.Size < MinSize || EnumFieldData.Size > MaxSize)
            {
                throw new ArgumentException($"Bitset size must be in range [{MinSize}, {MaxSize}] - received {EnumFieldData.Size}");
            }

            for (int i = 0; i < EnumFieldData.Size; i++)
            {
                if (!EnumFieldData.IsValueDefined(i))
                {
                    throw new ArgumentException($"Enum values must match declaration order - received {EnumFieldData}");
                }
            }
        }

        // is enum value valid - functionality we don't want outside the assembly,
        // as everywhere else an undefined enum value is treated the same as a missing key
        internal bool IsEnumFieldDefined(TKey enumField)
        {
            var index = EnumFieldData.FieldToValue<int>(enumField);
            return index >= 0 && index < Size;
        }

        // lookup by index - functionality we don't want outside the assembly, as clients should use the fields directly
        internal bool TryGetEnumField(int enumPosition, out TKey enumField)
        {
            enumField = EnumFieldData.ValueToField(enumPosition);
            return enumPosition >= 0 && enumPosition < Size;
        }

        // reverse lookup by index - functionality we don't want outside the assembly, as clients should use the fields directly
        internal bool TryGetEnumOrdering(TKey enumField, out int enumPosition)
        {
            enumPosition = EnumFieldData.FieldToValue<int>(enumField);
            return enumPosition >= 0 && enumPosition < Size;
        }


        private long Data  { get; set;         }
        public  int  Count { get; private set; }
        public  int  Size  { get; private set; }

        public Type Type => typeof(TKey);


        // todo: look into returning cached enumerators instead...

        /* Get a copy of each added field in the set (in their enum defined order) */
        public IReadOnlyList<TKey> Entries => ExtractItems(Data, Size).ToArray();

        /* What are all the fields defined in the backing enum, in order? */
        public IReadOnlyList<TKey> EnumFields => EnumFieldData.Fields;
        

        public EnumSet(bool value = false)
        {
            if (value)
            {
                Data  = ~0;
                Count = EnumFieldData.Size;
                Size  = EnumFieldData.Size;
            }
            else
            {
                Data  = 0;
                Count = 0;
                Size  = EnumFieldData.Size;
            }
        }

        public EnumSet(in TKey[] entries) : this(value: false)
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
                   $"{{ {string.Join(", ", ExtractItems(Data, Size))} }}";
        }

        /* Is key included in our set? */
        [Pure]
        public bool Contains(TKey key)
        {
            int index = EnumFieldData.FieldToValue<int>(key);
            long mask = 1L << index;
            return (Data & mask) != 0;
        }

        /* Is the other set a equivalent OR a subset of ours? */
        [Pure]
        public bool Contains(in EnumSet<TKey> other)
        {
            return (Data & other.Data) == other.Data;
        }
        
        /* If key is both valid enum and not found add it - otherwise throw. */
        public void Add(TKey key)
        {
            int index = EnumFieldData.FieldToValue<int>(key);
            if (index < 0 || index >= Size)
            {
                throw new ArgumentException($"Failed to add entry - key {key} is not a defined field of {typeof(TKey)}");
            }

            long mask = 1L << index;
            if ((Data & mask) != 0)
            {
                throw new ArgumentException($"Failed to add entry - values cannot be overriden, key {key} already found in {this}");
            }

            Data |= mask;
            Count++;
        }

        /* If key found remove entry - otherwise throw. */
        public void Remove(TKey key)
        {
            int index = EnumFieldData.FieldToValue<int>(key);
            long mask = 1L << index;
            if (index < 0 || index >= Size || (Data & mask) == 0)
            {
                throw new ArgumentException($"Failed to remove entry - key {key} not found in {this}");
            }

            Data &= mask;
            Count--;
        }


        /* If key is both defined and not found add it - otherwise throw (exception free alternative to add). */
        public bool TryAdd(TKey key)
        {
            int index = EnumFieldData.FieldToValue<int>(key);
            long mask = 1L << index;
            if (index < 0 || index >= Size || (Data & mask) != 0)
            {
                return false;
            }

            Data |= mask;
            Count++;
            return true;
        }

        /* If key found remove entry - otherwise throw (exception free alternative to remove). */
        public bool TryRemove(TKey key)
        {
            int index = EnumFieldData.FieldToValue<int>(key);
            long mask = 1L << index;
            if (index < 0 || index >= Size || (Data & mask) == 0)
            {
                return false;
            }

            Data &= mask;
            Count--;
            return true;
        }



        // todo: investigate just how much garbage these are creating, and consider replacing with list style enumerator struct

        private static IEnumerable<TKey> ExtractItems(long data, int size)
        {
            for (int i = 0; i < size; i++)
            {
                if ((data & (1L << i)) != 0)
                {
                    yield return EnumFieldData.Fields[i];
                }
            }
        }
    }
}
