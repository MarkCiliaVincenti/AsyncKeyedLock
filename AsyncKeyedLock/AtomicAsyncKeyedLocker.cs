// Copyright (c) All contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncKeyedLock;

/// <summary>
/// Represents a thread-safe keyed locker that allows you to lock based on a key (keyed semaphores), only allowing a specified number of concurrent threads that share the same key.
/// </summary>
public sealed class AtomicAsyncKeyedLocker<TKey> : IDisposable where TKey : notnull
{
    private bool _disposed;

    internal readonly AtomicAsyncKeyedLockDictionary<TKey> _dictionary;

    /// <summary>
    /// Read-only index of objects held by <see cref="AtomicAsyncKeyedLocker{TKey}"/>.
    /// </summary>
    public IReadOnlyDictionary<TKey, SemaphoreSlim> Index => _dictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomicAsyncKeyedLocker{TKey}" /> class, has the default concurrency level, has the default initial capacity, and uses the default comparer for the key type.
    /// </summary>
    public AtomicAsyncKeyedLocker()
    {
        _dictionary = new AtomicAsyncKeyedLockDictionary<TKey>(AtomicAsyncKeyedLockOptions.Default);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomicAsyncKeyedLocker{TKey}" /> class, uses the specified <see cref="AtomicAsyncKeyedLockOptions"/>, has the default concurrency level, has the default initial capacity, and uses the default comparer for the key type.
    /// </summary>
    /// <param name="options">The <see cref="AtomicAsyncKeyedLockOptions"/> to use.</param>
    /// <exception cref="ArgumentOutOfRangeException">Parameter is out of range.</exception>
    public AtomicAsyncKeyedLocker(AtomicAsyncKeyedLockOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _dictionary = new AtomicAsyncKeyedLockDictionary<TKey>(options);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomicAsyncKeyedLocker{TKey}" /> class, uses the specified <see cref="AtomicAsyncKeyedLockOptions"/>, has the default concurrency level, has the default initial capacity, and uses the default comparer for the key type.
    /// </summary>
    /// <param name="options">The <see cref="AtomicAsyncKeyedLockOptions"/> to use.</param>
    /// <exception cref="ArgumentOutOfRangeException">Parameter is out of range.</exception>
    public AtomicAsyncKeyedLocker(Action<AtomicAsyncKeyedLockOptions> options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var optionsParam = new AtomicAsyncKeyedLockOptions();
        options(optionsParam);

        _dictionary = new AtomicAsyncKeyedLockDictionary<TKey>(optionsParam);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomicAsyncKeyedLocker{TKey}" /> class, has the default concurrency level, has the default initial capacity, and uses the specified <see cref="IEqualityComparer{TKey}"/>.
    /// </summary>
    /// <param name="comparer">The equality comparison implementation to use when comparing keys.</param>
    /// <exception cref="ArgumentNullException"><paramref name="comparer"/> is null.</exception>
    public AtomicAsyncKeyedLocker(IEqualityComparer<TKey> comparer)
    {
        _dictionary = new AtomicAsyncKeyedLockDictionary<TKey>(AtomicAsyncKeyedLockOptions.Default, comparer);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomicAsyncKeyedLocker{TKey}" /> class, uses the specified <see cref="AtomicAsyncKeyedLockOptions"/>, has the default concurrency level, has the default initial capacity, and uses the specified <see cref="IEqualityComparer{TKey}"/>.
    /// </summary>
    /// <param name="options">The <see cref="AtomicAsyncKeyedLockOptions"/> to use.</param>
    /// <param name="comparer">The equality comparison implementation to use when comparing keys.</param>
    /// <exception cref="ArgumentOutOfRangeException">Parameter is out of range.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="comparer"/> is null.</exception>
    public AtomicAsyncKeyedLocker(AtomicAsyncKeyedLockOptions options, IEqualityComparer<TKey> comparer)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _dictionary = new AtomicAsyncKeyedLockDictionary<TKey>(options, comparer);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomicAsyncKeyedLocker{TKey}" /> class, uses the specified <see cref="AtomicAsyncKeyedLockOptions"/>, has the default concurrency level, has the default initial capacity, and uses the specified <see cref="IEqualityComparer{TKey}"/>.
    /// </summary>
    /// <param name="options">The <see cref="AtomicAsyncKeyedLockOptions"/> to use.</param>
    /// <param name="comparer">The equality comparison implementation to use when comparing keys.</param>
    /// <exception cref="ArgumentOutOfRangeException">Parameter is out of range.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="comparer"/> is null.</exception>
    public AtomicAsyncKeyedLocker(Action<AtomicAsyncKeyedLockOptions> options, IEqualityComparer<TKey> comparer)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var optionsParam = new AtomicAsyncKeyedLockOptions();
        options(optionsParam);

        _dictionary = new AtomicAsyncKeyedLockDictionary<TKey>(optionsParam, comparer);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomicAsyncKeyedLocker{TKey}" /> class, has the specified concurrency level and capacity, and uses the default comparer for the key type.
    /// </summary>
    /// <param name="concurrencyLevel">The estimated number of threads that will update the <see cref="AtomicAsyncKeyedLocker{TKey}"/> concurrently.</param>
    /// <param name="capacity">The initial number of elements that the <see cref="AtomicAsyncKeyedLocker{TKey}"/> can contain.</param>
    /// <exception cref="ArgumentOutOfRangeException">Parameter is out of range.</exception>
    public AtomicAsyncKeyedLocker(int concurrencyLevel, int capacity)
    {
        _dictionary = new AtomicAsyncKeyedLockDictionary<TKey>(AtomicAsyncKeyedLockOptions.Default, concurrencyLevel, capacity);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomicAsyncKeyedLocker{TKey}" /> class, uses the specified <see cref="AtomicAsyncKeyedLockOptions"/>, has the specified concurrency level and capacity, and uses the default comparer for the key type.
    /// </summary>
    /// <param name="options">The <see cref="AtomicAsyncKeyedLockOptions"/> to use.</param>
    /// <param name="concurrencyLevel">The estimated number of threads that will update the <see cref="AtomicAsyncKeyedLocker{TKey}"/> concurrently.</param>
    /// <param name="capacity">The initial number of elements that the <see cref="AtomicAsyncKeyedLocker{TKey}"/> can contain.</param>
    /// <exception cref="ArgumentOutOfRangeException">Parameter is out of range.</exception>
    public AtomicAsyncKeyedLocker(AtomicAsyncKeyedLockOptions options, int concurrencyLevel, int capacity)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _dictionary = new AtomicAsyncKeyedLockDictionary<TKey>(options, concurrencyLevel, capacity);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomicAsyncKeyedLocker{TKey}" /> class, uses the specified <see cref="AtomicAsyncKeyedLockOptions"/>, has the specified concurrency level and capacity, and uses the default comparer for the key type.
    /// </summary>
    /// <param name="options">The <see cref="AtomicAsyncKeyedLockOptions"/> to use.</param>
    /// <param name="concurrencyLevel">The estimated number of threads that will update the <see cref="AtomicAsyncKeyedLocker{TKey}"/> concurrently.</param>
    /// <param name="capacity">The initial number of elements that the <see cref="AtomicAsyncKeyedLocker{TKey}"/> can contain.</param>
    /// <exception cref="ArgumentOutOfRangeException">Parameter is out of range.</exception>
    public AtomicAsyncKeyedLocker(Action<AtomicAsyncKeyedLockOptions> options, int concurrencyLevel, int capacity)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var optionsParam = new AtomicAsyncKeyedLockOptions();
        options(optionsParam);

        _dictionary = new AtomicAsyncKeyedLockDictionary<TKey>(optionsParam, concurrencyLevel, capacity);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomicAsyncKeyedLocker{TKey}" /> class, uses the specified <see cref="SemaphoreSlim"/> initial count, has the specified concurrency level and capacity, and uses the default comparer for the key type.
    /// </summary>
    /// <param name="concurrencyLevel">The estimated number of threads that will update the <see cref="AtomicAsyncKeyedLocker{TKey}"/> concurrently.</param>
    /// <param name="capacity">The initial number of elements that the <see cref="AtomicAsyncKeyedLocker{TKey}"/> can contain.</param>
    /// <param name="comparer">The equality comparison implementation to use when comparing keys.</param>
    /// <exception cref="ArgumentOutOfRangeException">Parameter is out of range.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="comparer"/> is null.</exception>
    public AtomicAsyncKeyedLocker(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer)
    {
        _dictionary = new AtomicAsyncKeyedLockDictionary<TKey>(AtomicAsyncKeyedLockOptions.Default, concurrencyLevel, capacity, comparer);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomicAsyncKeyedLocker{TKey}" /> class, uses the specified <see cref="AtomicAsyncKeyedLockOptions"/>, has the specified concurrency level and capacity, and uses the default comparer for the key type.
    /// </summary>
    /// <param name="options">The <see cref="AtomicAsyncKeyedLockOptions"/> to use.</param>
    /// <param name="concurrencyLevel">The estimated number of threads that will update the <see cref="AtomicAsyncKeyedLocker{TKey}"/> concurrently.</param>
    /// <param name="capacity">The initial number of elements that the <see cref="AtomicAsyncKeyedLocker{TKey}"/> can contain.</param>
    /// <param name="comparer">The equality comparison implementation to use when comparing keys.</param>
    /// <exception cref="ArgumentOutOfRangeException">Parameter is out of range.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="comparer"/> is null.</exception>
    public AtomicAsyncKeyedLocker(AtomicAsyncKeyedLockOptions options, int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _dictionary = new AtomicAsyncKeyedLockDictionary<TKey>(options, concurrencyLevel, capacity, comparer);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomicAsyncKeyedLocker{TKey}" /> class, uses the specified <see cref="AtomicAsyncKeyedLockOptions"/>, has the specified concurrency level and capacity, and uses the default comparer for the key type.
    /// </summary>
    /// <param name="options">The <see cref="AtomicAsyncKeyedLockOptions"/> to use.</param>
    /// <param name="concurrencyLevel">The estimated number of threads that will update the <see cref="AtomicAsyncKeyedLocker{TKey}"/> concurrently.</param>
    /// <param name="capacity">The initial number of elements that the <see cref="AtomicAsyncKeyedLocker{TKey}"/> can contain.</param>
    /// <param name="comparer">The equality comparison implementation to use when comparing keys.</param>
    /// <exception cref="ArgumentOutOfRangeException">Parameter is out of range.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="comparer"/> is null.</exception>
    public AtomicAsyncKeyedLocker(Action<AtomicAsyncKeyedLockOptions> options, int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var optionsParam = new AtomicAsyncKeyedLockOptions();
        options(optionsParam);

        _dictionary = new AtomicAsyncKeyedLockDictionary<TKey>(optionsParam, concurrencyLevel, capacity, comparer);
    }

    /// <summary>
    /// Provider for <see cref="AtomicAsyncKeyedLockReleaser{TKey}"/>
    /// </summary>
    /// <param name="key">The key for which a releaser should be obtained.</param>
    /// <returns>A created or retrieved <see cref="AtomicAsyncKeyedLockReleaser{TKey}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AtomicAsyncKeyedLockReleaser<TKey> GetOrAdd(TKey key) => _dictionary.GetOrAdd(key);

    #region Synchronous
    /// <summary>
    /// Synchronously lock based on a key.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <returns>A disposable value.</returns>
    public IDisposable Lock(TKey key)
    {
        var releaser = GetOrAdd(key);
        releaser.SemaphoreSlim.Wait();
        return releaser;
    }

    /// <summary>
    /// Synchronously lock based on a key, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A disposable value.</returns>
    public IDisposable Lock(TKey key, CancellationToken cancellationToken)
    {
        var releaser = GetOrAdd(key);
        try
        {
            releaser.SemaphoreSlim.Wait(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            throw;
        }
        return releaser;
    }

    /// <summary>
    /// Synchronously lock based on a key, setting a limit for the number of milliseconds to wait.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public IDisposable? LockOrNull(TKey key, int millisecondsTimeout)
    {
        var releaser = GetOrAdd(key);
        if (releaser.SemaphoreSlim.Wait(millisecondsTimeout))
        {
            return releaser;
        }
        _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
        return null;
    }

    /// <summary>
    /// Synchronously lock based on a key, setting a limit for the <see cref="TimeSpan"/> to wait.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public IDisposable? LockOrNull(TKey key, TimeSpan timeout)
    {
        var releaser = GetOrAdd(key);
        if (releaser.SemaphoreSlim.Wait(timeout))
        {
            return releaser;
        }
        _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
        return null;
    }

    /// <summary>
    /// Synchronously lock based on a key, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public IDisposable? LockOrNull(TKey key, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        var releaser = GetOrAdd(key);
        try
        {
            if (releaser.SemaphoreSlim.Wait(millisecondsTimeout, cancellationToken))
            {
                return releaser;
            }
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            return null;
        }
        catch (OperationCanceledException)
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            throw;
        }
    }

    /// <summary>
    /// Synchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public IDisposable? LockOrNull(TKey key, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var releaser = GetOrAdd(key);
        try
        {
            if (releaser.SemaphoreSlim.Wait(timeout, cancellationToken))
            {
                return releaser;
            }
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            return null;
        }
        catch (OperationCanceledException)
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            throw;
        }
    }
    #endregion Synchronous

    #region SynchronousTry
    /// <summary>
    /// Synchronously lock based on a key, setting a limit for the number of milliseconds to wait, and if not timed out, scynchronously execute an action and release.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="action">The synchronous action.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <returns>False if timed out, true if it successfully entered.</returns>
    public bool TryLock(TKey key, Action action, int millisecondsTimeout)
    {
        var releaser = GetOrAdd(key);
        if (!releaser.SemaphoreSlim.Wait(millisecondsTimeout))
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            return false;
        }

        try
        {
            if (action is not null)
            {
                action();
            }
        }
        finally
        {
            _dictionary.Release(releaser);
        }
        return true;
    }

    /// <summary>
    /// Synchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, and if not timed out, scynchronously execute an action and release.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="action">The synchronous action.</param>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <returns>False if timed out, true if it successfully entered.</returns>
    public bool TryLock(TKey key, Action action, TimeSpan timeout)
    {
        var releaser = GetOrAdd(key);
        if (!releaser.SemaphoreSlim.Wait(timeout))
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            return false;
        }

        try
        {
            if (action is not null)
            {
                action();
            }
        }
        finally
        {
            _dictionary.Release(releaser);
        }
        return true;
    }

    /// <summary>
    /// Synchronously lock based on a key, setting a limit for the number of milliseconds to wait, and if not timed out, scynchronously execute an action and release, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="action">The synchronous action.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>False if timed out, true if it successfully entered.</returns>
    public bool TryLock(TKey key, Action action, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        var releaser = GetOrAdd(key);
        try
        {
            if (!releaser.SemaphoreSlim.Wait(millisecondsTimeout, cancellationToken))
            {
                _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
                return false;
            }
        }
        catch (OperationCanceledException)
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            throw;
        }

        try
        {
            if (action is not null)
            {
                action();
            }
        }
        finally
        {
            _dictionary.Release(releaser);
        }
        return true;
    }

    /// <summary>
    /// Synchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, and if not timed out, scynchronously execute an action and release, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="action">The synchronous action.</param>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>False if timed out, true if it successfully entered.</returns>
    public bool TryLock(TKey key, Action action, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var releaser = GetOrAdd(key);
        try
        {
            if (!releaser.SemaphoreSlim.Wait(timeout, cancellationToken))
            {
                _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
                return false;
            }
        }
        catch (OperationCanceledException)
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            throw;
        }

        try
        {
            if (action is not null)
            {
                action();
            }
        }
        finally
        {
            _dictionary.Release(releaser);
        }
        return true;
    }
    #endregion SynchronousTry

