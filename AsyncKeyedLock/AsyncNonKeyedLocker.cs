// Copyright (c) All contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncKeyedLock;

/// <summary>
/// Represents a lock, limiting concurrent threads to a specified number.
/// </summary>
/// <remarks>
/// The maximum number of requests for the semaphore that can be granted concurrently. Defaults to 1.
/// </remarks>
[System.Diagnostics.DebuggerDisplay("Current Count = {GetCurrentCount()}")]
public sealed class AsyncNonKeyedLocker(int maxCount = 1) : IDisposable
{

    /// <summary>
    /// The maximum number of requests for the semaphore that can be granted concurrently. Defaults to 1.
    /// </summary>
    public int MaxCount => maxCount;

    internal SemaphoreSlim _semaphoreSlim = new(maxCount, maxCount);

    #region Synchronous
    /// <summary>
    /// Synchronously lock.
    /// </summary>
    /// <returns>A disposable value of type <see cref="AsyncNonKeyedLockReleaser"/>.</returns>
    public AsyncNonKeyedLockReleaser Lock()
    {
        _semaphoreSlim.Wait();
        return new AsyncNonKeyedLockReleaser(this);
    }

    /// <summary>
    /// Synchronously lock, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A disposable value of type <see cref="AsyncNonKeyedLockReleaser"/>.</returns>
    public AsyncNonKeyedLockReleaser Lock(CancellationToken cancellationToken)
    {
        _semaphoreSlim.Wait(cancellationToken);
        return new AsyncNonKeyedLockReleaser(this);
    }

    /// <summary>
    /// Synchronously lock, setting a limit for the number of milliseconds to wait.
    /// </summary>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="entered">An out parameter showing whether or not the semaphore was entered.</param>
    /// <returns>A disposable value.</returns>
    [Obsolete("Use LockOrNull method instead as it is more performant.")]
    public IDisposable Lock(int millisecondsTimeout, out bool entered)
    {
        if (millisecondsTimeout == Timeout.Infinite)
        {
            entered = true;
            _semaphoreSlim.Wait();
            return new AsyncNonKeyedLockReleaser(this);
        }
        entered = _semaphoreSlim.Wait(millisecondsTimeout);
        return new AsyncNonKeyedLockTimeoutReleaser(this, entered);
    }

    /// <summary>
    /// Synchronously lock, setting a limit for the <see cref="TimeSpan"/> to wait.
    /// </summary>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="entered">An out parameter showing whether or not the semaphore was entered.</param>
    /// <returns>A disposable value.</returns>
    [Obsolete("Use LockOrNull method instead as it is more performant.")]
    public IDisposable Lock(TimeSpan timeout, out bool entered)
    {
        if (timeout.TotalMilliseconds == Timeout.Infinite)
        {
            entered = true;
            _semaphoreSlim.Wait();
            return new AsyncNonKeyedLockReleaser(this);
        }
        entered = _semaphoreSlim.Wait(timeout);
        return new AsyncNonKeyedLockTimeoutReleaser(this, entered);
    }

    /// <summary>
    /// Synchronously lock, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="entered">An out parameter showing whether or not the semaphore was entered.</param>
    /// <returns>A disposable value.</returns>
    [Obsolete("Use LockOrNull method instead as it is more performant.")]
    public IDisposable Lock(int millisecondsTimeout, CancellationToken cancellationToken, out bool entered)
    {
        try
        {
            if (millisecondsTimeout == Timeout.Infinite)
            {
                entered = true;
                _semaphoreSlim.Wait(cancellationToken);
                return new AsyncNonKeyedLockReleaser(this);
            }
            entered = _semaphoreSlim.Wait(millisecondsTimeout, cancellationToken);
            return new AsyncNonKeyedLockTimeoutReleaser(this, entered);
        }
        catch (OperationCanceledException)
        {
            entered = false;
            throw;
        }
    }

