using System;
using System.Diagnostics.Contracts;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;


namespace PQ.Common.Extensions
{
    /*
    Collection of utilities for working with generic enums.

    Unlike much of the Enum utilities provided in System.Enum, via generic constraints and unsafe conversions,
    we get boxing-free (eg no garbage) highly performant conversions and bit flag operations.

    Caution: For the sake of performance, all methods assume underlying type is the default int32
    */
    public static class EnumExtensions
    {
        [Pure] public static int  AsInt<T>(T val)                 where T : struct, Enum => UnsafeUtility.As<T, int>(ref val);
        [Pure] public static T    AsEnum<T>(int val)              where T : struct, Enum => UnsafeUtility.As<int, T>(ref val);
        [Pure] public static int  CountEnumValues<T>()            where T : struct, Enum => Enum.GetNames(typeof(T)).Length;
        [Pure] public static bool HasValue<T>(T value)            where T : struct, Enum => Enum.GetName(typeof(T), value) != null;
        [Pure] public static bool HasUnderlyingType<T>(Type type) where T : struct, Enum => Enum.GetUnderlyingType(typeof(T)) == type;

        /* Are all the enum values from 0 to n of type int32? */
        [Pure]
        public static bool AreAllEnumValuesDefault<TEnum>()
            where TEnum : struct, Enum
        {
            if (!HasUnderlyingType<TEnum>(typeof(int)))
            {
                return false;
            }

            int totalCount = CountEnumValues<TEnum>();
            for (int i = 0; i < totalCount; i++)
            {
                if (!HasValue(AsEnum<TEnum>(i)))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
