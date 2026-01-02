// Copyright (c) All contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
#if NET5_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif
using System.Threading;

namespace AsyncKeyedLock;

/// <summary>
/// Represents an <see cref="IDisposable"/> for AsyncKeyedLock.
/// </summary>
#if NET5_0_OR_GREATER
[SkipLocalsInit]
#endif
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
