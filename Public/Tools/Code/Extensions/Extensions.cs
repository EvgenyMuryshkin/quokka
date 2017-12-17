using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections.Concurrent;

namespace System.Collections.Generic
{
    public static class Extensions
    {
        public static T SingleOrDefault<T>(
            this IEnumerable<T> enumerable,
            string message)
        {
            try
            {
                return enumerable.SingleOrDefault();
            }
            catch(Exception ex)
            {
                throw new AggregateException($"{ex.Message}: {message}", ex);
            }
        }

        public static void ForEach<T>(
            this IEnumerable<T> enumerable, 
            Action<T> action)
        {
            foreach (var item in enumerable)
                action(item);
        }

        public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue>(
            this IEnumerable<TValue> enumerable,
            Func<TValue, TKey> keyConverter)
        {
            var result = new ConcurrentDictionary<TKey, TValue>();
            foreach (var item in enumerable)
            {
                result.AddOrUpdate(keyConverter(item), item, (k, v) => v);
            }

            return result;
        }

        public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue>(
            this Dictionary<TKey, TValue> enumerable)
        {
            var result = new ConcurrentDictionary<TKey, TValue>();

            foreach (var item in enumerable)
            {
                result.AddOrUpdate(item.Key, item.Value, (k, v) => v);
            }

            return result;
        }

        public static string ToCSV<T>(
            this List<T> list)
        {
            return ((IEnumerable<T>)list).ToCSV();
        }

        public static string ToCSV<T>(
            this IEnumerable<T> enumerable, 
            bool newLine = false)
        {
            return enumerable.Aggregate(
                new StringBuilder(),
                (a, s) =>
                {
                    if (0 != a.Length)
                        a.Append("," + (newLine ? "\r\n" : ""));
                    a.Append(s);
                    return a;
                }).ToString();
        }

        public static HashSet<TResult> ToHashSet<T, TResult>(
            this IEnumerable<T> enumerable, 
            Func<T, TResult> selector)
        {
            HashSet<TResult> set = new HashSet<TResult>();

            foreach (var item in enumerable)
                set.Add(selector(item));

            return set;
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return source.Aggregate(
                new HashSet<T>(),
                (h, s) => { h.Add(s); return h; }
                );
        }
    }
}
