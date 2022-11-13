using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace AsyncKeyedLock
{
    internal sealed class AsyncKeyedLockerDictionary<TKey> : ConcurrentDictionary<TKey, AsyncKeyedLockReleaser<TKey>>
    {
        private readonly int _maxCount = 1;

        public AsyncKeyedLockerDictionary() : base()
        {
        }

        public AsyncKeyedLockerDictionary(int maxCount) : base()
        {
            _maxCount = maxCount;
        }

        public AsyncKeyedLockerDictionary(IEqualityComparer<TKey> comparer) : base(comparer)
        {
        }

        public AsyncKeyedLockerDictionary(int concurrencyLevel, int capacity) : base(concurrencyLevel, capacity)
        {
        }

        public AsyncKeyedLockerDictionary(int maxCount, int concurrencyLevel, int capacity) : base(concurrencyLevel, capacity)
        {
            _maxCount = maxCount;
        }

        public AsyncKeyedLockerDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer) : base(concurrencyLevel, capacity, comparer)
        {
        }

        public AsyncKeyedLockerDictionary(int maxCount, int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer) : base(concurrencyLevel, capacity, comparer)
        {
            _maxCount = maxCount;
        }

        public AsyncKeyedLockReleaser<TKey> GetOrAdd(TKey key)
        {
            if (TryGetValue(key, out var referenceCounter) && referenceCounter.TryIncrement())
            {
                return referenceCounter;
            }

            var toAddReferenceCounter = new AsyncKeyedLockReleaser<TKey>(key, new SemaphoreSlim(_maxCount), this);
            if (TryAdd(key, toAddReferenceCounter))
            {
                return toAddReferenceCounter;
            }

            while (!(TryGetValue(key, out referenceCounter) && referenceCounter.TryIncrement()))
            {
                if (TryAdd(key, toAddReferenceCounter))
                {
                    return toAddReferenceCounter;
                }
            }

            return referenceCounter;
        }

        public void Release(IAsyncKeyedLockReleaser<TKey> referenceCounter)
        {
            Monitor.Enter(referenceCounter);

            if (--referenceCounter.ReferenceCount == 0)
            {
                TryRemove(referenceCounter.Key, out _);
            }

            Monitor.Exit(referenceCounter);

            referenceCounter.SemaphoreSlim.Release();
        }
    }
}