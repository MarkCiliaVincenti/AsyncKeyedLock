using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AsyncKeyedLock;

/// <summary>
/// Represents an <see cref="IDisposable"/> for AsyncKeyedLock with timeouts.
/// </summary>
public sealed class AsyncKeyedLockTimeoutReleaser<TKey> : IDisposable where TKey : notnull
{
    /// <summary>
    /// True if the timeout was reached, false if not.
    /// </summary>
    public bool EnteredSemaphore { get; internal set; }
    internal readonly AsyncKeyedLockReleaser<TKey> _releaser;

    /// <summary>
    /// Creates a releaser that only disposes the <see cref="SemaphoreSlim"/> if enteredSemaphore is true.
    /// </summary>
    /// <param name="enteredSemaphore">If set to true, will dispose the <see cref="SemaphoreSlim"/>.</param>
    /// <param name="releaser">The <see cref="AsyncKeyedLockReleaser{TKey}"/> releaser.</param>
    public AsyncKeyedLockTimeoutReleaser(bool enteredSemaphore, AsyncKeyedLockReleaser<TKey> releaser)
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
        _releaser.Dispose(EnteredSemaphore);
    }
}