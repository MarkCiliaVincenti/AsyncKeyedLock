using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AsyncKeyedLock
{
    internal sealed class AsyncKeyedLockDictionary<TKey> : ConcurrentDictionary<TKey, AsyncKeyedLockReleaser<TKey>>, IDisposable where TKey : notnull
    {
        public int MaxCount { get; private set; } = 1;
        internal readonly AsyncKeyedLockPool<TKey>? _pool;
        public bool PoolingEnabled { get; internal set; }

        public AsyncKeyedLockDictionary(AsyncKeyedLockOptions options) : base()
        {
            if (options.MaxCount < 1) throw new ArgumentOutOfRangeException(nameof(options), options.MaxCount, $"{nameof(options.MaxCount)} should be greater than or equal to 1.");

            MaxCount = options.MaxCount;
            if (options.PoolSize > 0)
            {
                PoolingEnabled = true;
                _pool = new AsyncKeyedLockPool<TKey>(this, options.PoolSize, options.PoolInitialFill);
            }
        }

        public AsyncKeyedLockDictionary(AsyncKeyedLockOptions options, IEqualityComparer<TKey> comparer) : base(comparer)
        {
            if (options.MaxCount < 1) throw new ArgumentOutOfRangeException(nameof(options), options.MaxCount, $"{nameof(options.MaxCount)} should be greater than or equal to 1.");

            MaxCount = options.MaxCount;
            if (options.PoolSize > 0)
            {
                PoolingEnabled = true;
                _pool = new AsyncKeyedLockPool<TKey>(this, options.PoolSize, options.PoolInitialFill);
            }
        }

        public AsyncKeyedLockDictionary(AsyncKeyedLockOptions options, int concurrencyLevel, int capacity) : base(concurrencyLevel, capacity)
        {
            if (options.MaxCount < 1) throw new ArgumentOutOfRangeException(nameof(options), options.MaxCount, $"{nameof(options.MaxCount)} should be greater than or equal to 1.");

            MaxCount = options.MaxCount;
            if (options.PoolSize > 0)
            {
                PoolingEnabled = true;
                _pool = new AsyncKeyedLockPool<TKey>(this, options.PoolSize, options.PoolInitialFill);
            }
        }

        public AsyncKeyedLockDictionary(AsyncKeyedLockOptions options, int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer) : base(concurrencyLevel, capacity, comparer)
        {
            if (options.MaxCount < 1) throw new ArgumentOutOfRangeException(nameof(options), options.MaxCount, $"{nameof(options.MaxCount)} should be greater than or equal to 1.");

            MaxCount = options.MaxCount;
            if (options.PoolSize > 0)
            {
                PoolingEnabled = true;
                _pool = new AsyncKeyedLockPool<TKey>(this, options.PoolSize, options.PoolInitialFill);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncKeyedLockReleaser<TKey> GetOrAdd(TKey key)
        {
            if (PoolingEnabled)
            {
                if (TryGetValue(key, out var releaser) && releaser.TryIncrement(key))
                {
                    return releaser;
                }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var releaserToAdd = _pool.GetObject(key);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                if (TryAdd(key, releaserToAdd))
                {
                    return releaserToAdd;
                }

                while (true)
                {
                    releaser = GetOrAdd(key, releaserToAdd);
                    if (ReferenceEquals(releaser, releaserToAdd))
                    {
                        return releaser;
                    }
                    if (releaser.TryIncrement(key))
                    {
                        releaserToAdd.IsNotInUse = true;
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
            if (PoolingEnabled)
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#if NET9_0_OR_GREATER
                releaser.Lock.Enter();
#else
                Monitor.Enter(releaser);
#endif

                if (releaser.ReferenceCount == 1)
                {
                    TryRemove(releaser.Key, out _);
                    releaser.IsNotInUse = true;
#if NET9_0_OR_GREATER
                    releaser.Lock.Exit();
#else
                    Monitor.Exit(releaser);
#endif
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    _pool.PutObject(releaser);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    releaser.SemaphoreSlim.Release();
                    return;
                }

                --releaser.ReferenceCount;
#if NET9_0_OR_GREATER
                releaser.Lock.Exit();
#else
                Monitor.Exit(releaser);
#endif
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
            else
            {
                Monitor.Enter(releaser);

                if (releaser.ReferenceCount == 1)
                {
                    TryRemove(releaser.Key, out _);
                    releaser.IsNotInUse = true;
                    Monitor.Exit(releaser);
                    releaser.SemaphoreSlim.Release();
                    return;
                }

                --releaser.ReferenceCount;
                Monitor.Exit(releaser);
            }
            releaser.SemaphoreSlim.Release();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReleaseWithoutSemaphoreRelease(AsyncKeyedLockReleaser<TKey> releaser)
        {
            if (PoolingEnabled)
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#if NET9_0_OR_GREATER
                releaser.Lock.Enter();
#else
                Monitor.Enter(releaser);
#endif

                if (releaser.ReferenceCount == 1)
                {
                    TryRemove(releaser.Key, out _);
                    releaser.IsNotInUse = true;
#if NET9_0_OR_GREATER
                    releaser.Lock.Exit();
#else
                    Monitor.Exit(releaser);
#endif
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    _pool.PutObject(releaser);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    return;
                }
                --releaser.ReferenceCount;
#if NET9_0_OR_GREATER
                releaser.Lock.Exit();
#else
                Monitor.Exit(releaser);
#endif
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
            else
            {
                Monitor.Enter(releaser);

                if (releaser.ReferenceCount == 1)
                {
                    TryRemove(releaser.Key, out _);
                    releaser.IsNotInUse = true;
                    Monitor.Exit(releaser);
                    return;
                }
                --releaser.ReferenceCount;
                Monitor.Exit(releaser);
            }
        }

        public void Dispose()
        {
            foreach (var semaphore in Values)
            {
                try
                {
                    semaphore.Dispose();
                }
                catch
                {
                    // do nothing
                }
            }
            Clear();
            if (PoolingEnabled)
            {
                try
                {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    _pool.Dispose();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                }
                catch
                {
                    // do nothing
                }
            }
        }
    }
}