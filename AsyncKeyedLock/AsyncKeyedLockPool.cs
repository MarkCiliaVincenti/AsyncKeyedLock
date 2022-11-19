using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace AsyncKeyedLock
{
    internal class AsyncKeyedLockPool<TKey>
    {
        private readonly BlockingCollection<AsyncKeyedLockReleaser<TKey>> _objects;
        private readonly Func<TKey, AsyncKeyedLockReleaser<TKey>> _objectGenerator;

        public AsyncKeyedLockPool(Func<TKey, AsyncKeyedLockReleaser<TKey>> objectGenerator, int capacity)
        {
            _objects = new BlockingCollection<AsyncKeyedLockReleaser<TKey>>(new ConcurrentBag<AsyncKeyedLockReleaser<TKey>>(), capacity);
            _objectGenerator = objectGenerator;
            for (int i = 0; i < capacity; i++)
            {
                _objects.Add(_objectGenerator(default));
            }
        }

        public AsyncKeyedLockReleaser<TKey> GetObject(TKey key)
        {
            if (_objects.TryTake(out var item))
            {
                item.Key = key;
                return item;
            }
            return _objectGenerator(key);
        }

        public void PutObject(AsyncKeyedLockReleaser<TKey> item)
        {
            _objects.TryAdd(item);
        }
    }
}
