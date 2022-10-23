using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

namespace AsyncKeyedLock
{
    /// <summary>
    /// AsyncKeyedLockerDictionary class
    /// </summary>
    /// <typeparam name="TKey">The type for the dictionary key</typeparam>
    public sealed class AsyncKeyedLockerDictionary<TKey> : ConcurrentDictionary<TKey, AsyncKeyedLockReferenceCounter<TKey>>
    {
        private readonly int _maxCount;

        /// <summary>
        /// Constructor for AsyncKeyedLockerDictionary
        /// </summary>
        /// <param name="maxCount">The number of semaphore counts to allow.</param>
        public AsyncKeyedLockerDictionary(int maxCount)
        {
            _maxCount = maxCount;
        }

        /// <summary>
        /// Provider for AsyncKeyedLockReferenceCounter
        /// </summary>
        /// <param name="key">The key for which a reference counter should be obtained.</param>
        /// <returns>A created or retrieved reference counter</returns>
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

        /// <summary>
        /// Dispose and release.
        /// </summary>
        /// <param name="referenceCounter">The reference counter instance.</param>
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
