using System;
using System.Collections;
using System.Collections.Generic;

namespace FPGA.Collections
{
    public class ReadOnlyDictionary<TKey,TValue> : IEnumerable
    {
        readonly Dictionary<TKey, TValue> _map;

        public ReadOnlyDictionary()
        {
            _map = new Dictionary<TKey, TValue>();
        }

        public void Add(TKey key, TValue value)
        {
            _map[key] = value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public TValue this[TKey key]
        {
            get
            {
                if (!_map.ContainsKey(key))
                {
                    return default(TValue);
                }

                return _map[key];
            }
        }
    }
}
