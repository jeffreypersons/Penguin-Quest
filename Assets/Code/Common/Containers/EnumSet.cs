using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;


namespace PQ.Common.Containers
{
    /*
    Simple ordered sequence of enums with set and flag operations.


    Essentially a layer on top of plain enums that allow toggling of flags without the need of declaring it for that.
    
    In other words, rather than writing [System.Flags] and assigning manual values (error prone and fragile to change!),
    this takes care of all the bit-shifting and validation needed to work with subsets of enums.


    Properties
    - intrinsically sorted by the order defined in the enum
    - constant time 'contains' check
    - generic (no boxing!) enum comparisons via comparer (relevant since == cannot be used with generic enum types)
    - upfront validation of enum constraints (that the values follow the pattern of 0,1,2,....,n-1,n)

    Notes
    - while the 64 max enum size restriction _could_ be lifted, though possibly a bad ROI as we shouldn't have such huge enums anyways
    - unlike the enum Flags attribute, the first/last value is NOT treated as a none and all field (and thus not needed in its declaration)
    */
    public struct EnumSet<T>
        where T : struct, Enum
    {
        public const int MinSize = 1;
        public const int MaxSize = 64;

        // only validate once per enum since defined at compile time
        // note that could be made thread safe using C#'s Lazy feature
        public static readonly EnumMetadata<T> EnumFields;
        static EnumSet()
        {
            EnumFields = new EnumMetadata<T>();
            if (EnumFields.Size < MinSize || EnumFields.Size > MaxSize)
            {
                throw new ArgumentException($"Bitset size must be in range [{MinSize}, {MaxSize}] - received {EnumFields.Size}");
            }

            foreach (var field in EnumFields.Entries())
            {
                if (EnumFields.IsDefault(field))
                {
                    throw new ArgumentException($"Enum values must match declaration order - received {EnumFields}");
                }
            }
        }
        
        private long Data  { readonly get; set;         }
        public int   Count { readonly get; private set; }
        public int   Size  { readonly get; private set; }

        public Type Type => typeof(T);
        public override string ToString() => $"{GetType().Name}<{typeof(T)}>{{ {string.Join(", ", Entries())} }}";
        
        public EnumSet(bool value = false)
        {
            if (value)
            {
                Data  = ~0;
                Count = EnumFields.Size;
                Size  = EnumFields.Size;
            }
            else
            {
                Data  = 0;
                Count = 0;
                Size  = EnumFields.Size;
            }
        }

        public EnumSet(in T[] flags) : this(value: false)
        {
            foreach (T flag in flags)
            {
                Add(flag);
            }
        }

        /* What are the enum fields included in our set, in order that they were declared? */
        public IEnumerable<T> Entries()
        {
            // todo: investigate just how much garbage this is creating
            foreach (T entry in EnumFields.Entries())
            {
                if (Contains(entry))
                    yield return entry;
            }
        }

        /* Is value included in our set? */
        [Pure]
        public bool Contains(T flag)
        {
            int index = (int)EnumFields.ValueOf(flag);
            long mask = 1L << index;
            return (Data & mask) != 0;
        }

        /* Is the other set a equivalent OR a subset of ours? */
        [Pure]
        public bool Contains(in EnumSet<T> other)
        {
            return (Data & other.Data) == other.Data;
        }

        /* If value is a valid enum that _is not_ already in our set, then include that flag. */
        public bool Add(T flag)
        {
            int index = (int)EnumFields.ValueOf(flag);
            long mask = 1L << index;
            if (index < 0 || index >= Size || (Data & mask) != 0)
            {
                return false;
            }

            Data |= mask;
            Count++;
            return true;
        }

        /* If value is a valid _is_ already in our set, then exclude that flag. */
        public bool Remove(T flag)
        {
            int index = (int)EnumFields.ValueOf(flag);
            long mask = 1L << index;
            if (index < 0 || index >= Size || (Data & mask) == 0)
            {
                return false;
            }

            Data &= mask;
            Count--;
            return true;
        }
    }
}