    #region AsynchronousTry
    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, and if not timed out, scynchronously execute an action and release.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="action">The synchronous action.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>False if timed out, true if it successfully entered.</returns>
    public async ValueTask<bool> TryLockAsync(TKey key, Action action, int millisecondsTimeout, bool continueOnCapturedContext = false)
    {
        var releaser = GetOrAdd(key);
        if (!await releaser.SemaphoreSlim.WaitAsync(millisecondsTimeout).ConfigureAwait(continueOnCapturedContext))
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            return false;
        }

        try
        {
            if (action is not null)
            {
                action();
            }
        }
        finally
        {
            _dictionary.Release(releaser);
        }
        return true;
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, and if not timed out, ascynchronously execute a <see cref="Func{Task}"/> and release.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="task">The asynchronous task.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>False if timed out, true if it successfully entered.</returns>
    public async ValueTask<bool> TryLockAsync(TKey key, Func<Task> task, int millisecondsTimeout, bool continueOnCapturedContext = false)
    {
        var releaser = GetOrAdd(key);
        if (!await releaser.SemaphoreSlim.WaitAsync(millisecondsTimeout).ConfigureAwait(continueOnCapturedContext))
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            return false;
        }

        try
        {
            if (task is not null)
            {
                await task().ConfigureAwait(continueOnCapturedContext);
            }
        }
        finally
        {
            _dictionary.Release(releaser);
        }
        return true;
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, and if not timed out, scynchronously execute an action and release.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="action">The synchronous action.</param>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>False if timed out, true if it successfully entered.</returns>
    public async ValueTask<bool> TryLockAsync(TKey key, Action action, TimeSpan timeout, bool continueOnCapturedContext = false)
    {
        var releaser = GetOrAdd(key);
        if (!await releaser.SemaphoreSlim.WaitAsync(timeout).ConfigureAwait(continueOnCapturedContext))
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            return false;
        }

