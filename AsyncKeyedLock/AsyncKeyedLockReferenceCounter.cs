using System.Threading;

namespace AsyncKeyedLock
{
    /// <summary>
    /// The AsyncKeyedLock ReferenceCounter class
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    public sealed class AsyncKeyedLockReferenceCounter<TKey>
    {
        private readonly TKey _key;
        /// <summary>
        /// The key
        /// </summary>
        public TKey Key => _key;

        /// <summary>
        /// The reference count
        /// </summary>
        public int ReferenceCount { get; set; }

        private readonly SemaphoreSlim _semaphoreSlim;
        
        /// <summary>
        /// The SemaphoreSlim object.
        /// </summary>
        public SemaphoreSlim SemaphoreSlim => _semaphoreSlim;

        /// <summary>
        /// The releaser
        /// </summary>
        public AsyncKeyedLockReleaser<TKey> Releaser;

        /// <summary>
        /// Constructor for AsyncKeyedLockReferenceCounter
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="semaphoreSlim">The SemaphoreSlim object</param>
        /// <param name="dictionary">The dictionary</param>
        public AsyncKeyedLockReferenceCounter(TKey key, SemaphoreSlim semaphoreSlim, AsyncKeyedLockerDictionary<TKey> dictionary)
        {
            _key = key;
            _semaphoreSlim = semaphoreSlim;
            Releaser = new AsyncKeyedLockReleaser<TKey>(dictionary, this);
        }
    }
}
