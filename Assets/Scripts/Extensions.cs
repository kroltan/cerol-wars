using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kroltan.GrapplingHook
{
    public static class Extensions
    {
        public static Vector3 Average(this IEnumerable<Vector3> self)
        {
            var total = Vector3.zero;
            var count = 0;

            foreach (var item in self)
            {
                total += item;
                count++;
            }

            return total / count;
        }

        public static Vector3 Average<T>(this IEnumerable<T> self, Func<T, Vector3> selector)
        {
            var total = Vector3.zero;
            var count = 0;

            foreach (var item in self)
            {
                total += selector(item);
                count++;
            }

            return total / count;
        }

        public static TItem MaxBy<TItem, TKey>(this IEnumerable<TItem> self, Func<TItem, TKey> key)
            where TKey : IComparable<TKey>
        {
            using var enumerator = self.GetEnumerator();

            if (!enumerator.MoveNext())
            {
                return default;
            }
            
            var currentValue = enumerator.Current;
            var currentMaximum = key(currentValue);

            while (enumerator.MoveNext())
            {
                if (key(enumerator.Current).CompareTo(currentMaximum) > 0)
                {
                    currentValue = enumerator.Current;
                }
            }

            return currentValue;
        }


        public static TItem MinBy<TItem, TKey>(this IEnumerable<TItem> self, Func<TItem, TKey> key)
            where TKey : IComparable<TKey>
        {
            using var enumerator = self.GetEnumerator();

            if (!enumerator.MoveNext())
            {
                return default;
            }

            var currentValue = enumerator.Current;
            var currentMaximum = key(currentValue);

            while (enumerator.MoveNext())
            {
                if (key(enumerator.Current).CompareTo(currentMaximum) < 0)
                {
                    currentValue = enumerator.Current;
                }
            }

            return currentValue;
        }

        public static IEnumerable<ArraySegment<T>> Chunks<T>(this T[] self, int size)
        {
            var end = self.Length - size;
            for (var i = 0; i < end; i += size)
            {
                yield return new(self, i, size);
            }
        }

        public static IEnumerable<T> NonNull<T>(this IEnumerable<T?> self)
            where T: struct
        {
            foreach (var item in self)
            {
                if (item == null)
                {
                    continue;
                }

                yield return item.Value;
            }
        }

        public static LineSegment ToSegment(this Ray ray, float length)
        {
            return new(ray.origin, ray.GetPoint(length));
        }

        public static IEnumerable<(int, T)> WithIndex<T>(this IEnumerable<T> self)
        {
            var i = 0;
            foreach (var item in self)
            {
                yield return (i, item);
                ++i;
            }
        }
    }
}