        try
        {
            if (action is not null)
            {
                action();
            }
        }
        finally
        {
            _dictionary.Release(releaser);
        }
        return true;
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, and if not timed out, ascynchronously execute a <see cref="Func{Task}"/> and release.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="task">The asynchronous task.</param>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>False if timed out, true if it successfully entered.</returns>
    public async ValueTask<bool> TryLockAsync(TKey key, Func<Task> task, TimeSpan timeout, bool continueOnCapturedContext = false)
    {
        var releaser = GetOrAdd(key);
        if (!await releaser.SemaphoreSlim.WaitAsync(timeout).ConfigureAwait(continueOnCapturedContext))
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            return false;
        }

        try
        {
            if (task is not null)
            {
                await task().ConfigureAwait(continueOnCapturedContext);
            }
        }
        finally
        {
            _dictionary.Release(releaser);
        }
        return true;
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, and if not timed out, scynchronously execute an action and release, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="action">The synchronous action.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>False if timed out, true if it successfully entered.</returns>
    public async ValueTask<bool> TryLockAsync(TKey key, Action action, int millisecondsTimeout, CancellationToken cancellationToken, bool continueOnCapturedContext = false)
    {
        var releaser = GetOrAdd(key);
        try
        {
            if (!await releaser.SemaphoreSlim.WaitAsync(millisecondsTimeout, cancellationToken).ConfigureAwait(continueOnCapturedContext))
            {
                _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
                return false;
            }
        }
        catch (OperationCanceledException)
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            throw;
        }