    /// <summary>
    /// Synchronously lock, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="entered">An out parameter showing whether or not the semaphore was entered.</param>
    /// <returns>A disposable value.</returns>
    [Obsolete("Use LockOrNull method instead as it is more performant.")]
    public IDisposable Lock(TimeSpan timeout, CancellationToken cancellationToken, out bool entered)
    {
        try
        {
            if (timeout.TotalMilliseconds == Timeout.Infinite)
            {
                entered = true;
                _semaphoreSlim.Wait(cancellationToken);
                return new AsyncNonKeyedLockReleaser(this);
            }
            entered = _semaphoreSlim.Wait(timeout, cancellationToken);
            return new AsyncNonKeyedLockTimeoutReleaser(this, entered);
        }
        catch (OperationCanceledException)
        {
            entered = false;
            throw;
        }
    }

    /// <summary>
    /// Synchronously lock, setting a limit for the number of milliseconds to wait.
    /// </summary>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public IDisposable? LockOrNull(int millisecondsTimeout)
    {
        if (_semaphoreSlim.Wait(millisecondsTimeout))
        {
            return new AsyncNonKeyedLockReleaser(this);
        }
        return null;
    }

    /// <summary>
    /// Synchronously lock, setting a limit for the <see cref="TimeSpan"/> to wait.
    /// </summary>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public IDisposable? LockOrNull(TimeSpan timeout)
    {
        if (_semaphoreSlim.Wait(timeout))
        {
            return new AsyncNonKeyedLockReleaser(this);
        }
        return null;
    }

    /// <summary>
    /// Synchronously lock, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public IDisposable? LockOrNull(int millisecondsTimeout, CancellationToken cancellationToken)
    {
        if (_semaphoreSlim.Wait(millisecondsTimeout, cancellationToken))
        {
            return new AsyncNonKeyedLockReleaser(this);
        }
        return null;
    }

    /// <summary>
    /// Synchronously lock, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public IDisposable? LockOrNull(TimeSpan timeout, CancellationToken cancellationToken)
    {
        if (_semaphoreSlim.Wait(timeout, cancellationToken))
        {
            return new AsyncNonKeyedLockReleaser(this);
        }
        return null;
    }
    #endregion Synchronous

    #region Asynchronous
    /// <summary>
    /// Asynchronously lock.
    /// </summary>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>A disposable value of type <see cref="AsyncNonKeyedLockReleaser"/>.</returns>
    public async ValueTask<AsyncNonKeyedLockReleaser> LockAsync(bool continueOnCapturedContext = false)
    {
        await _semaphoreSlim.WaitAsync().ConfigureAwait(continueOnCapturedContext);
        return new AsyncNonKeyedLockReleaser(this);
    }

