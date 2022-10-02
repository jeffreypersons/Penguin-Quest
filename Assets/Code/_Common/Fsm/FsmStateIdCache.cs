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

        [Pure] private static int   ToInt(TEnum value) => UnsafeUtility.As<TEnum, int>(ref value);
        [Pure] private static TEnum ToEnum(int value) => UnsafeUtility.As<int, TEnum>(ref value);

        private readonly string[] _names;
        private readonly Type     _type;
        private readonly BitSet   _bitset;
        private readonly string   _description;
        private readonly Comparer<TEnum>         _valueComparer;
        private readonly EqualityComparer<TEnum> _equalityComparer;

        private FsmStateIdCache()
        {
            _names            = Enum.GetNames(typeof(TEnum));
            _type             = typeof(TEnum);
            _bitset           = new(_names.Length, true);
            _description      = $"{_type.FullName} {{ {string.Join(',', _names)} }}";

            // note that since enums are a value type, we can't use ==, so this is the best we can do (no boxing!) for id comparisons
            _equalityComparer = EqualityComparer<TEnum>.Default;
            _valueComparer    = Comparer<TEnum>.Default;

            ThrowIf(!AreAllEnumValuesDefault(), "Enum values must be int32 with default values from 0 to n");
        }

        public Type   Type   => _type;
        public int    Count  => _bitset.Count;
        public string String => _description;

        public Comparer<TEnum>         ValueComparer    => _valueComparer;
        public EqualityComparer<TEnum> EqualityComparer => _equalityComparer;

        public IEnumerable<(int index, string name, TEnum field)> Fields()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return (i, _names[i], UnsafeUtility.As<int, TEnum>(ref i));
            }
        }

        public bool TryGetIndex(in TEnum id, out int index)
        {
            if (!IsDefined(id))
            {
                index = -1;
                return false;
            }

            index = GetIndex(id);
            return true;
        }

        [Pure]
        public bool IsDefined(int index)
        {
            return _bitset.IsSet(index);
        }
        [Pure]
        public bool IsDefined(in TEnum id)
        {
            return _bitset.IsSet(ToInt(id));
        }

        [Pure]
        public int GetIndex(in TEnum id)
        {
            int index = ToInt(id);
            ThrowIf(!_bitset.IsSet(index), $"Cannot look up index since id {id} is not defined");
            return index;
        }

        [Pure]
        public string GetName(in TEnum id)
        {
            int index = ToInt(id);
            ThrowIf(!_bitset.IsSet(index), $"Cannot look up name since id {id} is not defined");
            return _names[index];
        }
        
        [Pure]
        public TEnum GetValue(int index)
        {
            ThrowIf(!_bitset.IsSet(index), $"Cannot look up id since index {index} is not defined");
            return ToEnum(index);
        }



        [Pure]
        private void ThrowIf(bool condition, string message)
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
        private bool AreAllEnumValuesDefault()
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
