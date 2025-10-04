﻿using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AsyncKeyedLock;

/// <summary>
/// Represents an <see cref="IDisposable"/> for AsyncNonKeyedLocker with timeouts.
/// </summary>
public readonly struct AsyncNonKeyedLockTimeoutReleaser : IDisposable
{
    private readonly bool _enteredSemaphore;

    /// <summary>
    /// True if the timeout was reached, false if not.
    /// </summary>
    public readonly bool EnteredSemaphore => _enteredSemaphore;

    private readonly AsyncNonKeyedLocker _locker;

    internal AsyncNonKeyedLockTimeoutReleaser(AsyncNonKeyedLocker locker, bool enteredSemaphore)
    {
        _locker = locker;
        _enteredSemaphore = enteredSemaphore;
    }

    /// <summary>
    /// Releases the <see cref="SemaphoreSlim"/> object once, depending on whether or not the semaphore was entered.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Dispose()
    {
        if (_enteredSemaphore)
        {
            _locker._semaphoreSlim.Release();
        }            
    }
}