// Copyright (c) All contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AsyncKeyedLock;

internal sealed class AtomicAsyncKeyedLockDictionary<TKey> : ConcurrentDictionary<TKey, SemaphoreSlim>, IDisposable where TKey : notnull
{
    internal readonly AtomicAsyncKeyedLockPool<TKey>? _pool;
    public bool PoolingEnabled { get; internal set; }

    public AtomicAsyncKeyedLockDictionary(AtomicAsyncKeyedLockOptions options) : base()
    {
        if (options.PoolSize > 0)
        {
            PoolingEnabled = true;
            _pool = new AtomicAsyncKeyedLockPool<TKey>(this, options.PoolSize, options.PoolInitialFill);
        }
    }

    public AtomicAsyncKeyedLockDictionary(AtomicAsyncKeyedLockOptions options, IEqualityComparer<TKey> comparer) : base(comparer)
    {
        if (options.PoolSize > 0)
        {
            PoolingEnabled = true;
            _pool = new AtomicAsyncKeyedLockPool<TKey>(this, options.PoolSize, options.PoolInitialFill);
        }
    }

    public AtomicAsyncKeyedLockDictionary(AtomicAsyncKeyedLockOptions options, int concurrencyLevel, int capacity) : base(concurrencyLevel, capacity)
    {
        if (options.PoolSize > 0)
        {
            PoolingEnabled = true;
            _pool = new AtomicAsyncKeyedLockPool<TKey>(this, options.PoolSize, options.PoolInitialFill);
        }
    }

    public AtomicAsyncKeyedLockDictionary(AtomicAsyncKeyedLockOptions options, int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer) : base(concurrencyLevel, capacity, comparer)
    {
        if (options.PoolSize > 0)
        {
            PoolingEnabled = true;
            _pool = new AtomicAsyncKeyedLockPool<TKey>(this, options.PoolSize, options.PoolInitialFill);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AtomicAsyncKeyedLockReleaser<TKey> GetOrAdd(TKey key)
    {
        if (PoolingEnabled)
        {
            if (TryGetValue(key, out var sem))
            {
                return new AtomicAsyncKeyedLockReleaser<TKey>(key, sem, false, this);
            }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var semToAdd = _pool.GetObject();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            if (TryAdd(key, semToAdd))
            {
                return new AtomicAsyncKeyedLockReleaser<TKey>(key, semToAdd, true, this);
            }

            sem = GetOrAdd(key, semToAdd);
            if (ReferenceEquals(sem, semToAdd))
            {
                return new AtomicAsyncKeyedLockReleaser<TKey>(key, semToAdd, true, this);
            }
            _pool.PutObject(semToAdd);
            return new AtomicAsyncKeyedLockReleaser<TKey>(key, sem, false, this);
        }

        if (TryGetValue(key, out var semNoPooling))
        {
            return new AtomicAsyncKeyedLockReleaser<TKey>(key, semNoPooling, false, this);
        }

        var semToAddNoPooling = new SemaphoreSlim(1, 1);
        if (TryAdd(key, semToAddNoPooling))
        {
            return new AtomicAsyncKeyedLockReleaser<TKey>(key, semToAddNoPooling, true, this);
        }

        semNoPooling = GetOrAdd(key, semToAddNoPooling);
        if (ReferenceEquals(semNoPooling, semToAddNoPooling))
        {
            return new AtomicAsyncKeyedLockReleaser<TKey>(key, semToAddNoPooling, true, this);
        }
        semToAddNoPooling.Dispose();
        return new AtomicAsyncKeyedLockReleaser<TKey>(key, semNoPooling, false, this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Release(AtomicAsyncKeyedLockReleaser<TKey> releaser)
    {
        if (PoolingEnabled)
        {
            if (releaser.IsOwner)
            {
                TryRemove(releaser.Key, out _);
                _pool!.PutObject(releaser.SemaphoreSlim);
            }
        }
        else
        {
            if (releaser.IsOwner)
            {
                TryRemove(releaser.Key, out _);
            }
        }
        releaser.SemaphoreSlim.Release();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReleaseWithoutSemaphoreRelease(AtomicAsyncKeyedLockReleaser<TKey> releaser)
    {
        if (PoolingEnabled)
        {
            if (releaser.IsOwner)
            {
                TryRemove(releaser.Key, out _);
                _pool!.PutObject(releaser.SemaphoreSlim);
            }
        }
        else
        {
            if (releaser.IsOwner)
            {
                TryRemove(releaser.Key, out _);
            }
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    public void Dispose()
    {
        foreach (var semaphore in Values)
        {
            try
            {
                semaphore?.Dispose();
            }
            catch { } // do nothing
        }
        Clear();
        if (PoolingEnabled)
        {
            try
            {
                _pool!.Dispose();
            }
            catch { } // do nothing
        }
    }
}
