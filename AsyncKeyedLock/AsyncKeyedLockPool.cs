using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace AsyncKeyedLock
{
    internal sealed class AsyncKeyedLockPool<TKey>
    {
        private readonly BlockingCollection<AsyncKeyedLockReleaser<TKey>> _objects;
        private readonly Func<TKey, AsyncKeyedLockReleaser<TKey>> _objectGenerator;

        public AsyncKeyedLockPool(Func<TKey, AsyncKeyedLockReleaser<TKey>> objectGenerator, int capacity, int initialFill = -1)
        {
            _objects = new BlockingCollection<AsyncKeyedLockReleaser<TKey>>(new ConcurrentBag<AsyncKeyedLockReleaser<TKey>>(), capacity);
            _objectGenerator = objectGenerator;
            if (initialFill < 0)
            {
                for (int i = 0; i < capacity; ++i)
                {
                    _objects.Add(_objectGenerator(default));
                }
            }
            else
            {
                for (int i = 0; i < initialFill; ++i)
                {
                    _objects.Add(_objectGenerator(default));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncKeyedLockReleaser<TKey> GetObject(TKey key)
        {
            if (_objects.TryTake(out var item))
            {
                item.Key = key;
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