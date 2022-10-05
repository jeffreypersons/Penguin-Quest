using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Unity.Collections.LowLevel.Unsafe;
using PQ.Common.Containers;


namespace PQ.Common.Fsm
{
    /*
    Layer on top of generic enum with caching and validation, done statically only once per generic enum type.

    That is, since this functionality is intended to extend the generic StateId param used in other classes,
    it is only to be used statically, as enum definitions are processed at compile time, so we only need to do things once.

    Note that since enums are a value type, we can't use ==, so this is the best we can do (no boxing!) for id comparisons.
    */
    internal class FsmStateIdCache<TEnum>
        where TEnum : struct, Enum
    {
        // since enums are evaluated at compile time and bound to corresponding template parameter,
        // we only need to validate once, when this instance is first used
        private static FsmStateIdCache<TEnum> _instance;
        public static FsmStateIdCache<TEnum> Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FsmStateIdCache<TEnum>();
                }
                return _instance;
            }
        }

        private readonly Type     _type;
        private readonly string[] _names;
        private readonly BitSet   _bitset;

        private readonly Comparer<TEnum>         _valueComparer;
        private readonly EqualityComparer<TEnum> _equalityComparer;

        private const int MinIndexValue = BitSet.MinSize;
        private const int MaxIndexValue = BitSet.MaxSize;

        private FsmStateIdCache()
        {
            _names            = ExtractNames<TEnum>();
            _type             = typeof(TEnum);
            _bitset           = new(_names.Length, true);
            _equalityComparer = EqualityComparer<TEnum>.Default;
            _valueComparer    = Comparer<TEnum>.Default;
        }

        public Type Type  => _type;
        public int  Count => _bitset.Count;

        public Comparer<TEnum>         ValueComparer    => _valueComparer;
        public EqualityComparer<TEnum> EqualityComparer => _equalityComparer;


        public IEnumerable<(int index, string name, TEnum id)> Fields()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return (i, _names[i], UnsafeUtility.As<int, TEnum>(ref i));
            }
        }

        [Pure]
        public bool TryGetIndex(TEnum id, out int index)
        {
            index = UnsafeUtility.As<TEnum, int>(ref id);
            if (!_bitset.HasIndex(index))
            {
                index = -1;
                return false;
            }
            return true;
        }

        [Pure]
        public int GetIndex(in TEnum id)
        {
            ThrowIf(!TryGetIndex(id, out int index), $"Cannot get index - id {id} not defined");
            return index;
        }

        [Pure]
        public string GetName(in TEnum id)
        {
            ThrowIf(!TryGetIndex(id, out int index), $"Cannot get name - id {id} not defined");
            return _names[index];
        }


        [Pure]
        public bool TryGetId(int index, out TEnum id)
        {
            id = UnsafeUtility.As<int, TEnum>(ref index);
            return _bitset.HasIndex(index);
        }

        [Pure]
        public TEnum GetId(int index)
        {
            ThrowIf(!TryGetId(index, out TEnum id), $"Cannot get id - index {index} not defined");
            return id;
        }
        


        [Pure]
        private static string[] ExtractNames<T>() where T : struct, Enum
        {
            var type  = typeof(T);
            var names = Enum.GetNames(type);
            var count = names.Length;

            ThrowIf(count < MinIndexValue || count > MaxIndexValue,
                $"Enum values must be default from {MinIndexValue} to {MaxIndexValue}");
            for (int i = 0; i < count; i++)
            {
                ThrowIf(Enum.GetName(type, i) == null, "Enum values must be default from 0 to n");
            }

            return names;
        }


        [Pure]
        private static void ThrowIf(bool hasError, string message)
        {
            if (!hasError)
            {
                return;
            }
            
            [Pure] static string _EnumFieldToString(string name, TEnum value) =>
                $"{name}={UnsafeUtility.As<TEnum, long>(ref value)}";

            var type   = typeof(TEnum);
            var names  = Enum.GetNames(type);
            var values = (TEnum[])Enum.GetValues(type);

            string enumName   = type.FullName;
            string typeName   = Enum.GetUnderlyingType(type).FullName;
            string enumFields = string.Join(',', names.Zip(values, _EnumFieldToString));

            throw new ArgumentException($"{message} - received enum {enumName} : {typeName} {{ {enumFields} }}");
        }
    }
}
