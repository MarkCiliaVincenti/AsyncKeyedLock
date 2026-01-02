// Copyright (c) All contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AsyncKeyedLock;

internal sealed class AtomicAsyncKeyedLockPool<TKey> : IDisposable where TKey : notnull
{
#if NET9_0_OR_GREATER
    private readonly Lock _lock = new();
#endif
    private readonly List<SemaphoreSlim> _objects;
    private readonly int _capacity;

    public AtomicAsyncKeyedLockPool(AtomicAsyncKeyedLockDictionary<TKey> atomicAsyncKeyedLockDictionary, int capacity, int initialFill = -1)
    {
        _capacity = capacity;
        _objects = new List<SemaphoreSlim>(capacity);

        if (initialFill < 0)
        {
            for (int i = 0; i < capacity; ++i)
            {
                _objects.Add(new SemaphoreSlim(1, 1));
            }
        }
        else
        {
            initialFill = Math.Min(initialFill, capacity);
            for (int i = 0; i < initialFill; ++i)
            {
                _objects.Add(new SemaphoreSlim(1, 1));
            }
        }
    }

    public void Dispose()
    {
        _objects.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SemaphoreSlim GetObject()
    {
#if NET9_0_OR_GREATER
        _lock.Enter();
#else
        Monitor.Enter(_objects);
#endif
        if (_objects.Count > 0)
        {
            int lastPos = _objects.Count - 1;
            var item = _objects[lastPos];
            _objects.RemoveAt(lastPos);
#if NET9_0_OR_GREATER
            _lock.Exit();
#else
            Monitor.Exit(_objects);
#endif
            return item;
        }
#if NET9_0_OR_GREATER
        _lock.Exit();
#else
        Monitor.Exit(_objects);
#endif

        return new SemaphoreSlim(1, 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PutObject(SemaphoreSlim item)
    {
#if NET9_0_OR_GREATER
        _lock.Enter();
#else
        Monitor.Enter(_objects);
#endif
        if (_objects.Count < _capacity)
        {
            _objects.Add(item);
        }
#if NET9_0_OR_GREATER
        _lock.Exit();
#else
        Monitor.Exit(_objects);
#endif
    }
}
