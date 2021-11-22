using System;
using System.Threading;

namespace AsyncKeyedLock
{
    internal sealed class Releaser : IDisposable
    {
        public readonly object _key;

        public Releaser(object key)
        {
            _key = key;
        }

        public void Dispose()
        {
            ReferenceCounter<SemaphoreSlim> item;
            lock (AsyncKeyedLocker.SemaphoreSlims)
            {
                item = AsyncKeyedLocker.SemaphoreSlims[_key];
                --item.ReferenceCount;
                if (item.ReferenceCount == 0)
                {
                    AsyncKeyedLocker.SemaphoreSlims.Remove(_key);
                }
            }
            item.Value.Release();
        }
    }
}
