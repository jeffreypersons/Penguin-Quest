using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Unity.Collections.LowLevel.Unsafe;


namespace PQ.Common.Containers
{
    /*
    Utility that extends enum such there's a streamlined interface for accessing meta data about individual enum fields.

    Note that despite enums being defined at compile time and thus are fully static, we don't enforce singleton/static
    such that it's up to client code whether it's instanced in a thread-safe context or not.
    */
    public class EnumMetadata<T>
        where T : struct, Enum
    {
        private readonly int  _size;
        private readonly Type _type;
        private readonly Type _underlyingType;
        private string[] Names  => Enum.GetNames(typeof(T));
        private T[]      Values => (T[])Enum.GetValues(typeof(T));

        public EnumMetadata()
        {
            _size = Names.Length;
            _type = typeof(T);
            _underlyingType = Enum.GetUnderlyingType(typeof(T));
        }

        public IEnumerable<T> Entries()
        {
            // todo: replace with cached `public IEnumerator GetEnumerator()` implementation
            for (int i = 0; i < Size; i++)
            {
                yield return UnsafeUtility.As<int, T>(ref i);
            }
        }

        [Pure] public int  Size           => _size;
        [Pure] public Type Type           => _type;
        [Pure] public Type UnderlyingType => _underlyingType;

        [Pure] public long ValueOf(T field)     => UnsafeUtility.As<T, long>(ref field);
        [Pure] public T    FieldAt(int index)   => UnsafeUtility.As<int, T>(ref index);
        [Pure] public bool IsDefault(int index) => Enum.GetName(_type, index) == null;
        [Pure] public bool IsDefault(T field)   => Enum.GetName(_type, (int)ValueOf(field)) == null;

        [Pure]
        public override string ToString()
        {
            string name   = _type.Name;
            string type   = _underlyingType.Name;
            string fields = string.Join(", ", Names.Zip(Values, (k, v) => $"{k}={ValueOf(v)}"));

            return $"{name} : {type} {{ {fields} }}";
        }
    }
}