        try
        {
            if (action is not null)
            {
                action();
            }
        }
        finally
        {
            _dictionary.Release(releaser);
        }
        return true;
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, and if not timed out, ascynchronously execute a <see cref="Func{Task}"/> and release, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="task">The asynchronous task.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>False if timed out, true if it successfully entered.</returns>
    public async ValueTask<bool> TryLockAsync(TKey key, Func<Task> task, int millisecondsTimeout, CancellationToken cancellationToken, bool continueOnCapturedContext = false)
    {
        var releaser = GetOrAdd(key);
        try
        {
            if (!await releaser.SemaphoreSlim.WaitAsync(millisecondsTimeout, cancellationToken).ConfigureAwait(continueOnCapturedContext))
            {
                _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
                return false;
            }
        }
        catch (OperationCanceledException)
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            throw;
        }

        try
        {
            if (task is not null)
            {
                await task().ConfigureAwait(continueOnCapturedContext);
            }
        }
        finally
        {
            _dictionary.Release(releaser);
        }
        return true;
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, and if not timed out, scynchronously execute an action and release, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="action">The synchronous action.</param>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>False if timed out, true if it successfully entered.</returns>
    public async ValueTask<bool> TryLockAsync(TKey key, Action action, TimeSpan timeout, CancellationToken cancellationToken, bool continueOnCapturedContext = false)
    {
        var releaser = GetOrAdd(key);
        try
        {
            if (!await releaser.SemaphoreSlim.WaitAsync(timeout, cancellationToken).ConfigureAwait(continueOnCapturedContext))
            {
                _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
                return false;
            }
        }
        catch (OperationCanceledException)
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            throw;
        }

