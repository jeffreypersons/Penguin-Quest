using System;


namespace PQ.Common.Extensions
{
    /*
    Fixed size array of bits.

    Unlike C#'s BitVector32 collection, this is more similar to C++'s bitset, with more support
    for checking non-contiguous subsets (as opposed to BitVector32.Section).
    */
    public struct BitSet : IEquatable<BitSet>, IComparable<BitSet>
    {
        public int Value    { get; private set; }
        public int SetCount { get; private set; }
        public int Length   { get; private set; }
        

        public BitSet(int length)
        {            
            if (length < 0)
            {
                throw new ArgumentException($"Bitset size cannot be negative");
            }            
            Value    = 0;
            SetCount = 0;
            Length   = length;
        }

        public bool TryAdd(int index)
        {
            int element = 1 << index;
            if (index < 0 || index >= Length || (Value & element) != 0)
            {
                return false;
            }

            Value |= element;
            SetCount++;
            return true;
        }

        public bool TryRemove(int index)
        {
            int element = 1 << index;
            if (index < 0 || index >= Length || (Value & element) == 0)
            {
                return false;
            }

            Value &= element;
            SetCount--;
            return true;
        }

        public bool IsSet(int index)
        {
            // note that no index check is necessary since that's enforced in Set()
            return (Value & (1 << index)) != 0;
        }

        public bool IsSubset(int mask)
        {
            // explicitly check against given mask to ensure that zero
            // is not included unless the mask is as well
            return (Value & mask) == mask;
        }

        public static string ToString(BitSet bitSet)
        {
            var bits = new char[bitSet.Length];
            for (int i = 0; i < bits.Length; i++)
            {
                bits[i] = bitSet.IsSet(i) ? '1' : '0';
            }
            return new string(bits);
        }

        
        public override string  ToString()         => ToString(bitSet: this);
        public override int     GetHashCode()      => HashCode.Combine(Value);
        public override bool    Equals(object obj) => obj is BitSet && Equals((BitSet)obj);

        bool IEquatable<BitSet>.Equals(BitSet other)    => Value == other.Value;
        int IComparable<BitSet>.CompareTo(BitSet other) => Value.CompareTo(other.Value);

        public static bool operator ==(BitSet left, BitSet right) => left.Value == right.Value;
        public static bool operator !=(BitSet left, BitSet right) => left.Value != right.Value;
    }
}
