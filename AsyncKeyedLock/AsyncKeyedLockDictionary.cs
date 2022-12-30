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
        internal bool PoolingEnabled { get; private set; }

        public AsyncKeyedLockDictionary() : base()
        {
        }

        public AsyncKeyedLockDictionary(AsyncKeyedLockOptions options) : base()
        {
            if (options.MaxCount < 1) throw new ArgumentOutOfRangeException(nameof(options), options.MaxCount, $"{nameof(options.MaxCount)} should be greater than or equal to 1.");

            MaxCount = options.MaxCount;
            if (options.PoolSize > 0)
            {
                PoolingEnabled = true;
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
                PoolingEnabled = true;
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
                PoolingEnabled = true;
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
                PoolingEnabled = true;
                _pool = new AsyncKeyedLockPool<TKey>((key) => new AsyncKeyedLockReleaser<TKey>(key, new SemaphoreSlim(MaxCount, MaxCount), this), options.PoolSize);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncKeyedLockReleaser<TKey> GetOrAdd(TKey key)
        {
            if (PoolingEnabled)
            {
                if (TryGetValue(key, out var releaser) && releaser.TryIncrement())
                {
                    return releaser;
                }

                var releaserToAdd = _pool.GetObject(key);
                if (TryAdd(key, releaserToAdd))
                {
                    if (releaserToAdd.IsPooled)
                    {
                        releaserToAdd.Key = key;
                        releaserToAdd.IsPooled = false;
                    }
                    return releaserToAdd;
                }

                releaser = GetOrAdd(key, releaserToAdd);
                if (ReferenceEquals(releaser, releaserToAdd))
                {
                    return releaser;
                }

                while (true)
                {
                    releaser = GetOrAdd(key, releaserToAdd);
                    if (ReferenceEquals(releaser, releaserToAdd))
                    {
                        if (releaserToAdd.IsPooled)
                        {
                            releaserToAdd.Key = key;
                            releaserToAdd.IsPooled = false;
                        }
                        return releaser;
                    }
                    if (releaser.TryIncrement())
                    {
                        _pool.PutObject(releaserToAdd);
                        return releaser;
                    }
                }
            }

            if (TryGetValue(key, out var releaserNoPooling) && releaserNoPooling.TryIncrementNoPooling())
            {
                return releaserNoPooling;
            }

            var releaserToAddNoPooling = new AsyncKeyedLockReleaser<TKey>(key, new SemaphoreSlim(MaxCount, MaxCount), this);
            if (TryAdd(key, releaserToAddNoPooling))
            {
                return releaserToAddNoPooling;
            }

            while (true)
            {
                releaserNoPooling = GetOrAdd(key, releaserToAddNoPooling);
                if (ReferenceEquals(releaserNoPooling, releaserToAddNoPooling) || releaserNoPooling.TryIncrementNoPooling())
                {
                    return releaserNoPooling;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release(AsyncKeyedLockReleaser<TKey> releaser)
        {
            Monitor.Enter(releaser);

            if (--releaser.ReferenceCount == 0)
            {
                TryRemove(releaser.Key, out _);
                if (PoolingEnabled)
                {
                    releaser.IsPooled = true;
                    Monitor.Exit(releaser);
                    releaser.ReferenceCount = 1;
                    _pool.PutObject(releaser);
                }
                else
                {
                    Monitor.Exit(releaser);
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
                if (PoolingEnabled)
                {
                    releaser.IsPooled = true;
                    Monitor.Exit(releaser);
                    releaser.ReferenceCount = 1;
                    _pool.PutObject(releaser);
                }
                else
                {
                    Monitor.Exit(releaser);
                }
                return;
            }

            Monitor.Exit(releaser);
        }
    }
}