    /// <summary>
    /// Asynchronously lock, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>A disposable value of type <see cref="AsyncNonKeyedLockReleaser"/>.</returns>
    public async ValueTask<AsyncNonKeyedLockReleaser> LockAsync(CancellationToken cancellationToken, bool continueOnCapturedContext = false)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext);
        return new AsyncNonKeyedLockReleaser(this);
    }

    /// <summary>
    /// Asynchronously lock, setting a limit for the number of milliseconds to wait.
    /// </summary>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>A disposable value of type <see cref="AsyncNonKeyedLockTimeoutReleaser"/>.</returns>
    [Obsolete("Use LockOrNullAsync method instead as it is more performant.")]
    public async ValueTask<AsyncNonKeyedLockTimeoutReleaser> LockAsync(int millisecondsTimeout, bool continueOnCapturedContext = false)
    {
        bool entered = await _semaphoreSlim.WaitAsync(millisecondsTimeout).ConfigureAwait(continueOnCapturedContext);
        return new AsyncNonKeyedLockTimeoutReleaser(this, entered);
    }

    /// <summary>
    /// Asynchronously lock, setting a limit for the <see cref="TimeSpan"/> to wait.
    /// </summary>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>A disposable value of type <see cref="AsyncNonKeyedLockTimeoutReleaser"/>.</returns>
    [Obsolete("Use LockOrNullAsync method instead as it is more performant.")]
    public async ValueTask<AsyncNonKeyedLockTimeoutReleaser> LockAsync(TimeSpan timeout, bool continueOnCapturedContext = false)
    {
        bool entered = await _semaphoreSlim.WaitAsync(timeout).ConfigureAwait(continueOnCapturedContext);
        return new AsyncNonKeyedLockTimeoutReleaser(this, entered);
    }

    /// <summary>
    /// Asynchronously lock, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>A disposable value of type <see cref="AsyncNonKeyedLockTimeoutReleaser"/>.</returns>
    [Obsolete("Use LockOrNullAsync method instead as it is more performant.")]
    public async ValueTask<AsyncNonKeyedLockTimeoutReleaser> LockAsync(int millisecondsTimeout, CancellationToken cancellationToken, bool continueOnCapturedContext = false)
    {
        try
        {
            bool entered = await _semaphoreSlim.WaitAsync(millisecondsTimeout, cancellationToken).ConfigureAwait(continueOnCapturedContext);
            return new AsyncNonKeyedLockTimeoutReleaser(this, entered);
        }
        catch (OperationCanceledException)
        {
            return new AsyncNonKeyedLockTimeoutReleaser(this, false);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously lock, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>A disposable value of type <see cref="AsyncNonKeyedLockTimeoutReleaser"/>.</returns>
    [Obsolete("Use LockOrNullAsync method instead as it is more performant.")]
    public async ValueTask<AsyncNonKeyedLockTimeoutReleaser> LockAsync(TimeSpan timeout, CancellationToken cancellationToken, bool continueOnCapturedContext = false)
    {
        try
        {
            bool entered = await _semaphoreSlim.WaitAsync(timeout, cancellationToken).ConfigureAwait(continueOnCapturedContext);
            return new AsyncNonKeyedLockTimeoutReleaser(this, entered);
        }
        catch (OperationCanceledException)
        {
            return new AsyncNonKeyedLockTimeoutReleaser(this, false);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously lock, setting a limit for the number of milliseconds to wait.
    /// </summary>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> LockOrNullAsync(int millisecondsTimeout, bool continueOnCapturedContext = false)
    {
        if (await _semaphoreSlim.WaitAsync(millisecondsTimeout).ConfigureAwait(continueOnCapturedContext))
        {
            return new AsyncNonKeyedLockReleaser(this);
        }
        return null;
    }

    /// <summary>
    /// Asynchronously lock, setting a limit for the <see cref="TimeSpan"/> to wait.
    /// </summary>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> LockOrNullAsync(TimeSpan timeout, bool continueOnCapturedContext = false)
    {
        if (await _semaphoreSlim.WaitAsync(timeout).ConfigureAwait(continueOnCapturedContext))
        {
            return new AsyncNonKeyedLockReleaser(this);
        }
        return null;
    }

    /// <summary>
    /// Asynchronously lock, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> LockOrNullAsync(int millisecondsTimeout, CancellationToken cancellationToken, bool continueOnCapturedContext = false)
    {
        if (await _semaphoreSlim.WaitAsync(millisecondsTimeout, cancellationToken).ConfigureAwait(continueOnCapturedContext))
        {
            return new AsyncNonKeyedLockReleaser(this);
        }
        return null;
    }

    /// <summary>
    /// Asynchronously lock, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> LockOrNullAsync(TimeSpan timeout, CancellationToken cancellationToken, bool continueOnCapturedContext = false)
    {
        if (await _semaphoreSlim.WaitAsync(timeout, cancellationToken).ConfigureAwait(continueOnCapturedContext))
        {
            return new AsyncNonKeyedLockReleaser(this);
        }
        return null;
    }
    #endregion Asynchronous

    #region AsynchronousNet8.0
#if NET8_0_OR_GREATER
#pragma warning disable CA1068 // CancellationToken parameters must come last
    /// <summary>
    /// Asynchronously lock.
    /// </summary>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>A disposable value of type <see cref="AsyncNonKeyedLockReleaser"/>.</returns>
    public async ValueTask<AsyncNonKeyedLockReleaser> LockAsync(ConfigureAwaitOptions configureAwaitOptions)
    {
        await _semaphoreSlim.WaitAsync().ConfigureAwait(configureAwaitOptions);
        return new AsyncNonKeyedLockReleaser(this);
    }

    /// <summary>
    /// Asynchronously lock, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>A disposable value of type <see cref="AsyncNonKeyedLockReleaser"/>.</returns>
    public async ValueTask<AsyncNonKeyedLockReleaser> LockAsync(CancellationToken cancellationToken, ConfigureAwaitOptions configureAwaitOptions)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(configureAwaitOptions);
        return new AsyncNonKeyedLockReleaser(this);
    }

    /// <summary>
    /// Asynchronously lock, setting a limit for the number of milliseconds to wait.
    /// </summary>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>A disposable value of type <see cref="AsyncNonKeyedLockTimeoutReleaser"/>.</returns>
    [Obsolete("Use LockOrNullAsync method instead as it is more performant.")]
    public async ValueTask<AsyncNonKeyedLockTimeoutReleaser> LockAsync(int millisecondsTimeout, ConfigureAwaitOptions configureAwaitOptions)
    {
        bool entered = await _semaphoreSlim.WaitAsync(millisecondsTimeout).ConfigureAwait(configureAwaitOptions);
        return new AsyncNonKeyedLockTimeoutReleaser(this, entered);
    }

    /// <summary>
    /// Asynchronously lock, setting a limit for the <see cref="TimeSpan"/> to wait.
    /// </summary>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>A disposable value of type <see cref="AsyncNonKeyedLockTimeoutReleaser"/>.</returns>
    [Obsolete("Use LockOrNullAsync method instead as it is more performant.")]
    public async ValueTask<AsyncNonKeyedLockTimeoutReleaser> LockAsync(TimeSpan timeout, ConfigureAwaitOptions configureAwaitOptions)
    {
        bool entered = await _semaphoreSlim.WaitAsync(timeout).ConfigureAwait(configureAwaitOptions);
        return new AsyncNonKeyedLockTimeoutReleaser(this, entered);
    }

    /// <summary>
    /// Asynchronously lock, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>A disposable value of type <see cref="AsyncNonKeyedLockTimeoutReleaser"/>.</returns>
    [Obsolete("Use LockOrNullAsync method instead as it is more performant.")]
    public async ValueTask<AsyncNonKeyedLockTimeoutReleaser> LockAsync(int millisecondsTimeout, CancellationToken cancellationToken, ConfigureAwaitOptions configureAwaitOptions)
    {
        try
        {
            bool entered = await _semaphoreSlim.WaitAsync(millisecondsTimeout, cancellationToken).ConfigureAwait(configureAwaitOptions);
            return new AsyncNonKeyedLockTimeoutReleaser(this, entered);
        }
        catch (OperationCanceledException)
        {
            return new AsyncNonKeyedLockTimeoutReleaser(this, false);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously lock, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>A disposable value of type <see cref="AsyncNonKeyedLockTimeoutReleaser"/>.</returns>
    [Obsolete("Use LockOrNullAsync method instead as it is more performant.")]
    public async ValueTask<AsyncNonKeyedLockTimeoutReleaser> LockAsync(TimeSpan timeout, CancellationToken cancellationToken, ConfigureAwaitOptions configureAwaitOptions)
    {
        try
        {
            bool entered = await _semaphoreSlim.WaitAsync(timeout, cancellationToken).ConfigureAwait(configureAwaitOptions);
            return new AsyncNonKeyedLockTimeoutReleaser(this, entered);
        }
        catch (OperationCanceledException)
        {
            return new AsyncNonKeyedLockTimeoutReleaser(this, false);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously lock, setting a limit for the number of milliseconds to wait.
    /// </summary>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> LockOrNullAsync(int millisecondsTimeout, ConfigureAwaitOptions configureAwaitOptions)
    {
        if (await _semaphoreSlim.WaitAsync(millisecondsTimeout).ConfigureAwait(configureAwaitOptions))
        {
            return new AsyncNonKeyedLockReleaser(this);
        }
        return null;
    }

    /// <summary>
    /// Asynchronously lock, setting a limit for the <see cref="TimeSpan"/> to wait.
    /// </summary>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> LockOrNullAsync(TimeSpan timeout, ConfigureAwaitOptions configureAwaitOptions)
    {
        if (await _semaphoreSlim.WaitAsync(timeout).ConfigureAwait(configureAwaitOptions))
        {
            return new AsyncNonKeyedLockReleaser(this);
        }
        return null;
    }

    /// <summary>
    /// Asynchronously lock, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> LockOrNullAsync(int millisecondsTimeout, CancellationToken cancellationToken, ConfigureAwaitOptions configureAwaitOptions)
    {
        if (await _semaphoreSlim.WaitAsync(millisecondsTimeout, cancellationToken).ConfigureAwait(configureAwaitOptions))
        {
            return new AsyncNonKeyedLockReleaser(this);
        }
        return null;
    }

    /// <summary>
    /// Asynchronously lock, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> LockOrNullAsync(TimeSpan timeout, CancellationToken cancellationToken, ConfigureAwaitOptions configureAwaitOptions)
    {
        if (await _semaphoreSlim.WaitAsync(timeout, cancellationToken).ConfigureAwait(configureAwaitOptions))
        {
            return new AsyncNonKeyedLockReleaser(this);
        }
        return null;
    }
#pragma warning restore CA1068 // CancellationToken parameters must come last
#endif
    #endregion AsynchronousNet8.0

    #region ConditionalSynchronous
    /// <summary>
    /// Synchronously lock. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public IDisposable? ConditionalLock(bool getLock)
    {
        if (getLock)
        {
            return Lock();
        }
        return null;
    }

    /// <summary>
    /// Synchronously lock, while observing a <see cref="CancellationToken"/>. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public IDisposable? ConditionalLock(bool getLock, CancellationToken cancellationToken)
    {
        if (getLock)
        {
            return Lock(cancellationToken);
        }
        return null;
    }

    /// <summary>
    /// Synchronously lock, setting a limit for the number of milliseconds to wait. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public IDisposable? ConditionalLock(bool getLock, int millisecondsTimeout)
    {
        if (getLock)
        {
            return LockOrNull(millisecondsTimeout);
        }
        return null;
    }

    /// <summary>
    /// Synchronously lock, setting a limit for the <see cref="TimeSpan"/> to wait. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public IDisposable? ConditionalLock(bool getLock, TimeSpan timeout)
    {
        if (getLock)
        {
            return LockOrNull(timeout);
        }
        return null;
    }

    /// <summary>
    /// Synchronously lock, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public IDisposable? ConditionalLock(bool getLock, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        if (getLock)
        {
            return LockOrNull(millisecondsTimeout, cancellationToken);
        }
        return null;
    }

    /// <summary>
    /// Synchronously lock, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public IDisposable? ConditionalLock(bool getLock, TimeSpan timeout, CancellationToken cancellationToken)
    {
        if (getLock)
        {
            return LockOrNull(timeout, cancellationToken);
        }
        return null;
    }
    #endregion ConditionalSynchronous

    #region ConditionalAsynchronous
    /// <summary>
    /// Asynchronously lock. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> ConditionalLockAsync(bool getLock, bool continueOnCapturedContext = false)
    {
        if (getLock)
        {
            return await LockAsync(continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
        }
        return null;
    }

    /// <summary>
    /// Asynchronously lock, while observing a <see cref="CancellationToken"/>. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> ConditionalLockAsync(bool getLock, CancellationToken cancellationToken, bool continueOnCapturedContext = false)
    {
        if (getLock)
        {
            return await LockAsync(cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
        }
        return null;
    }

    /// <summary>
    /// Asynchronously lock, setting a limit for the number of milliseconds to wait. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> ConditionalLockAsync(bool getLock, int millisecondsTimeout, bool continueOnCapturedContext = false)
    {
        if (getLock)
        {
            return await LockOrNullAsync(millisecondsTimeout, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
        }
        return null;
    }

    /// <summary>
    /// Asynchronously lock, setting a limit for the <see cref="TimeSpan"/> to wait. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> ConditionalLockAsync(bool getLock, TimeSpan timeout, bool continueOnCapturedContext = false)
    {
        if (getLock)
        {
            return await LockOrNullAsync(timeout, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
        }
        return null;
    }

    /// <summary>
    /// Asynchronously lock, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> ConditionalLockAsync(bool getLock, int millisecondsTimeout, CancellationToken cancellationToken, bool continueOnCapturedContext = false)
    {
        if (getLock)
        {
            return await LockOrNullAsync(millisecondsTimeout, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
        }
        return null;
    }

    /// <summary>
    /// Asynchronously lock, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> ConditionalLockAsync(bool getLock, TimeSpan timeout, CancellationToken cancellationToken, bool continueOnCapturedContext = false)
    {
        if (getLock)
        {
            return await LockOrNullAsync(timeout, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
        }
        return null;
    }
    #endregion ConditionalAsynchronous

    #region ConditionalAsynchronousNet8.0
#if NET8_0_OR_GREATER
#pragma warning disable CA1068 // CancellationToken parameters must come last
    /// <summary>
    /// Asynchronously lock. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> ConditionalLockAsync(bool getLock, ConfigureAwaitOptions configureAwaitOptions)
    {
        if (getLock)
        {
            await _semaphoreSlim.WaitAsync().ConfigureAwait(configureAwaitOptions);
            return new AsyncNonKeyedLockReleaser(this);
        }
        return null;
    }

    /// <summary>
    /// Asynchronously lock, while observing a <see cref="CancellationToken"/>. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> ConditionalLockAsync(bool getLock, CancellationToken cancellationToken, ConfigureAwaitOptions configureAwaitOptions)
    {
        if (getLock)
        {
            await _semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(configureAwaitOptions);
            return new AsyncNonKeyedLockReleaser(this);
        }
        return null;
    }

    /// <summary>
    /// Asynchronously lock, setting a limit for the number of milliseconds to wait. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> ConditionalLockAsync(bool getLock, int millisecondsTimeout, ConfigureAwaitOptions configureAwaitOptions)
    {
        if (getLock)
        {
            if (await _semaphoreSlim.WaitAsync(millisecondsTimeout).ConfigureAwait(configureAwaitOptions))
            {
                return new AsyncNonKeyedLockReleaser(this);
            }
            return null;
        }
        return null;
    }

    /// <summary>
    /// Asynchronously lock, setting a limit for the <see cref="TimeSpan"/> to wait. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> ConditionalLockAsync(bool getLock, TimeSpan timeout, ConfigureAwaitOptions configureAwaitOptions)
    {
        if (getLock)
        {
            if (await _semaphoreSlim.WaitAsync(timeout).ConfigureAwait(configureAwaitOptions))
            {
                return new AsyncNonKeyedLockReleaser(this);
            }
            return null;
        }
        return null;
    }

    /// <summary>
    /// Asynchronously lock, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> ConditionalLockAsync(bool getLock, int millisecondsTimeout, CancellationToken cancellationToken, ConfigureAwaitOptions configureAwaitOptions)
    {
        if (getLock)
        {
            if (await _semaphoreSlim.WaitAsync(millisecondsTimeout, cancellationToken).ConfigureAwait(configureAwaitOptions))
            {
                return new AsyncNonKeyedLockReleaser(this);
            }
            return null;
        }
        return null;
    }

    /// <summary>
    /// Asynchronously lock, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> ConditionalLockAsync(bool getLock, TimeSpan timeout, CancellationToken cancellationToken, ConfigureAwaitOptions configureAwaitOptions)
    {
        if (getLock)
        {
            if (await _semaphoreSlim.WaitAsync(timeout, cancellationToken).ConfigureAwait(configureAwaitOptions))
            {
                return new AsyncNonKeyedLockReleaser(this);
            }
            return null;
        }
        return null;
    }
#pragma warning restore CA1068 // CancellationToken parameters must come last
#endif
    #endregion ConditionalAsynchronousNet8.0

    /// <summary>
    /// Get the number of requests concurrently locked.
    /// </summary>
    /// <returns>The number of requests concurrently locked.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CA1024 // Use properties where appropriate
    public int GetRemainingCount() => maxCount - _semaphoreSlim.CurrentCount;
#pragma warning restore CA1024 // Use properties where appropriate

    /// <summary>
    /// Get the number of remaining threads that can enter the lock.
    /// </summary>
    /// <returns>The number of remaining threads that can enter the lock.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CA1024 // Use properties where appropriate
    public int GetCurrentCount() => _semaphoreSlim.CurrentCount;
#pragma warning restore CA1024 // Use properties where appropriate

    /// <summary>
    /// Disposes the AsyncNonKeyedLocker.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => _semaphoreSlim.Dispose();
}
