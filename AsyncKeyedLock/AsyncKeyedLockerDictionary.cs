using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

namespace AsyncKeyedLock
{
    internal class AsyncKeyedLockerDictionary<TKey> : ConcurrentDictionary<TKey, ReferenceCounter<TKey>>
    {
        public ReferenceCounter<TKey> GetOrAdd(TKey key, int maxCount)
        {
            while (true)
            {
                if (TryGetValue(key, out var referenceCounter) && Monitor.TryEnter(referenceCounter.SemaphoreSlim))
                {
                    ++referenceCounter.ReferenceCount;
                    Monitor.Exit(referenceCounter.SemaphoreSlim);
                    return referenceCounter;
                }

                referenceCounter = new ReferenceCounter<TKey>(key, new SemaphoreSlim(maxCount));                
                if (TryAdd(key, referenceCounter))
                {
                    return referenceCounter;
                }
            }
        }
    }
}
