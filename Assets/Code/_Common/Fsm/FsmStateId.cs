using System;
using System.Linq;
using System.Diagnostics.Contracts;
using Unity.Collections.LowLevel.Unsafe;
using PQ.Common.Containers;


namespace PQ.Common.Fsm
{
    /*
    Layer on top of generic enum with caching and validation, done statically only once per generic enum type.

    That is, since this functionality is intended to extend the generic StateId param used in other classes,
    it is only to be used statically, as enum definitions are processed at compile time, so we only need to do things once.
    */
    public static class FsmStateId<Id>
        where Id : struct, Enum
    {
        // since enums are evaluated at compile time and bound to corresponding template parameter,
        // we only need to validate once, when this file first loads
        private static readonly Type     _type   = typeof(Id);
        private static readonly string[] _names  = Enum.GetNames(_type);
        private static readonly BitSet   _bitset = new(_names.Length, true);
        private static readonly int      _count  = _names.Length;

        public static Type Type  => _type;
        public static int  Count => _count;

        static FsmStateId()
        {
            if (!AreAllEnumValuesDefault<Id>())
            {
                throw new ArgumentException($"Enum values must be int32 with default values from 0 to n - received {AsUserFriendlyString<Id>()} instead");
            }
        }

        [Pure] public static bool HasId(Id id)         => _bitset.IsSet(UnsafeUtility.As<Id, int>(ref id));
        [Pure] public static bool HasIndex(int index)  => _bitset.IsSet(index);

        [Pure]
        public static Id AsId(int index)
        {
            if (!_bitset.IsSet(index))
            {
                throw new ArgumentException($"Index {index} is an invalid - not defined for {AsUserFriendlyString<Id>()}");
            }
            return UnsafeUtility.As<int, Id>(ref index);
        }

        [Pure]
        public static int AsIndex(Id id)
        {
            int index = UnsafeUtility.As<Id, int>(ref id);
            if (!_bitset.IsSet(index))
            {
                throw new ArgumentException($"Id {id} is an invalid - not defined for {AsUserFriendlyString<Id>()}");
            }
            return index;
        }

        [Pure]
        public static string AsName(int index)
        {
            if (!_bitset.IsSet(index))
            {
                throw new ArgumentException($"Id {index} is an invalid - not defined for {AsUserFriendlyString<Id>()}");
            }
            return _names[index];
        }

        [Pure]
        public static string AsName(Id id)
        {
            int index = UnsafeUtility.As<Id, int>(ref id);
            if (!_bitset.IsSet(index))
            {
                throw new ArgumentException($"Id {id} is an invalid - not defined for {AsUserFriendlyString<Id>()}");
            }
            return _names[index];
        }

        [Pure]
        private static bool AreAllEnumValuesDefault<TEnum>()
            where TEnum : struct, Enum
        {
            if (Enum.GetUnderlyingType(_type) != typeof(int))
            {
                return false;
            }

            // note that checking for name existence is the most performant least garbage producing method,
            // compare to the much more reflection heavy Enum.GetValues() and Enum.IsDefined()
            for (int i = 0; i < _bitset.Size; i++)
            {
                if (Enum.GetName(_type, i) == null)
                {
                    return false;
                }
            }
            return true;
        }

        /* Performance warning - uses reflection and multiple array allocations. */
        [Pure]
        private static string AsUserFriendlyString<TEnum>()
            where TEnum : struct, Enum
        {
            [Pure] static string _EnumFieldToString(string name, TEnum value) =>
                $"{name}={UnsafeUtility.As<TEnum, long>(ref value)}";


            var enumType       = typeof(TEnum);
            var underlyingType = Enum.GetUnderlyingType(enumType);
            var names          = Enum.GetNames(enumType);
            var values         = (TEnum[])Enum.GetValues(enumType);

            string enumName   = enumType.FullName;
            string typeName   = Enum.GetUnderlyingType(enumType).FullName;
            string enumFields = string.Join(',', names.Zip(values, _EnumFieldToString));

            return $"enum {enumType.FullName}:{typeName} {{ {enumFields} }}";
        }
    }
}
