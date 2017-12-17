using System;
using System.Collections.Generic;
using System.Linq;

namespace Quokka.Public.Tools
{
    public class SmartDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public SmartDictionary(string hint)
        {
            Hint = string.IsNullOrWhiteSpace(hint) ? "" : hint + " ";
        }

        public SmartDictionary(IDictionary<TKey, TValue> source) : base(source)
        {

        }

        public string Hint { get; set; }

        public new TValue this[TKey key]
        {
            get
            {
                if (!ContainsKey(key))
                {
                    throw new KeyNotFoundException((Hint ?? "") + key.ToString());
                }

                return base[key];
            }
            set
            {
                base[key] = value;
            }
        }
    }

    public static class SmartDictionaryExtensions
    {
        public static SmartDictionary<TKey, TValue> ToSmartDictionary<TSource, TKey, TValue>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TValue> valueSelector
            )
        {
            return new SmartDictionary<TKey, TValue>(source.ToDictionary(keySelector, valueSelector));
        }

        public static SmartDictionary<TKey, TValue> ToSmartDictionary<TKey, TValue>(
            this IEnumerable<TValue> source,
            Func<TValue, TKey> keySelector
            )
        {
            return new SmartDictionary<TKey, TValue>(source.ToDictionary(keySelector));
        }
    }
}
