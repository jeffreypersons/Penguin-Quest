using System;
using System.Diagnostics.Contracts;
using Unity.Collections.LowLevel.Unsafe;


namespace PQ.Common.Extensions
{
    /*
    Collection of utilities for working with generic enums.

    Unlike much of the Enum utilities provided in System.Enum, via generic constraints and unsafe conversions,
    we get boxing-free (eg no garbage) highly performant conversions and bit flag operations.

    Caution: For the sake of performance, all methods assume underlying type is the default int32, and that
             system.flags is used as an attribute, with default value of none
    */
    public static class EnumExtensions
    {
        [Pure] public static bool IsDefined<T>(T value) where T : struct, Enum => Enum.IsDefined(typeof(T), value);


        [Pure] public static int AsInt<T>(T value)    where T : struct, Enum => UnsafeUtility.As<T, int>(ref value);

        [Pure] public static T   AsEnum<T>(int value) where T : struct, Enum => UnsafeUtility.As<int, T>(ref value);


        [Pure] public static string NameOf<T>()                 where T : struct, Enum => nameof(T);

        [Pure] public static string NameOf<T>(T value)          where T : struct, Enum => nameof(value);

        [Pure] public static string FullNameOfValue<T>(T value) where T : struct, Enum => $"{nameof(T)}.{nameof(value)}";


        [Pure] public static T SetFlags<T>(T set, T subset)   where T : struct, Enum => AsEnum<T>(AsInt(set) | AsInt(subset));
        [Pure] public static T ClearFlags<T>(T set, T subset) where T : struct, Enum => AsEnum<T>(AsInt(set) & AsInt(subset));


        // check if ALL given flags are a proper subset of constraints
        // note that unlike enum.hasFlags, this returns false for None = 0
        [Pure]
        public static bool HasFlags<T>(T set, T subset)
            where T : struct, Enum
        {
            var set_    = AsInt(set);
            var subset_ = AsInt(subset);
            return (set_ & subset_) == subset_;
        }

        // OR all given flags together
        [Pure]
        public static T CombineFlags<T>(in T[] subsets)
            where T : struct, Enum
        {
            int bits = 0;
            for (int i = 0; i < subsets.Length; i++)
            {
                bits |= AsInt(subsets[i]);
            }
            return AsEnum<T>(bits);
        }
    }
}
