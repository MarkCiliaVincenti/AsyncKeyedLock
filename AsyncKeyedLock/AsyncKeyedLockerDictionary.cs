using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

namespace AsyncKeyedLock
{
    internal sealed class AsyncKeyedLockerDictionary<TKey> : ConcurrentDictionary<TKey, ReferenceCounter<TKey>>
    {
        public ReferenceCounter<TKey> GetOrAdd(TKey key, int maxCount)
        {
            while (true)
            {
                if (TryGetValue(key, out var referenceCounter) && Monitor.TryEnter(referenceCounter))
                {
                    ++referenceCounter.ReferenceCount;
                    Monitor.Exit(referenceCounter);
                    return referenceCounter;
                }

                referenceCounter = new ReferenceCounter<TKey>(key, new SemaphoreSlim(maxCount));                
                if (TryAdd(key, referenceCounter))
                {
                    return referenceCounter;
                }
            }
        }

        public void Release(ReferenceCounter<TKey> referenceCounter)
        {
            while (!Monitor.TryEnter(referenceCounter)) { }

            var remainingConsumers = --referenceCounter.ReferenceCount;

            if (remainingConsumers == 0)
            {
                TryRemove(referenceCounter.Key, out _);
            }

            Monitor.Exit(referenceCounter);

            referenceCounter.SemaphoreSlim.Release();
        }
    }
}
