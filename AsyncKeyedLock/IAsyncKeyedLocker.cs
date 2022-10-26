using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncKeyedLock
{
    /// <summary>
    /// AsyncKeyedLock interface
    /// </summary>
    public interface IAsyncKeyedLocker : IAsyncKeyedLocker<object>
    {
    }

    /// <summary>
    /// AsyncKeyedLock interface
    /// </summary>
    public interface IAsyncKeyedLocker<TKey>
    {
        /// <summary>
        /// The maximum number of requests for the semaphore that can be granted concurrently. Defaults to 1.
        /// </summary>
        int MaxCount { get; set; }

        #region Synchronous
        /// <summary>
        /// Synchronously lock based on a key.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <returns>A disposable value.</returns>
        IDisposable Lock(TKey key);

        /// <summary>
        /// Synchronously lock based on a key, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A disposable value.</returns>
        IDisposable Lock(TKey key, CancellationToken cancellationToken);

        /// <summary>
        /// Synchronously lock based on a key, setting a limit for the number of milliseconds to wait.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <returns>A disposable value.</returns>
        IDisposable Lock(TKey key, int millisecondsTimeout);

        /// <summary>
        /// Synchronously lock based on a key, setting a limit for the number of milliseconds to wait.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="success">False if timed out, true if it successfully entered.</param>
        /// <returns>A disposable value.</returns>
        IDisposable Lock(TKey key, int millisecondsTimeout, out bool success);

        /// <summary>
        /// Synchronously lock based on a key, setting a limit for the <see cref="TimeSpan"/> to wait.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <returns>A disposable value.</returns>
        IDisposable Lock(TKey key, TimeSpan timeout);

        /// <summary>
        /// Synchronously lock based on a key, setting a limit for the <see cref="TimeSpan"/> to wait.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="success">False if timed out, true if it successfully entered.</param>
        /// <returns>A disposable value.</returns>
        IDisposable Lock(TKey key, TimeSpan timeout, out bool success);

        /// <summary>
        /// Synchronously lock based on a key, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A disposable value.</returns>
        IDisposable Lock(TKey key, int millisecondsTimeout, CancellationToken cancellationToken);

        /// <summary>
        /// Synchronously lock based on a key, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <param name="success">False if timed out, true if it successfully entered.</param>
        /// <returns>A disposable value.</returns>
        IDisposable Lock(TKey key, int millisecondsTimeout, CancellationToken cancellationToken, out bool success);

        /// <summary>
        /// Synchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A disposable value.</returns>
        IDisposable Lock(TKey key, TimeSpan timeout, CancellationToken cancellationToken);

        /// <summary>
        /// Synchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <param name="success">False if timed out, true if it successfully entered.</param>
        /// <returns>A disposable value.</returns>
        IDisposable Lock(TKey key, TimeSpan timeout, CancellationToken cancellationToken, out bool success);
        #endregion Synchronous

        #region AsynchronousTry
        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, and if not timed out, scynchronously execute an action and release.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="action">The synchronous action.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        Task<bool> TryLockAsync(TKey key, Action action, int millisecondsTimeout);

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, and if not timed out, scynchronously execute an action and release.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="task">The asynchronous task.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        Task<bool> TryLockAsync(TKey key, Func<Task> task, int millisecondsTimeout);

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, and if not timed out, scynchronously execute an action and release.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="action">The synchronous action.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        Task<bool> TryLockAsync(TKey key, Action action, TimeSpan timeout);

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, and if not timed out, scynchronously execute an action and release.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="task">The asynchronous task.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        Task<bool> TryLockAsync(TKey key, Func<Task> task, TimeSpan timeout);

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, and if not timed out, scynchronously execute an action and release, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="action">The synchronous action.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        Task<bool> TryLockAsync(TKey key, Action action, int millisecondsTimeout, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, and if not timed out, scynchronously execute an action and release, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="task">The asynchronous task.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        Task<bool> TryLockAsync(TKey key, Func<Task> task, int millisecondsTimeout, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, and if not timed out, scynchronously execute an action and release, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="action">The synchronous action.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        Task<bool> TryLockAsync(TKey key, Action action, TimeSpan timeout, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, and if not timed out, scynchronously execute an action and release, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="task">The asynchronous task.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        Task<bool> TryLockAsync(TKey key, Func<Task> task, TimeSpan timeout, CancellationToken cancellationToken);
        #endregion AsynchronousTry

        #region Asynchronous
        /// <summary>
        /// Asynchronously lock based on a key.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <returns>A disposable value.</returns>
        Task<IDisposable> LockAsync(TKey key);

        /// <summary>
        /// Asynchronously lock based on a key, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A disposable value.</returns>
        Task<IDisposable> LockAsync(TKey key, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <returns>A disposable value.</returns>
        Task<IDisposable> LockAsync(TKey key, int millisecondsTimeout);

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the <see cref="TimeSpan"/> to wait.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <returns>A disposable value.</returns>
        Task<IDisposable> LockAsync(TKey key, TimeSpan timeout);

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A disposable value.</returns>
        Task<IDisposable> LockAsync(TKey key, int millisecondsTimeout, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A disposable value.</returns>
        Task<IDisposable> LockAsync(TKey key, TimeSpan timeout, CancellationToken cancellationToken);
        #endregion

        /// <summary>
        /// Checks whether or not there is a thread making use of a keyed lock.
        /// </summary>
        /// <param name="key">The key requests are locked on.</param>
        /// <returns><see langword="true"/> if the key is in use; otherwise, false.</returns>
        bool IsInUse(TKey key);

        /// <summary>
        /// Get the number of requests concurrently locked for a given key.
        /// </summary>
        /// <param name="key">The key requests are locked on.</param>
        /// <returns>The number of requests.</returns>
        [Obsolete("This method should not longer be used as it is confusing with Semaphore terminology. Use <see cref=\"GetCurrentCount\"/> or <see cref=\"GetRemaningCount\"/> instead depending what you want to do.")]
        int GetCount(TKey key);

        /// <summary>
        /// Get the number of requests concurrently locked for a given key.
        /// </summary>
        /// <param name="key">The key requests are locked on.</param>
        /// <returns>The number of requests concurrently locked for a given key.</returns>
        int GetRemainingCount(TKey key);

        /// <summary>
        /// Get the number of remaining threads that can enter the lock for a given key.
        /// </summary>
        /// <param name="key">The key requests are locked on.</param>
        /// <returns>The number of remaining threads that can enter the lock for a given key.</returns>
        int GetCurrentCount(TKey key);

        /// <summary>
        /// Forces requests to be released from the semaphore.
        /// </summary>
        /// <param name="key">The key requests are locked on.</param>
        /// <returns><see langword="true"/> if the key is successfully found and removed; otherwise, false.</returns>
        bool ForceRelease(TKey key);
    }
}
