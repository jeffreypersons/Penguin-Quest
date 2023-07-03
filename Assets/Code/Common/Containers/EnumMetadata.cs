using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Unity.Collections.LowLevel.Unsafe;


//
// *** todo: look into serializable enum name support ***
// - add a wrapper for individual values with serialization/editor custom attribute built in, such that it's name is serialized
// - this would allow new enum members to be added anywhere within their definition _without_ the current side effect
//   of all currently stored enum fields in the editor becoming incorrect due to change in enum member's implicit integer
//   value, since that index is what Unity serializes by default
//
//
namespace PQ.Common.Containers
{
    /*
    Utility that extends enum such there's a streamlined interface for accessing meta data about individual enum fields.

    Note that despite enums being defined at compile time and thus are fully static, we don't enforce singleton/static
    such that it's up to client code whether it's instanced in a thread-safe context or not.
    */
    public sealed class EnumMetadata<TEnum>
        where TEnum : struct, Enum
    {
        private readonly int      _size;
        private readonly Type     _type;
        private readonly Type     _underlyingType;
        private readonly string[] _names;
        private readonly TEnum[]  _values;

        public EnumMetadata()
        {
            var enumType = typeof(TEnum);
            var enumNames = Enum.GetNames(enumType);
            _size           = enumNames.Length;
            _type           = enumType;
            _underlyingType = Enum.GetUnderlyingType(enumType);
            _names          = enumNames;
            _values         = (TEnum[])Enum.GetValues(enumType);
        }

        [Pure] public int  Size => _size;
        [Pure] public Type Type => _type;
        [Pure] public Type UnderlyingType => _underlyingType;
        [Pure] public IReadOnlyList<string> Names  => _names;
        [Pure] public IReadOnlyList<TEnum>  Fields => _values;

        [Pure] public U     FieldToValue<U>(TEnum field) where U : struct => UnsafeUtility.As<TEnum, U>(ref field);
        [Pure] public TEnum ValueToField<U>(U value)     where U : struct => UnsafeUtility.As<U, TEnum>(ref value);
        [Pure] public bool  IsValueDefined<U>(U value)   where U : struct => Enum.GetName(_type, value) != null;
        [Pure] public bool  IsFieldDefined(TEnum field) => Enum.GetName(_type, UnsafeUtility.As<TEnum, long>(ref field)) != null;

        public override string ToString()
        {
            var fields = _names.Zip(_values, (k, v) => $"{k}={UnsafeUtility.As<TEnum, long>(ref v)}");

            return $"enum {_type} : {_underlyingType} {{ {string.Join(", ", fields)} }}";
        }
    }
}
