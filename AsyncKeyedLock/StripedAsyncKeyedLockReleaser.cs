// Copyright (c) All contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;

namespace AsyncKeyedLock;

/// <summary>
/// Represents an <see cref="IDisposable"/> for AsyncKeyedLock.
/// </summary>
public readonly struct StripedAsyncKeyedLockReleaser : IDisposable
{
    internal StripedAsyncKeyedLockReleaser(SemaphoreSlim semaphoreSlim)
    {
        SemaphoreSlim = semaphoreSlim;
    }

    /// <summary>
    /// The exposed <see cref="System.Threading.SemaphoreSlim"/> instance used to limit the number of threads that can access the lock concurrently.
    /// </summary>
    public readonly SemaphoreSlim SemaphoreSlim { get; }

    /// <summary>
    /// Releases the <see cref="System.Threading.SemaphoreSlim"/> object once.
    /// </summary>
    public void Dispose() => SemaphoreSlim.Release();
}
