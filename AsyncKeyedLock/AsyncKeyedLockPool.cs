using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AsyncKeyedLock
{
    internal sealed class AsyncKeyedLockPool<TKey> : IDisposable
    {
        private readonly BlockingCollection<AsyncKeyedLockReleaser<TKey>> _objects;
        private readonly Func<TKey, AsyncKeyedLockReleaser<TKey>> _objectGenerator;

        public AsyncKeyedLockPool(AsyncKeyedLockDictionary<TKey> asyncKeyedLockDictionary, int capacity, int initialFill = -1)
        {
            _objects = new BlockingCollection<AsyncKeyedLockReleaser<TKey>>(new ConcurrentBag<AsyncKeyedLockReleaser<TKey>>(), capacity);
            _objectGenerator = (key) => new AsyncKeyedLockReleaser<TKey>(
                key,
                new SemaphoreSlim(asyncKeyedLockDictionary.MaxCount, asyncKeyedLockDictionary.MaxCount),
                asyncKeyedLockDictionary);

            if (initialFill < 0)
            {
                for (int i = 0; i < capacity; ++i)
                {
                    var releaser = _objectGenerator(default);
                    releaser.IsNotInUse = true;
                    _objects.Add(releaser);
                }
            }
            else
            {
                initialFill = Math.Min(initialFill, capacity);
                for (int i = 0; i < initialFill; ++i)
                {
                    var releaser = _objectGenerator(default);
                    releaser.IsNotInUse = true;
                    _objects.Add(releaser);
                }
            }
        }

        public void Dispose()
        {
            _objects.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncKeyedLockReleaser<TKey> GetObject(TKey key)
        {
            if (_objects.TryTake(out var item))
            {
                item.Key = key;
                item.IsNotInUse = false;
                return item;
            }
            return _objectGenerator(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PutObject(AsyncKeyedLockReleaser<TKey> item)
        {
            _objects.TryAdd(item);
        }
    }
}