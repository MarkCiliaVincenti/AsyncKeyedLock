using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AsyncKeyedLock
{
    internal sealed class AsyncKeyedLockDictionary<TKey> : ConcurrentDictionary<TKey, AsyncKeyedLockReleaser<TKey>>
    {
        public int MaxCount { get; private set; } = 1;
        private readonly AsyncKeyedLockPool<TKey> _pool;
        private readonly bool _poolingEnabled;

        public AsyncKeyedLockDictionary() : base()
        {
        }

        public AsyncKeyedLockDictionary(AsyncKeyedLockOptions options) : base()
        {
            if (options.MaxCount < 1) throw new ArgumentOutOfRangeException(nameof(options), options.MaxCount, $"{nameof(options.MaxCount)} should be greater than or equal to 1.");

            MaxCount = options.MaxCount;
            if (options.PoolSize > 0)
            {
                _poolingEnabled = true;
                _pool = new AsyncKeyedLockPool<TKey>((key) => new AsyncKeyedLockReleaser<TKey>(key, new SemaphoreSlim(MaxCount, MaxCount), this), options.PoolSize);
            }
        }

        public AsyncKeyedLockDictionary(IEqualityComparer<TKey> comparer) : base(comparer)
        {
        }

        public AsyncKeyedLockDictionary(AsyncKeyedLockOptions options, IEqualityComparer<TKey> comparer) : base(comparer)
        {
            if (options.MaxCount < 1) throw new ArgumentOutOfRangeException(nameof(options), options.MaxCount, $"{nameof(options.MaxCount)} should be greater than or equal to 1.");

            MaxCount = options.MaxCount;
            if (options.PoolSize > 0)
            {
                _poolingEnabled = true;
                _pool = new AsyncKeyedLockPool<TKey>((key) => new AsyncKeyedLockReleaser<TKey>(key, new SemaphoreSlim(MaxCount, MaxCount), this), options.PoolSize);
            }
        }

        public AsyncKeyedLockDictionary(int concurrencyLevel, int capacity) : base(concurrencyLevel, capacity)
        {
        }

        public AsyncKeyedLockDictionary(AsyncKeyedLockOptions options, int concurrencyLevel, int capacity) : base(concurrencyLevel, capacity)
        {
            if (options.MaxCount < 1) throw new ArgumentOutOfRangeException(nameof(options), options.MaxCount, $"{nameof(options.MaxCount)} should be greater than or equal to 1.");

            MaxCount = options.MaxCount;
            if (options.PoolSize > 0)
            {
                _poolingEnabled = true;
                _pool = new AsyncKeyedLockPool<TKey>((key) => new AsyncKeyedLockReleaser<TKey>(key, new SemaphoreSlim(MaxCount, MaxCount), this), options.PoolSize);
            }
        }

        public AsyncKeyedLockDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer) : base(concurrencyLevel, capacity, comparer)
        {

        }

        public AsyncKeyedLockDictionary(AsyncKeyedLockOptions options, int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer) : base(concurrencyLevel, capacity, comparer)
        {
            if (options.MaxCount < 1) throw new ArgumentOutOfRangeException(nameof(options), options.MaxCount, $"{nameof(options.MaxCount)} should be greater than or equal to 1.");

            MaxCount = options.MaxCount;
            if (options.PoolSize > 0)
            {
                _poolingEnabled = true;
                _pool = new AsyncKeyedLockPool<TKey>((key) => new AsyncKeyedLockReleaser<TKey>(key, new SemaphoreSlim(MaxCount, MaxCount), this), options.PoolSize);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncKeyedLockReleaser<TKey> GetOrAdd(TKey key)
        {
            if (TryGetValue(key, out var releaser) && releaser.TryIncrement())
            {
                return releaser;
            }

            if (_poolingEnabled)
            {
                var releaserToAdd = _pool.GetObject(key);
                if (TryAdd(key, releaserToAdd))
                {
                    return releaserToAdd;
                }

                while (!TryGetValue(key, out releaser) || !releaser.TryIncrement())
                {
                    if (TryAdd(key, releaserToAdd))
                    {
                        return releaserToAdd;
                    }
                }

                _pool.PutObject(releaserToAdd);
                return releaser;
            }

            var releaserToAddNoPooling = new AsyncKeyedLockReleaser<TKey>(key, new SemaphoreSlim(MaxCount, MaxCount), this);
            if (TryAdd(key, releaserToAddNoPooling))
            {
                return releaserToAddNoPooling;
            }

            while (!TryGetValue(key, out releaser) || !releaser.TryIncrement())
            {
                if (TryAdd(key, releaserToAddNoPooling))
                {
                    return releaserToAddNoPooling;
                }
            }

            return releaser;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release(AsyncKeyedLockReleaser<TKey> releaser)
        {
            Monitor.Enter(releaser);

            if (--releaser.ReferenceCount == 0)
            {
                TryRemove(releaser.Key, out _);
                Monitor.Exit(releaser);
                if (_poolingEnabled)
                {
                    _pool.PutObject(releaser);
                }
                releaser.SemaphoreSlim.Release();
                return;
            }

            Monitor.Exit(releaser);
            releaser.SemaphoreSlim.Release();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReleaseWithoutSemaphoreRelease(AsyncKeyedLockReleaser<TKey> releaser)
        {
            Monitor.Enter(releaser);

            if (--releaser.ReferenceCount == 0)
            {
                TryRemove(releaser.Key, out _);
                Monitor.Exit(releaser);
                if (_poolingEnabled)
                {
                    _pool.PutObject(releaser);
                }
                return;
            }

            Monitor.Exit(releaser);
        }
    }
}
