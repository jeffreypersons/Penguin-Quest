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
    public sealed class EnumSet<T>
        where T : struct, Enum
    {
        public const int MinSize = 1;
        public const int MaxSize = 64;

        // only validate once per enum since defined at compile time
        // note that could be made thread safe using C#'s Lazy feature
        private static EnumMetadata<T> EnumFields { get; set; }
        static EnumSet()
        {
            EnumFields = new EnumMetadata<T>();
            if (EnumFields.Size < MinSize || EnumFields.Size > MaxSize)
            {
                throw new ArgumentException($"Bitset size must be in range [{MinSize}, {MaxSize}] - received {EnumFields.Size}");
            }

            for (int i = 0; i < EnumFields.Size; i++)
            {
                if (!EnumFields.IsDefined(i))
                {
                    throw new ArgumentException($"Enum values must match declaration order - received {EnumFields}");
                }
            }
        }


        private long Data  { get; set;         }
        public  int  Count { get; private set; }
        public  int  Size  { get; private set; }

        public Type Type => typeof(T);
        public override string ToString() => $"{GetType().Name}<{typeof(T)}>{{ {string.Join(", ", Flags(Data, Size))} }}";
        
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
                if (!Add(flag))
                {
                    throw new ArgumentException(
                        $"Cannot add undefined or duplicate flags - " +
                        $"possible flags include {{{string.Join(", ", EnumFields.Names)}}} " +
                        $"yet received [{string.Join(", ", flags)}]");
                }
            }
        }

        /* What are the enum fields included in our set, in order that they were declared? */
        public IEnumerable<T> Entries()
        {
            return Flags(Data, Size);
        }

        /* Is value included in our set? */
        [Pure]
        public bool Contains(T flag)
        {
            int index = EnumFields.AsValue<int>(flag);
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
            int index = EnumFields.AsValue<int>(flag);
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
            int index = EnumFields.AsValue<int>(flag);
            long mask = 1L << index;
            if (index < 0 || index >= Size || (Data & mask) == 0)
            {
                return false;
            }

            Data &= mask;
            Count--;
            return true;
        }

        // todo: investigate just how much garbage this is creating
        private static IEnumerable<T> Flags(long data, int size)
        {
            for (int i = 0; i < size; i++)
            {
                if ((data & (1L << i)) != 0)
                {
                    yield return EnumFields.Fields[i];
                }
            }
        }
    }
}
