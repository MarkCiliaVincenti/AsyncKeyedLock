namespace AsyncKeyedLock
{
    /// <summary>
    /// Options for the <see cref="AsyncKeyedLocker"/> constructors
    /// </summary>
    public struct AsyncKeyedLockOptions
    {
        /// <summary>
        /// The maximum number of requests for the semaphore that can be granted concurrently. Defaults to 1.
        /// </summary>
        public int MaxCount { get; set; } = 1;

        /// <summary>
        /// The size of the pool to use in order for generated objects to be reused. Defaults to 0 (disabled).
        /// </summary>
        public int PoolSize { get; set; } = 0;

        /// <summary>
        /// The number of items to fill the pool with during initialization. Defaults to -1 (fill up to pool size).
        /// </summary>
        public int PoolInitialFill { get; set; } = -1;

        /// <summary>
        /// Initializes options for the <see cref="AsyncKeyedLocker"/> constructors
        /// </summary>
        /// <param name="maxCount">The maximum number of requests for the semaphore that can be granted concurrently. Defaults to 1.</param>
        /// <param name="poolSize">The size of the pool to use in order for generated objects to be reused. Defaults to 0 (disabled).</param>
        /// <param name="poolInitialFill">The number of items to fill the pool with during initialization. Defaults to -1 (fill up to pool size).</param>
        public AsyncKeyedLockOptions(int maxCount = 1, int poolSize = 0, int poolInitialFill = -1)
        {
            MaxCount = maxCount;
            PoolSize = poolSize;
            PoolInitialFill = poolInitialFill;
        }
    }
}