        try
        {
            if (action is not null)
            {
                action();
            }
        }
        finally
        {
            _dictionary.Release(releaser);
        }
        return true;
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, and if not timed out, ascynchronously execute a <see cref="Func{Task}"/> and release, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="task">The asynchronous task.</param>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>False if timed out, true if it successfully entered.</returns>
    public async ValueTask<bool> TryLockAsync(TKey key, Func<Task> task, TimeSpan timeout, CancellationToken cancellationToken, bool continueOnCapturedContext = false)
    {
        var releaser = GetOrAdd(key);
        try
        {
            if (!await releaser.SemaphoreSlim.WaitAsync(timeout, cancellationToken).ConfigureAwait(continueOnCapturedContext))
            {
                _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
                return false;
            }
        }
        catch (OperationCanceledException)
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            throw;
        }

        try
        {
            if (task is not null)
            {
                await task().ConfigureAwait(continueOnCapturedContext);
            }
        }
        finally
        {
            _dictionary.Release(releaser);
        }
        return true;
    }
    #endregion AsynchronousTry

    #region AsynchronousTryNet8.0
#if NET8_0_OR_GREATER
#pragma warning disable CA1068 // CancellationToken parameters must come last
    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, and if not timed out, scynchronously execute an action and release.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="action">The synchronous action.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>False if timed out, true if it successfully entered.</returns>
    public async ValueTask<bool> TryLockAsync(TKey key, Action action, int millisecondsTimeout, ConfigureAwaitOptions configureAwaitOptions)
    {
        var releaser = GetOrAdd(key);
        if (!await releaser.SemaphoreSlim.WaitAsync(millisecondsTimeout).ConfigureAwait(configureAwaitOptions))
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            return false;
        }

        try
        {
            if (action is not null)
            {
                action();
            }
        }
        finally
        {
            _dictionary.Release(releaser);
        }
        return true;
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, and if not timed out, ascynchronously execute a <see cref="Func{Task}"/> and release.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="task">The asynchronous task.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>False if timed out, true if it successfully entered.</returns>
    public async ValueTask<bool> TryLockAsync(TKey key, Func<Task> task, int millisecondsTimeout, ConfigureAwaitOptions configureAwaitOptions)
    {
        var releaser = GetOrAdd(key);
        if (!await releaser.SemaphoreSlim.WaitAsync(millisecondsTimeout).ConfigureAwait(configureAwaitOptions))
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            return false;
        }

        try
        {
            if (task is not null)
            {
                await task().ConfigureAwait(configureAwaitOptions);
            }
        }
        finally
        {
            _dictionary.Release(releaser);
        }
        return true;
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, and if not timed out, scynchronously execute an action and release.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="action">The synchronous action.</param>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>False if timed out, true if it successfully entered.</returns>
    public async ValueTask<bool> TryLockAsync(TKey key, Action action, TimeSpan timeout, ConfigureAwaitOptions configureAwaitOptions)
    {
        var releaser = GetOrAdd(key);
        if (!await releaser.SemaphoreSlim.WaitAsync(timeout).ConfigureAwait(configureAwaitOptions))
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            return false;
        }

        try
        {
            if (action is not null)
            {
                action();
            }
        }
        finally
        {
            _dictionary.Release(releaser);
        }
        return true;
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, and if not timed out, ascynchronously execute a <see cref="Func{Task}"/> and release.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="task">The asynchronous task.</param>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>False if timed out, true if it successfully entered.</returns>
    public async ValueTask<bool> TryLockAsync(TKey key, Func<Task> task, TimeSpan timeout, ConfigureAwaitOptions configureAwaitOptions)
    {
        var releaser = GetOrAdd(key);
        if (!await releaser.SemaphoreSlim.WaitAsync(timeout).ConfigureAwait(configureAwaitOptions))
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            return false;
        }

        try
        {
            if (task is not null)
            {
                await task().ConfigureAwait(configureAwaitOptions);
            }
        }
        finally
        {
            _dictionary.Release(releaser);
        }
        return true;
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, and if not timed out, scynchronously execute an action and release, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="action">The synchronous action.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>False if timed out, true if it successfully entered.</returns>
    public async ValueTask<bool> TryLockAsync(TKey key, Action action, int millisecondsTimeout, CancellationToken cancellationToken, ConfigureAwaitOptions configureAwaitOptions)
    {
        var releaser = GetOrAdd(key);
        try
        {
            if (!await releaser.SemaphoreSlim.WaitAsync(millisecondsTimeout, cancellationToken).ConfigureAwait(configureAwaitOptions))
            {
                _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
                return false;
            }
        }
        catch (OperationCanceledException)
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            throw;
        }

        try
        {
            if (action is not null)
            {
                action();
            }
        }
        finally
        {
            _dictionary.Release(releaser);
        }
        return true;
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, and if not timed out, ascynchronously execute a <see cref="Func{Task}"/> and release, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="task">The asynchronous task.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>False if timed out, true if it successfully entered.</returns>
    public async ValueTask<bool> TryLockAsync(TKey key, Func<Task> task, int millisecondsTimeout, CancellationToken cancellationToken, ConfigureAwaitOptions configureAwaitOptions)
    {
        var releaser = GetOrAdd(key);
        try
        {
            if (!await releaser.SemaphoreSlim.WaitAsync(millisecondsTimeout, cancellationToken).ConfigureAwait(configureAwaitOptions))
            {
                _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
                return false;
            }
        }
        catch (OperationCanceledException)
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            throw;
        }

        try
        {
            if (task is not null)
            {
                await task().ConfigureAwait(configureAwaitOptions);
            }
        }
        finally
        {
            _dictionary.Release(releaser);
        }
        return true;
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, and if not timed out, scynchronously execute an action and release, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="action">The synchronous action.</param>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>False if timed out, true if it successfully entered.</returns>
    public async ValueTask<bool> TryLockAsync(TKey key, Action action, TimeSpan timeout, CancellationToken cancellationToken, ConfigureAwaitOptions configureAwaitOptions)
    {
        var releaser = GetOrAdd(key);
        try
        {
            if (!await releaser.SemaphoreSlim.WaitAsync(timeout, cancellationToken).ConfigureAwait(configureAwaitOptions))
            {
                _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
                return false;
            }
        }
        catch (OperationCanceledException)
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            throw;
        }

        try
        {
            if (action is not null)
            {
                action();
            }
        }
        finally
        {
            _dictionary.Release(releaser);
        }
        return true;
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, and if not timed out, ascynchronously execute a <see cref="Func{Task}"/> and release, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="task">The asynchronous task.</param>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>False if timed out, true if it successfully entered.</returns>
    public async ValueTask<bool> TryLockAsync(TKey key, Func<Task> task, TimeSpan timeout, CancellationToken cancellationToken, ConfigureAwaitOptions configureAwaitOptions)
    {
        var releaser = GetOrAdd(key);
        try
        {
            if (!await releaser.SemaphoreSlim.WaitAsync(timeout, cancellationToken).ConfigureAwait(configureAwaitOptions))
            {
                _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
                return false;
            }
        }
        catch (OperationCanceledException)
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            throw;
        }

        try
        {
            if (task is not null)
            {
                await task().ConfigureAwait(configureAwaitOptions);
            }
        }
        finally
        {
            _dictionary.Release(releaser);
        }
        return true;
    }
