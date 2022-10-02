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
        public int Data  { get; private set; }
        public int Count { get; private set; }
        public int Size  { get; private set; }
        

        public BitSet(int size, bool value=false)
        {
            if (size <= 0)
            {
                throw new ArgumentException($"Bitset size cannot be zero or less");
            }

            Data  = 0;
            Count = 0;
            Size  = size;

            if (value)
            {
                Data  = ~0;
                Count = Size;
            }
            else
            {
                Data  = 0;
                Count = 0;
            }
        }

        [Pure] public bool IsTrue(int index)       => (Data & (1 << index)) != 0;
        [Pure] public bool IsSubset(int mask)      => (Data & mask) == mask;
        [Pure] public bool IsSubset(BitSet bitSet) => (Data & bitSet.Data) == bitSet.Data;

        public bool TryAdd(int index)
        {
            int element = 1 << index;
            if (index < 0 || index >= Size || (Data & element) != 0)
            {
                return false;
            }

            Data |= element;
            Count++;
            return true;
        }

        public bool TryRemove(int bitPosition)
        {
            int element = 1 << bitPosition;
            if (bitPosition < 0 || bitPosition >= Size || (Data & element) == 0)
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
