using System;
using System.Diagnostics.Contracts;
using Unity.Collections.LowLevel.Unsafe;


namespace PQ.Common.Extensions
{
    /*
    Collection of utilities for working with generic enums.

    Unlike much of the Enum utilities provided in System.Enum, via generic constraints and unsafe conversions,
    we get boxing-free (eg no garbage) highly performant conversions and bit flag operations.
    */
    public static class EnumExtensions
    {
        [Pure] public static bool IsDefined<T>(T value) where T : struct, Enum => Enum.IsDefined(typeof(T), value);


        [Pure] public static int AsInt<T>(T value)    where T : struct, Enum => UnsafeUtility.As<T, int>(ref value);

        [Pure] public static T   AsEnum<T>(int value) where T : struct, Enum => UnsafeUtility.As<int, T>(ref value);


        [Pure] public static string NameOf<T>()                 where T : struct, Enum => nameof(T);

        [Pure] public static string NameOf<T>(T value)          where T : struct, Enum => nameof(value);

        [Pure] public static string FullNameOfValue<T>(T value) where T : struct, Enum => $"{nameof(T)}.{nameof(value)}";

        
        // check if ALL given flags are a proper subset of constraints
        // note that unlike enum.hasFlags, this returns false for None = 0
        [Pure]
        public static bool HasAllFlags<T>(T constraints, T flags)
            where T : struct, Enum
        {
            var constraints_ = AsInt(constraints);
            var flags_       = AsInt(flags);
            return (constraints_ & flags_) == flags_;
        }
    }
}
