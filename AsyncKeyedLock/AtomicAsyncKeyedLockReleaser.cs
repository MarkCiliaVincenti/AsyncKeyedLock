// Copyright (c) All contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AsyncKeyedLock;

/// <summary>
/// Represents an <see cref="IDisposable"/> for AsyncKeyedLock.
/// </summary>
#if NET5_0_OR_GREATER
[SkipLocalsInit]
#endif
public readonly struct AtomicAsyncKeyedLockReleaser<TKey> : IDisposable where TKey : notnull
{
    internal readonly bool IsOwner;

    internal readonly TKey Key;

    private readonly SemaphoreSlim _semaphoreSlim;

    /// <summary>
    /// The exposed <see cref="SemaphoreSlim"/> instance used to limit the number of threads that can access the lock concurrently.
    /// </summary>
    public SemaphoreSlim SemaphoreSlim => _semaphoreSlim;

    private readonly AtomicAsyncKeyedLockDictionary<TKey> _dictionary;

    internal AtomicAsyncKeyedLockReleaser(TKey key, SemaphoreSlim semaphoreSlim, bool isOwner, AtomicAsyncKeyedLockDictionary<TKey> dictionary)
    {
        Key = key;
        _semaphoreSlim = semaphoreSlim;
        IsOwner = isOwner;
        _dictionary = dictionary;
    }

    /// <summary>
    /// Releases the <see cref="SemaphoreSlim"/> object once.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        _dictionary.Release(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Dispose(bool enteredSemaphore)
    {
        if (enteredSemaphore)
        {
            _dictionary.Release(this);
        }
        else
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(this);
        }
    }
}
