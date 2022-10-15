using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Unity.Collections.LowLevel.Unsafe;


namespace PQ.Common.Containers
{
    /*
    Fixed array of enum values built on top of plain enum types, with ability to lookup index/values.
        
    Note that despite enums being defined at compile time and thus are fully static, we don't enforce singleton/static
    such that it's up to client code whether it's instanced in a thread-safe context or not.
    */
    internal class ExtendedEnum<T>
        where T : struct, Enum
    {
        private readonly int Size;
        public ExtendedEnum()
        {
            Size = Enum.GetNames(typeof(T)).Length;
        }

        public IEnumerable<T> Entries()
        {
            // todo: investigate just how much garbage this creates
            for (int i = 0; i < Size; i++)
            {
                yield return UnsafeUtility.As<int, T>(ref i);
            }
        }
        [Pure] public Type Type => typeof(T);

        [Pure] public long ValueAt(T field)     => UnsafeUtility.As<T, long>(ref field);
        [Pure] public int  IndexAt(T field)     => UnsafeUtility.As<T, int>(ref field);
        [Pure] public T    FieldAt(int index)   => UnsafeUtility.As<int, T>(ref index);

        [Pure] public bool IsDefined(int index) => index >= 0 && index < Size;
        [Pure] public bool IsDefined(T field)   => IndexAt(field) >= 0 && IndexAt(field) < Size;
        [Pure] public bool IsDefault(int index) => Enum.GetName(typeof(T), index) == null;
        [Pure] public bool IsDefault(T field)   => Enum.GetName(typeof(T), IndexAt(field)) == null;

        [Pure]
        public override string ToString()
        {
            [Pure] static string _EnumFieldToString(string name, T value) =>
                $"{name}={UnsafeUtility.As<T, long>(ref value)}";

            var type   = typeof(T);
            var names  = Enum.GetNames(type);
            var values = (T[])Enum.GetValues(type);

            string enumName   = type.FullName;
            string typeName   = Enum.GetUnderlyingType(type).FullName;
            string enumFields = string.Join(", ", names.Zip(values, _EnumFieldToString));

            return $"{enumName} : {typeName} {{ {enumFields} }}";
        }
    }
}
