using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace AsyncKeyedLock
{
    internal sealed class AsyncKeyedLockerDictionary<TKey> : ConcurrentDictionary<TKey, AsyncKeyedLockReleaser<TKey>>
    {
        public int MaxCount { get; private set; } = 1;
        private readonly AsyncKeyedLockPool<TKey> _pool;
        private readonly bool _poolingEnabled;

        public AsyncKeyedLockerDictionary() : base()
        {
        }

        public AsyncKeyedLockerDictionary(AsyncKeyedLockOptions options) : base()
        {
            MaxCount = options.MaxCount;
            if (options.PoolSize > 0)
            {
                _poolingEnabled = true;
                _pool = new AsyncKeyedLockPool<TKey>((key) => new AsyncKeyedLockReleaser<TKey>(key, new SemaphoreSlim(MaxCount), this), options.PoolSize);
            }
        }

        public AsyncKeyedLockerDictionary(IEqualityComparer<TKey> comparer) : base(comparer)
        {
        }

        public AsyncKeyedLockerDictionary(AsyncKeyedLockOptions options, IEqualityComparer<TKey> comparer) : base(comparer)
        {
            MaxCount = options.MaxCount;
            if (options.PoolSize > 0)
            {
                _poolingEnabled = true;
                _pool = new AsyncKeyedLockPool<TKey>((key) => new AsyncKeyedLockReleaser<TKey>(key, new SemaphoreSlim(MaxCount), this), options.PoolSize);
            }
        }

        public AsyncKeyedLockerDictionary(int concurrencyLevel, int capacity) : base(concurrencyLevel, capacity)
        {
        }

        public AsyncKeyedLockerDictionary(AsyncKeyedLockOptions options, int concurrencyLevel, int capacity) : base(concurrencyLevel, capacity)
        {
            MaxCount = options.MaxCount;
            if (options.PoolSize > 0)
            {
                _poolingEnabled = true;
                _pool = new AsyncKeyedLockPool<TKey>((key) => new AsyncKeyedLockReleaser<TKey>(key, new SemaphoreSlim(MaxCount), this), options.PoolSize);
            }
        }

        public AsyncKeyedLockerDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer) : base(concurrencyLevel, capacity, comparer)
        {
        }

        public AsyncKeyedLockerDictionary(AsyncKeyedLockOptions options, int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer) : base(concurrencyLevel, capacity, comparer)
        {
            MaxCount = options.MaxCount;
            if (options.PoolSize > 0)
            {
                _poolingEnabled = true;
                _pool = new AsyncKeyedLockPool<TKey>((key) => new AsyncKeyedLockReleaser<TKey>(key, new SemaphoreSlim(MaxCount), this), options.PoolSize);
            }
        }

        private AsyncKeyedLockReleaser<TKey> GetReleaser(TKey key)
        {
            if (_poolingEnabled)
            {
                return _pool.GetObject(key);
            }
            return new AsyncKeyedLockReleaser<TKey>(key, new SemaphoreSlim(MaxCount), this);
        }

        private void AddToPool(AsyncKeyedLockReleaser<TKey> item)
        {
            if (_poolingEnabled)
            {
                _pool.PutObject((AsyncKeyedLockReleaser<TKey>)item);
            }
        }

        public AsyncKeyedLockReleaser<TKey> GetOrAdd(TKey key)
        {
            if (TryGetValue(key, out var referenceCounter) && referenceCounter.TryIncrement())
            {
                return referenceCounter;
            }

            var toAddReferenceCounter = GetReleaser(key);
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

            AddToPool(toAddReferenceCounter);
            return referenceCounter;
        }

        public void Release(AsyncKeyedLockReleaser<TKey> referenceCounter)
        {
            Monitor.Enter(referenceCounter);

            if (--referenceCounter.ReferenceCount == 0)
            {
                TryRemove(referenceCounter.Key, out _);
                Monitor.Exit(referenceCounter);
                AddToPool(referenceCounter);
                referenceCounter.SemaphoreSlim.Release();
                return;
            }

            Monitor.Exit(referenceCounter);
            referenceCounter.SemaphoreSlim.Release();
        }
    }
}