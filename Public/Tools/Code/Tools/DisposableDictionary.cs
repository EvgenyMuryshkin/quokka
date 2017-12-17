using System;

namespace Quokka.Public.Tools
{
    public class DisposableDictionary<TKey, TValue> : SmartDictionary<TKey, TValue>, IDisposable where TValue : IDisposable
    {
        public DisposableDictionary() : base("Disposable not found:") { }

        public void Dispose()
        {
            foreach(var p in this)
            {
                p.Value.Dispose();
            }

            Clear();
        }
    }
}
