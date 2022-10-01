using System;
using System.Diagnostics.Contracts;


namespace PQ.Common.Containers
{
    /*
    Fixed size array of bits.

    Unlike C#'s BitVector32 collection, this is more similar to C++'s bitset, with more support
    for checking non-contiguous subsets (as opposed to BitVector32.Section).
    */
    public struct BitSet : IEquatable<BitSet>, IComparable<BitSet>
    {
        public int Value { get; private set; }
        public int Count { get; private set; }
        public int Size  { get; private set; }
        

        public BitSet(int size, bool defaultBitValue=false)
        {
            if (size <= 0)
            {
                throw new ArgumentException($"Bitset size cannot be zero or less");
            }

            Value = 0;
            Count = 0;
            Size  = size;

            if (defaultBitValue)
            {
                SetAll();
            }
        }

        public void SetAll()
        {
            Value = ~0;
            Count = Size;
        }

        public void RemoveAll()
        {
            Value = 0;
            Count = 0;
        }

        public bool TryAdd(int bitPosition)
        {
            int element = 1 << bitPosition;
            if (bitPosition < 0 || bitPosition >= Size || (Value & element) != 0)
            {
                return false;
            }

            Value |= element;
            Count++;
            return true;
        }

        public bool TryRemove(int bitPosition)
        {
            int element = 1 << bitPosition;
            if (bitPosition < 0 || bitPosition >= Size || (Value & element) == 0)
            {
                return false;
            }

            Value &= element;
            Count--;
            return true;
        }

        [Pure]
        public bool IsSet(int bitPosition)
        {
            // note that no index check is necessary since that's enforced in Set()
            return (Value & (1 << bitPosition)) != 0;
        }

        [Pure]
        public bool IsSubset(int mask)
        {
            // explicitly check against given mask to ensure that zero
            // is not included unless the mask is as well
            return (Value & mask) == mask;
        }

        [Pure]
        public bool IsSubset(BitSet bitSet)
        {
            // explicitly check against given mask to ensure that zero
            // is not included unless the mask is as well
            return (Value & bitSet.Value) == bitSet.Value;
        }

        [Pure]
        public static string ToString(BitSet bitSet)
        {
            var bits = new char[bitSet.Size];
            for (int i = 0; i < bits.Length; i++)
            {
                bits[i] = bitSet.IsSet(i) ? '1' : '0';
            }
            return new string(bits);
        }

        bool IEquatable<BitSet>.Equals(BitSet other) => Value == other.Value;
        int IComparable<BitSet>.CompareTo(BitSet other) => Value.CompareTo(other.Value);

        public override string  ToString()         => ToString(bitSet: this);
        public override int     GetHashCode()      => HashCode.Combine(Value);
        public override bool    Equals(object obj) => ((IEquatable<BitSet>)this).Equals((BitSet)obj);

        public static bool operator ==(BitSet left, BitSet right) => ((IEquatable<BitSet>)left).Equals(right);
        public static bool operator !=(BitSet left, BitSet right) => !(left == right);
    }
}
