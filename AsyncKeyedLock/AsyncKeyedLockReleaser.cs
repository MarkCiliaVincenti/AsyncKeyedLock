using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AsyncKeyedLock;

/// <summary>
/// Represents an <see cref="IDisposable"/> for AsyncKeyedLock.
/// </summary>
public sealed class AsyncKeyedLockReleaser<TKey> : IDisposable where TKey : notnull
{
#if NET9_0_OR_GREATER
    private readonly Lock? _lock;

    internal Lock? Lock
    {
        get => _lock;
    }
#endif

    internal bool IsNotInUse { get; set; } = false;

    private TKey _key;

    /// <summary>
    /// The key used for locking.
    /// </summary>
    public TKey Key
    {
        get => _key;
        internal set => _key = value;
    }

    private int _referenceCount = 1;

    /// <summary>
    /// The number of threads processing or waiting to process for the specific <see cref="Key"/>.
    /// </summary>
    public int ReferenceCount
    {
        get => _referenceCount;
        internal set => _referenceCount = value;
    }

    private readonly SemaphoreSlim _semaphoreSlim;

    /// <summary>
    /// The exposed <see cref="SemaphoreSlim"/> instance used to limit the number of threads that can access the lock concurrently.
    /// </summary>
    public SemaphoreSlim SemaphoreSlim => _semaphoreSlim;

    private readonly AsyncKeyedLockDictionary<TKey> _dictionary;

    internal AsyncKeyedLockReleaser(TKey key, SemaphoreSlim semaphoreSlim, AsyncKeyedLockDictionary<TKey> dictionary)
    {
        _key = key;
        _semaphoreSlim = semaphoreSlim;
        _dictionary = dictionary;
#if NET9_0_OR_GREATER
        if (dictionary.PoolingEnabled)
        {
            _lock = new();
        }
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TryIncrement(TKey key)
    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#if NET9_0_OR_GREATER
        if (Lock.TryEnter())
#else
        if (Monitor.TryEnter(this))
#endif
        {
            if (IsNotInUse || !_key.Equals(key)) // rare race condition
            {
#if NET9_0_OR_GREATER
                Lock.Exit();
#else
                Monitor.Exit(this);
#endif
                return false;
            }
            ++_referenceCount;
#if NET9_0_OR_GREATER
            Lock.Exit();
#else
            Monitor.Exit(this);
#endif
            return true;
        }
        return false;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TryIncrementNoPooling()
    {
        if (Monitor.TryEnter(this))
        {
            if (IsNotInUse) // rare race condition
            {
                Monitor.Exit(this);
                return false;
            }
            ++_referenceCount;
            Monitor.Exit(this);
            return true;
        }
        return false;
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