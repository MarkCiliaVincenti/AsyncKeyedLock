using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AsyncKeyedLock;

/// <summary>
/// Represents an <see cref="IDisposable"/> for AsyncNonKeyedLocker.
/// </summary>
public readonly struct AsyncNonKeyedLockReleaser : IDisposable
{
    private readonly AsyncNonKeyedLocker _locker;

    internal AsyncNonKeyedLockReleaser(AsyncNonKeyedLocker locker)
    {
        _locker = locker;
    }

    /// <summary>
    /// Releases the <see cref="SemaphoreSlim"/> object once.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Dispose()
    {
        _locker?._semaphoreSlim.Release();
    }
}