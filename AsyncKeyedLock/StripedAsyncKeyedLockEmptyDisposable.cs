using System;

namespace AsyncKeyedLock
{
    /// <summary>
    /// A disposable that does absolutely nothing.
    /// </summary>
    public class StripedAsyncKeyedLockEmptyDisposable : IDisposable
    {
        /// <summary>
        /// Dispose but in reality do nothing
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
