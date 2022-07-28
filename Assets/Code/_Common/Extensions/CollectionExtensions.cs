using System;
using System.Collections.Generic;
using UnityEngine;


namespace PQ.Common.Extensions
{
    public static class CollectionExtensions
    {
        public static T[] PrependToArray<T>(T value, T[] source)
        {
            T[] newArray = new T[source.Length + 1];
            newArray[0] = value;
            Array.Copy(source, 0, newArray, 1, source.Length);
            return newArray;
        }

        /*
        Return true if all elements of given arrays are matching.

        Assumes comparator `Equals` is defined for given element type.
        */
        public static bool AreArraysEqual<T>(T[] array1, T[] array2)
        {
            if (ReferenceEquals(array1, array2))
            {
                return true;
            }
            if (array1 == null || array2 == null || array1.Length != array2.Length)
            {
                return false;
            }

            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < array1.Length; i++)
            {
                if (!comparer.Equals(array1[i], array2[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /*
        Are elements of given array segments array1[s1,] and array2[s2,] matching (in length and value)?

        Assumes comparator `Equals` is defined for given element type, and start <= array.length - 1.
        */
        public static bool AreArraySegmentsEqual<T>(T[] array1, T[] array2, int start1, int start2)
        {
            if (ReferenceEquals(array1, array2) && start1 == start2 && array1.Length == array2.Length)
            {
                return true;
            }
            if (array1 == null || array2 == null)
            {
                return false;
            }

            int count1 = array1.Length - start1;
            int count2 = array2.Length - start2;
            if (count1 != count2)
            {
                return false;
            }

            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < count1; i++)
            {
                if (!comparer.Equals(array1[i + start1], array2[i + start2]))
                {
                    return false;
                }
            }
            return true;
        }

        /*
        Are elements of given array segments array1[s1,s1+count1] and array2[s2,s2+count2] matching (in length and value)?

        Assumes comparator `Equals` is defined for given element type, and start <= array.length - 1.
        Note that if start+count exceed array.length, then the segment spans to that point.
        */
        public static bool AreArraySegmentsEqual<T>(T[] array1, T[] array2, int start1, int start2, int count1, int count2)
        {
            if (ReferenceEquals(array1, array2) && start1 == start2 && count1 == count2)
            {
                return true;
            }

            if (array1 == null || array2 == null)
            {
                return false;
            }
            int segmentLength1 = Mathf.Min(count1, array1.Length - start1);
            int segmentLength2 = Mathf.Min(count2, array2.Length - start2);
            if (segmentLength1 != segmentLength2)
            {
                return false;
            }

            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < segmentLength1; i++)
            {
                if (!comparer.Equals(array1[i + start1], array2[i + start2]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
