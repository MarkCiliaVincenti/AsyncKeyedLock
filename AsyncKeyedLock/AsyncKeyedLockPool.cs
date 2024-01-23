using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AsyncKeyedLock
{
    internal sealed class AsyncKeyedLockPool<TKey> : IDisposable
    {
        private readonly IList<AsyncKeyedLockReleaser<TKey>> _objects;
        private readonly Func<TKey, AsyncKeyedLockReleaser<TKey>> _objectGenerator;
        private readonly int _capacity;

        public AsyncKeyedLockPool(AsyncKeyedLockDictionary<TKey> asyncKeyedLockDictionary, int capacity, int initialFill = -1)
        {
            _capacity = capacity;
            _objects = new List<AsyncKeyedLockReleaser<TKey>>(capacity);
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
            _objects.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncKeyedLockReleaser<TKey> GetObject(TKey key)
        {
            Monitor.Enter(_objects);
            if (_objects.Count > 0)
            {
                int lastPos = _objects.Count - 1;
                var item = _objects[lastPos];
                _objects.RemoveAt(lastPos);
                Monitor.Exit(_objects);
                item.Key = key;
                item.IsNotInUse = false;
                return item;
            }
            Monitor.Exit(_objects);

            return _objectGenerator(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PutObject(AsyncKeyedLockReleaser<TKey> item)
        {
            Monitor.Enter(_objects);
            if (_objects.Count < _capacity)
            {
                _objects.Add(item);
            }
            Monitor.Exit(_objects);
        }
    }
}