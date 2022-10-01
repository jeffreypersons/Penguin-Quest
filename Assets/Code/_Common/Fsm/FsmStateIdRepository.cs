using System;
using System.Linq;
using System.Diagnostics.Contracts;
using Unity.Collections.LowLevel.Unsafe;
using PQ.Common.Containers;
using System.Collections.Generic;


namespace PQ.Common.Fsm
{
    /*
    Layer on top of generic enum with caching and validation, done statically only once per generic enum type.

    That is, since this functionality is intended to extend the generic StateId param used in other classes,
    it is only to be used statically, as enum definitions are processed at compile time, so we only need to do things once.
    */
    public static class FsmStateIdRepository<TEnum>
        where TEnum : struct, Enum
    {
        // since enums are evaluated at compile time and bound to corresponding template parameter,
        // we only need to validate once, when this file first loads

        private static readonly string[] _names;
        private static readonly Type     _type;
        private static readonly BitSet   _bitset;
        private static readonly string   _description;


        static FsmStateIdRepository()
        {
            _names       = Enum.GetNames(typeof(TEnum));
            _type        = typeof(TEnum);
            _bitset      = new(_names.Length, true);
            _description = $"{_type.FullName} {{ {string.Join(',', _names)} }}";
            
            ThrowIf(!AreAllEnumValuesDefault(), "Enum values must be int32 with default values from 0 to n");
        }

        public static Type   Type   => _type;
        public static int    Count  => _bitset.Count;
        public static string String => _description;

        public static IEnumerable<(int index, string name, TEnum field)> Fields()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return (i, _names[i], UnsafeUtility.As<int, TEnum>(ref i));
            }
        }


        [Pure] public static bool IsDefined(int index) => _bitset.IsSet(index);
        [Pure] public static bool IsDefined(TEnum id)  => _bitset.IsSet(UnsafeUtility.As<TEnum, int>(ref id));


        [Pure]
        public static int GetIndex(TEnum id)
        {
            int index = UnsafeUtility.As<TEnum, int>(ref id);
            ThrowIf(!_bitset.IsSet(index), $"Cannot look up index since id {id} is not defined");
            return index;
        }

        [Pure]
        public static string GetName(TEnum id)
        {
            int index = UnsafeUtility.As<TEnum, int>(ref id);
            ThrowIf(!_bitset.IsSet(index), $"Cannot look up name since id {id} is not defined");
            return _names[index];
        }
        
        [Pure]
        public static TEnum GetValue(int index)
        {
            ThrowIf(!_bitset.IsSet(index), $"Cannot look up id since index {index} is not defined");
            return UnsafeUtility.As<int, TEnum>(ref index);
        }



        [Pure]
        private static void ThrowIf(bool condition, string message)
        {
            if (!condition)
            {
                return;
            }

            var name   = _type.FullName;
            var type   = Enum.GetUnderlyingType(_type).FullName;
            var fields = _names.Zip(Enum.GetValues(_type).Cast<long>(), (k, v) => $"{k}={v}");

            throw new ArgumentException($"{message} - received enum {name} : {type} {{ {string.Join(',', fields)} }}");
        }

        [Pure]
        private static bool AreAllEnumValuesDefault()
        {
            if (Enum.GetUnderlyingType(_type) != typeof(int))
            {
                return false;
            }

            // note that checking for name existence is the most performant least garbage producing method,
            // compare to the much more reflection heavy Enum.GetValues() and Enum.IsDefined()
            for (int i = 0; i < _bitset.Count; i++)
            {
                if (Enum.GetName(_type, i) == null)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
