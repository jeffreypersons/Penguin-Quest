using System;
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

        public BitSet(int size, bool value=false)
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

        [Pure] public bool IsTrue(int index)       => (Data & (1 << index)) != 0;
        [Pure] public bool IsSubset(long mask)     => (Data & mask) == mask;
        [Pure] public bool IsSubset(BitSet bitSet) => (Data & bitSet.Data) == bitSet.Data;


        public bool TryAdd(int index)
        {
            long element = 1 << index;
            if (index < 0 || index >= Size || (Data & element) != 0)
            {
                return false;
            }

            Data |= element;
            Count++;
            return true;
        }

        public bool TryRemove(int index)
        {
            long element = 1 << index;
            if (index < 0 || index >= Size || (Data & element) == 0)
            {
                return false;
            }

            Data &= element;
            Count--;
            return true;
        }

        [Pure]
        public static string ToString(BitSet bitSet)
        {
            var bits = new char[bitSet.Size];
            for (int i = 0; i < bits.Length; i++)
            {
                bits[i] = bitSet.IsTrue(i) ? '1' : '0';
            }
            return new string(bits);
        }

        bool IEquatable<BitSet>.Equals(BitSet other)              =>  Data == other.Data && Count == other.Count && Size == other.Size;
        int IComparable<BitSet>.CompareTo(BitSet other)           =>  Data.CompareTo(other.Data);
        public override string  ToString()                        =>  ToString(bitSet: this);
        public override int     GetHashCode()                     =>  HashCode.Combine(Data);
        public override bool    Equals(object obj)                =>  ((IEquatable<BitSet>)this).Equals((BitSet)obj);
        public static bool operator ==(BitSet left, BitSet right) =>  ((IEquatable<BitSet>)left).Equals(right);
        public static bool operator !=(BitSet left, BitSet right) => !((IEquatable<BitSet>)left).Equals(right);
    }
}
