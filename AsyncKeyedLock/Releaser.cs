using System;
using System.Threading;

namespace AsyncKeyedLock
{
    internal sealed class Releaser : IDisposable
    {
        private readonly AsyncKeyedLocker _asyncKeyedLocker;
        private readonly object _key;

        public Releaser(AsyncKeyedLocker asyncKeyedLocker, object key)
        {
            _asyncKeyedLocker = asyncKeyedLocker;
            _key = key;
        }

        public void Dispose()
        {
            ReferenceCounter<SemaphoreSlim> item;
            lock (_asyncKeyedLocker.SemaphoreSlims)
            {
                item = _asyncKeyedLocker.SemaphoreSlims[_key];
                --item.ReferenceCount;
                if (item.ReferenceCount == 0)
                {
                    _asyncKeyedLocker.SemaphoreSlims.Remove(_key);
                }
            }
            item.Value.Release();
        }
    }
}
