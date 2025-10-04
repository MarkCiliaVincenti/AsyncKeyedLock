using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AsyncKeyedLock;

/// <summary>
/// Represents an <see cref="IDisposable"/> for AsyncKeyedLock with timeouts.
/// </summary>
public sealed class StripedAsyncKeyedLockTimeoutReleaser : IDisposable
{
    /// <summary>
    /// True if the timeout was reached, false if not.
    /// </summary>
    public bool EnteredSemaphore { get; internal set; }
    internal readonly StripedAsyncKeyedLockReleaser _releaser;

    internal StripedAsyncKeyedLockTimeoutReleaser(bool enteredSemaphore, StripedAsyncKeyedLockReleaser releaser)
    {
        EnteredSemaphore = enteredSemaphore;
        _releaser = releaser;
    }

    /// <summary>
    /// Releases the <see cref="SemaphoreSlim"/> object once, depending on whether or not the semaphore was entered.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (EnteredSemaphore)
        {
            _releaser.Dispose();
        }
    }
}