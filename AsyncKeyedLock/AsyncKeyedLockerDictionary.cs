using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

namespace AsyncKeyedLock
{
    internal sealed class AsyncKeyedLockerDictionary<TKey> : ConcurrentDictionary<TKey, AsyncKeyedLockReferenceCounter<TKey>>
    {
        private readonly int _maxCount;
        public AsyncKeyedLockerDictionary(int maxCount)
        {
            _maxCount = maxCount;
        }

        public AsyncKeyedLockReferenceCounter<TKey> GetOrAdd(TKey key)
        {
            if (TryGetValue(key, out var firstReferenceCounter) && Monitor.TryEnter(firstReferenceCounter))
            {
                ++firstReferenceCounter.ReferenceCount;
                Monitor.Exit(firstReferenceCounter);
                return firstReferenceCounter;
            }

            firstReferenceCounter = new AsyncKeyedLockReferenceCounter<TKey>(key, new SemaphoreSlim(_maxCount), this);
            if (TryAdd(key, firstReferenceCounter))
            {
                return firstReferenceCounter;
            }

            while (true)
            {
                if (TryGetValue(key, out var referenceCounter) && Monitor.TryEnter(referenceCounter))
                {
                    ++referenceCounter.ReferenceCount;
                    Monitor.Exit(referenceCounter);
                    return referenceCounter;
                }

                if (TryAdd(key, firstReferenceCounter))
                {
                    return firstReferenceCounter;
                }
            }
        }

        public void Release(AsyncKeyedLockReferenceCounter<TKey> referenceCounter)
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
