using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;


namespace PQ.Common.Containers
{
    /*
    Fixed size array of bits.

    Unlike C#'s BitVector32 collection, this is more similar to C++'s bitset, with more support
    for checking non-contiguous subsets (as opposed to BitVector32.Section).

    Note that no index checking is necessary outside of the mutating methods that strictly enforce it.
    */
    public struct BitSet : IEquatable<BitSet>, IComparable<BitSet>
    {
        public const int MinSize = 1;
        public const int MaxSize = 64;

        public long Data  { get; private set; }
        public int  Count { get; private set; }
        public int  Size  { get; private set; }

        public BitSet(int size, bool value = false)
        {
            if (size < MinSize || size > MaxSize)
            {
                throw new ArgumentException($"Bitset size must be in range [{MinSize}, {MaxSize}] - received {size}");
            }

            if (value)
            {
                Data  = ~0;
                Count = size;
                Size  = size;
            }
            else
            {
                Data  = 0;
                Count = 0;
                Size  = size;
            }
        }

        /* Is the ith bit set to true? */
        [Pure] public bool HasIndex(int index)     => (Data & (1L << index)) != 0;

        /* Is given bitset a subset of ours? */
        [Pure] public bool IsSubset(BitSet bitSet) => (Data & bitSet.Data) == bitSet.Data;


        /* If ith bit false, set to true. */
        public bool TryAdd(int index)
        {
            long mask = 1L << index;
            if (index < 0 || index >= Size || (Data & mask) != 0)
            {
                return false;
            }

            Data |= mask;
            Count++;
            return true;
        }

        /* If ith bit true, set to false. */
        public bool TryRemove(int index)
        {
            long mask = 1L << index;
            if (index < 0 || index >= Size || (Data & mask) == 0)
            {
                return false;
            }

            Data &= mask;
            Count--;
            return true;
        }

        /* Retrieve positions of all set bits. */
        public IEnumerable<int> Indices()
        {
            for (int i = 0; i < Count; i++)
            {
                if (HasIndex(i))
                {
                    yield return i;
                }
            }
        }

        bool IEquatable<BitSet>.Equals(BitSet other)              =>  Data == other.Data && Count == other.Count && Size == other.Size;
        int IComparable<BitSet>.CompareTo(BitSet other)           =>  Data.CompareTo(other.Data);
        public override string  ToString()                        =>  AsBitString(Data, Size);
        public override int     GetHashCode()                     =>  HashCode.Combine(Data);
        public override bool    Equals(object obj)                =>  ((IEquatable<BitSet>)this).Equals((BitSet)obj);
        public static bool operator ==(BitSet left, BitSet right) =>  ((IEquatable<BitSet>)left).Equals(right);
        public static bool operator !=(BitSet left, BitSet right) => !((IEquatable<BitSet>)left).Equals(right);



        // assumes [start < end] AND [(end - start) <= max]
        [Pure]
        public static long CreateMask(int startIndex, int endIndex, int maxLength)
        {
            long maskFrom0ToStart = (1L << startIndex) - 1;
            long maskFrom0ToEnd   = (1L << endIndex)   - 1;
            return maskFrom0ToStart ^ maskFrom0ToEnd;
        }

        [Pure]
        private static string AsBitString(long data, int length)
        {
            return Convert
                .ToString(value: data, toBase: 2)
                .PadLeft(totalWidth: length, paddingChar: '0');
        }
    }
}
