using System;
using System.Linq;
using System.Diagnostics.Contracts;
using Unity.Collections.LowLevel.Unsafe;


namespace PQ.Common.Extensions
{
    /*
    Collection of utilities for working with generic enums.

    Unlike much of the Enum utilities provided in System.Enum, via generic constraints and unsafe conversions,
    we get boxing-free (eg no garbage) highly performant conversions and bit flag operations.


    Caution: For the sake of performance, all methods assume underlying type is the default int32.

    Note that there isn't any 'cross reference' between these API functions, to keep the call stack simpler.
    */
    public static class EnumExtensions
    {
        [Pure] public static int  AsInt<T>(T value)               where T : struct, Enum => UnsafeUtility.As<T, int>(ref value);
        [Pure] public static T    AsEnum<T>(int value)            where T : struct, Enum => UnsafeUtility.As<int, T>(ref value);
        [Pure] public static int  CountEnumValues<T>()            where T : struct, Enum => Enum.GetNames(typeof(T)).Length;
        [Pure] public static bool HasValue<T>(T value)            where T : struct, Enum => Enum.GetName(typeof(T), value) != null;
        [Pure] public static bool HasUnderlyingType<T>(Type type) where T : struct, Enum => Enum.GetUnderlyingType(typeof(T)) == type;


        /* Are all the enum values from 0 to n and of type int32? */
        [Pure]
        public static bool AreAllEnumValuesDefault<T>()
            where T : struct, Enum
        {
            Type enumType = typeof(T);
            if (Enum.GetUnderlyingType(enumType) != typeof(int))
            {
                return false;
            }

            int valueCount = Enum.GetNames(enumType).Length;
            for (int i = 0; i < valueCount; i++)
            {
                if (Enum.GetName(enumType, i) == null)
                {
                    return false;
                }
            }
            return true;
        }
        
        /* Performance warning - uses reflection and multiple array allocations. */
        [Pure]
        public static string AsUserFriendlyString<T>()
            where T : struct, Enum
        {
            [Pure] static string EnumFieldToString(string name, T value) =>
                $"{name}={UnsafeUtility.As<T, long>(ref value)}";

            string enumName   = typeof(T).FullName;
            string typeName   = Enum.GetUnderlyingType(typeof(T)).FullName;
            string enumFields = string.Join(',', Names<T>().Zip(Values<T>(), EnumFieldToString));

            return $"enum {enumName}:{typeName} {{{enumFields}}}";
        }


        [Pure] private static string[] Names<T>()  where T : struct, Enum =>      Enum.GetNames(typeof(T));
        [Pure] private static T[]      Values<T>() where T : struct, Enum => (T[])Enum.GetValues(typeof(T));
    }
}
