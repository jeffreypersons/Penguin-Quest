using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Unity.Collections.LowLevel.Unsafe;


namespace PQ.Common.Containers
{
    /*
    Simple sequence of ordered enum sets with functionality for looking up names, indices, and comparisons.


    That is, this functionality is intended to extend the generic enum param when used as a flat, ordered sequence.

    Despite enums being defined at compile time and thus are fully static, this class does not enforce singleton access
    to give clients the option, in the case that it is used in a multi-threaded context.


    Properties
    - constant time 'contains' check
    - generic (no boxing!) enum comparisons via comparer (relevant since == cannot be used with generic enum types)
    - upfront validation of enum constraints (that the values follow the pattern of 0,1,2,....,n-1,n)
    */
    public class OrderedEnumSet<TEnum>
        where TEnum : struct, Enum
    {
        private readonly Type     _type;
        private readonly string[] _names;
        private readonly int      _count;

        public OrderedEnumSet()
        {
            _type  = typeof(TEnum);
            _names = ExtractNames<TEnum>();
            _count = _names.Length;
        }

        public override string ToString() => $"{{{string.Join(',', _names)}}}";
        public Type Type  => _type;
        public int  Count => _count;

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
            if (index < 0 || index > _count)
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
            return index >= 0 && index < _count;
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