#pragma warning restore CA1068 // CancellationToken parameters must come last
#endif
    #endregion AsynchronousTryNet8.0

    #region Asynchronous
    /// <summary>
    /// Asynchronously lock based on a key.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>A disposable value.</returns>
    public async ValueTask<IDisposable> LockAsync(TKey key, bool continueOnCapturedContext = false)
    {
        var releaser = GetOrAdd(key);
        await releaser.SemaphoreSlim.WaitAsync().ConfigureAwait(continueOnCapturedContext);
        return releaser;
    }

    /// <summary>
    /// Asynchronously lock based on a key, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>A disposable value.</returns>
    public async ValueTask<IDisposable> LockAsync(TKey key, CancellationToken cancellationToken, bool continueOnCapturedContext = false)
    {
        var releaser = GetOrAdd(key);
        try
        {
            await releaser.SemaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }
        catch (OperationCanceledException)
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            throw;
        }
        return releaser;
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> LockOrNullAsync(TKey key, int millisecondsTimeout, bool continueOnCapturedContext = false)
    {
        var releaser = GetOrAdd(key);
        if (await releaser.SemaphoreSlim.WaitAsync(millisecondsTimeout).ConfigureAwait(continueOnCapturedContext))
        {
            return releaser;
        }
        _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
        return null;
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the <see cref="TimeSpan"/> to wait.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> LockOrNullAsync(TKey key, TimeSpan timeout, bool continueOnCapturedContext = false)
    {
        var releaser = GetOrAdd(key);
        if (await releaser.SemaphoreSlim.WaitAsync(timeout).ConfigureAwait(continueOnCapturedContext))
        {
            return releaser;
        }
        _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
        return null;
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> LockOrNullAsync(TKey key, int millisecondsTimeout, CancellationToken cancellationToken, bool continueOnCapturedContext = false)
    {
        var releaser = GetOrAdd(key);
        try
        {
            if (await releaser.SemaphoreSlim.WaitAsync(millisecondsTimeout, cancellationToken).ConfigureAwait(continueOnCapturedContext))
            {
                return releaser;
            }
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            return null;                
        }
        catch (OperationCanceledException)
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> LockOrNullAsync(TKey key, TimeSpan timeout, CancellationToken cancellationToken, bool continueOnCapturedContext = false)
    {
        var releaser = GetOrAdd(key);
        try
        {
            if (await releaser.SemaphoreSlim.WaitAsync(timeout, cancellationToken).ConfigureAwait(continueOnCapturedContext))
            {
                return releaser;
            }
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            return null;
        }
        catch (OperationCanceledException)
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            throw;
        }
    }
    #endregion Asynchronous

    #region AsynchronousNet8.0
#if NET8_0_OR_GREATER
#pragma warning disable CA1068 // CancellationToken parameters must come last
    /// <summary>
    /// Asynchronously lock based on a key.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>A disposable value.</returns>
    public async ValueTask<IDisposable> LockAsync(TKey key, ConfigureAwaitOptions configureAwaitOptions)
    {
        var releaser = GetOrAdd(key);
        await releaser.SemaphoreSlim.WaitAsync().ConfigureAwait(configureAwaitOptions);
        return releaser;
    }

    /// <summary>
    /// Asynchronously lock based on a key, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>A disposable value.</returns>
    public async ValueTask<IDisposable> LockAsync(TKey key, CancellationToken cancellationToken, ConfigureAwaitOptions configureAwaitOptions)
    {
        var releaser = GetOrAdd(key);
        try
        {
            await releaser.SemaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(configureAwaitOptions);
        }
        catch (OperationCanceledException)
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            throw;
        }
        return releaser;
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> LockOrNullAsync(TKey key, int millisecondsTimeout, ConfigureAwaitOptions configureAwaitOptions)
    {
        var releaser = GetOrAdd(key);
        if (await releaser.SemaphoreSlim.WaitAsync(millisecondsTimeout).ConfigureAwait(configureAwaitOptions))
        {
            return releaser;
        }
        _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
        return null;
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the <see cref="TimeSpan"/> to wait.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> LockOrNullAsync(TKey key, TimeSpan timeout, ConfigureAwaitOptions configureAwaitOptions)
    {
        var releaser = GetOrAdd(key);
        if (await releaser.SemaphoreSlim.WaitAsync(timeout).ConfigureAwait(configureAwaitOptions))
        {
            return releaser;
        }
        _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
        return null;
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> LockOrNullAsync(TKey key, int millisecondsTimeout, CancellationToken cancellationToken, ConfigureAwaitOptions configureAwaitOptions)
    {
        var releaser = GetOrAdd(key);
        try
        {
            if (await releaser.SemaphoreSlim.WaitAsync(millisecondsTimeout, cancellationToken).ConfigureAwait(configureAwaitOptions))
            {
                return releaser;
            }
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            return null;                
        }
        catch (OperationCanceledException)
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> LockOrNullAsync(TKey key, TimeSpan timeout, CancellationToken cancellationToken, ConfigureAwaitOptions configureAwaitOptions)
    {
        var releaser = GetOrAdd(key);
        try
        {
            if (await releaser.SemaphoreSlim.WaitAsync(timeout, cancellationToken).ConfigureAwait(configureAwaitOptions))
            {
                return releaser;
            }
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            return null;
        }
        catch (OperationCanceledException)
        {
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            throw;
        }
    }
#pragma warning restore CA1068 // CancellationToken parameters must come last
#endif
    #endregion AsynchronousNet8.0

    #region ConditionalSynchronous
    /// <summary>
    /// Synchronously lock based on a key. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public IDisposable? ConditionalLock(TKey key, bool getLock)
    {
        if (getLock)
        {
            return Lock(key);
        }
        return null;
    }

    /// <summary>
    /// Synchronously lock based on a key, while observing a <see cref="CancellationToken"/>. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public IDisposable? ConditionalLock(TKey key, bool getLock, CancellationToken cancellationToken)
    {
        if (getLock)
        {
            return Lock(key, cancellationToken);
        }
        return null;
    }

    /// <summary>
    /// Synchronously lock based on a key, setting a limit for the <see cref="TimeSpan"/> to wait. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public IDisposable? ConditionalLock(TKey key, bool getLock, int millisecondsTimeout)
    {
        if (getLock)
        {
            return LockOrNull(key, millisecondsTimeout);
        }
        return null;
    }

    /// <summary>
    /// Synchronously lock based on a key, setting a limit for the <see cref="TimeSpan"/> to wait. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public IDisposable? ConditionalLock(TKey key, bool getLock, TimeSpan timeout)
    {
        if (getLock)
        {
            return LockOrNull(key, timeout);
        }
        return null;
    }

    /// <summary>
    /// Synchronously lock based on a key, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public IDisposable? ConditionalLock(TKey key, bool getLock, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        if (getLock)
        {
            return LockOrNull(key, millisecondsTimeout, cancellationToken);
        }
        return null;
    }

    /// <summary>
    /// Synchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public IDisposable? ConditionalLock(TKey key, bool getLock, TimeSpan timeout, CancellationToken cancellationToken)
    {
        if (getLock)
        {
            return LockOrNull(key, timeout, cancellationToken);
        }
        return null;
    }
    #endregion ConditionalSynchronous

    #region ConditionalAsynchronous
    /// <summary>
    /// Asynchronously lock based on a key. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> ConditionalLockAsync(TKey key, bool getLock, bool continueOnCapturedContext = false)
    {
        if (getLock)
        {
            return await LockAsync(key, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
        }
        return null;
    }

    /// <summary>
    /// Asynchronously lock based on a key, while observing a <see cref="CancellationToken"/>. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> ConditionalLockAsync(TKey key, bool getLock, CancellationToken cancellationToken, bool continueOnCapturedContext = false)
    {
        if (getLock)
        {
            return await LockAsync(key, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
        }
        return null;
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> ConditionalLockAsync(TKey key, bool getLock, int millisecondsTimeout, bool continueOnCapturedContext = false)
    {
        if (getLock)
        {
            return await LockOrNullAsync(key, millisecondsTimeout, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
        }
        return null;
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the <see cref="TimeSpan"/> to wait. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> ConditionalLockAsync(TKey key, bool getLock, TimeSpan timeout, bool continueOnCapturedContext = false)
    {
        if (getLock)
        {
            return await LockOrNullAsync(key, timeout, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
        }
        return null;
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> ConditionalLockAsync(TKey key, bool getLock, int millisecondsTimeout, CancellationToken cancellationToken, bool continueOnCapturedContext = false)
    {
        if (getLock)
        {
            return await LockOrNullAsync(key, millisecondsTimeout, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
        }
        return null;
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false. Defaults to false.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> ConditionalLockAsync(TKey key, bool getLock, TimeSpan timeout, CancellationToken cancellationToken, bool continueOnCapturedContext = false)
    {
        if (getLock)
        {
            return await LockOrNullAsync(key, timeout, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
        }
        return null;
    }
    #endregion ConditionalAsynchronous

    #region ConditionalAsynchronousNet8.0
#if NET8_0_OR_GREATER
#pragma warning disable CA1068 // CancellationToken parameters must come last
    /// <summary>
    /// Asynchronously lock based on a key. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> ConditionalLockAsync(TKey key, bool getLock, ConfigureAwaitOptions configureAwaitOptions)
    {
        if (getLock)
        {
            var releaser = GetOrAdd(key);
            await releaser.SemaphoreSlim.WaitAsync().ConfigureAwait(configureAwaitOptions);
            return releaser;
        }
        return null;
    }

    /// <summary>
    /// Asynchronously lock based on a key, while observing a <see cref="CancellationToken"/>. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> ConditionalLockAsync(TKey key, bool getLock, CancellationToken cancellationToken, ConfigureAwaitOptions configureAwaitOptions)
    {
        if (getLock)
        {
            var releaser = GetOrAdd(key);
            try
            {
                await releaser.SemaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(configureAwaitOptions);
            }
            catch (OperationCanceledException)
            {
                _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
                throw;
            }
            return releaser;
        }
        return null;
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> ConditionalLockAsync(TKey key, bool getLock, int millisecondsTimeout, ConfigureAwaitOptions configureAwaitOptions)
    {
        if (getLock)
        {
            var releaser = GetOrAdd(key);
            if (await releaser.SemaphoreSlim.WaitAsync(millisecondsTimeout).ConfigureAwait(configureAwaitOptions))
            {
                return releaser;
            }
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            return null;
        }
        return null;
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the <see cref="TimeSpan"/> to wait. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> ConditionalLockAsync(TKey key, bool getLock, TimeSpan timeout, ConfigureAwaitOptions configureAwaitOptions)
    {
        if (getLock)
        {
            var releaser = GetOrAdd(key);
            if (await releaser.SemaphoreSlim.WaitAsync(timeout).ConfigureAwait(configureAwaitOptions))
            {
                return releaser;
            }
            _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
            return null;
        }
        return null;
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> ConditionalLockAsync(TKey key, bool getLock, int millisecondsTimeout, CancellationToken cancellationToken, ConfigureAwaitOptions configureAwaitOptions)
    {
        if (getLock)
        {
            var releaser = GetOrAdd(key);
            try
            {
                if (await releaser.SemaphoreSlim.WaitAsync(millisecondsTimeout, cancellationToken).ConfigureAwait(configureAwaitOptions))
                {
                    return releaser;
                }
                _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
                return null;
            }
            catch (OperationCanceledException)
            {
                _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
                throw;
            }
        }
        return null;
    }

    /// <summary>
    /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>. If the condition is false, it enters without locking.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="getLock">Condition for getting lock if true, otherwise enters without locking.</param>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <param name="configureAwaitOptions">Options used to configure how awaits on this task are performed.</param>
    /// <returns>A disposable value if entered, otherwise null.</returns>
    public async ValueTask<IDisposable?> ConditionalLockAsync(TKey key, bool getLock, TimeSpan timeout, CancellationToken cancellationToken, ConfigureAwaitOptions configureAwaitOptions)
    {
        if (getLock)
        {
            var releaser = GetOrAdd(key);
            try
            {
                if (await releaser.SemaphoreSlim.WaitAsync(timeout, cancellationToken).ConfigureAwait(configureAwaitOptions))
                {
                    return releaser;
                }
                _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
                return null;
            }
            catch (OperationCanceledException)
            {
                _dictionary.ReleaseWithoutSemaphoreRelease(releaser);
                throw;
            }
        }
        return null;
    }
#pragma warning restore CA1068 // CancellationToken parameters must come last
#endif
    #endregion ConditionalAsynchronousNet8.0

    /// <summary>
    /// Checks whether or not there is a thread making use of a keyed lock.
    /// </summary>
    /// <param name="key">The key requests are locked on.</param>
    /// <returns><see langword="true"/> if the key is in use; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsInUse(TKey key) => _dictionary.ContainsKey(key);

    /// <summary>
    /// Disposes the AtomicAsyncKeyedLocker.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the AtomicAsyncKeyedLocker.
    /// </summary>
    /// <param name="disposing">True if called from Dispose; false if called from finalizer.</param>
    public void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _dictionary.Dispose();
        }

        _disposed = true;
    }
}
