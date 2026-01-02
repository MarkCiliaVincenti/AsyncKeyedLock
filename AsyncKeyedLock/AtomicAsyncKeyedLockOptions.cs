// Copyright (c) All contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AsyncKeyedLock;

/// <summary>
/// Options for the <see cref="AtomicAsyncKeyedLocker{T}"/> constructors
/// </summary>
/// <remarks>
/// Initializes options for the <see cref="AtomicAsyncKeyedLocker{T}"/> constructors
/// </remarks>
/// <param name="poolSize">The size of the pool to use in order for generated objects to be reused. This is NOT a concurrency limit,
/// but if the pool is empty then a new object will be created rather than waiting for an object to return to
/// the pool. Set to 0 to disable pooling (strongly recommended to use). Defaults to 20.</param>
/// <param name="poolInitialFill">The number of items to fill the pool with during initialization. A value of -1 means to fill up to
/// the pool size. Defaults to 1.</param>
public sealed class AtomicAsyncKeyedLockOptions(int poolSize = 20, int poolInitialFill = 1)
{
    /// <summary>
    /// The size of the pool to use in order for generated objects to be reused. This is NOT a concurrency limit,
    /// but if the pool is empty then a new object will be created rather than waiting for an object to return to
    /// the pool. Set to 0 to disable pooling (strongly recommended to use). Defaults to 20.
    /// </summary>
    public int PoolSize { get; set; } = poolSize;

    /// <summary>
    /// The number of items to fill the pool with during initialization. A value of -1 means to fill up to the pool size. Defaults to 1.
    /// </summary>
    public int PoolInitialFill { get; set; } = poolInitialFill;

    /// <summary>
    /// Default lock options for the <see cref="AtomicAsyncKeyedLocker{T}"/> constructors.
    /// Sets <see cref="PoolSize"/> to 20 and <see cref="PoolInitialFill"/> to 1.
    /// </summary>
    public static AtomicAsyncKeyedLockOptions Default => new();
}
