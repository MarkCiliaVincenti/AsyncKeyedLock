using System;

namespace AsyncKeyedLock
{
    /// <summary>
    /// A disposable that does absolutely nothing.
    /// </summary>
    public class EmptyDisposable : IDisposable
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
