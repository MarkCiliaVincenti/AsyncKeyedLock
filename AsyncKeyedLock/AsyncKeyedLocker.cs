using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncKeyedLock
{
    /// <summary>
    /// AsyncKeyedLock class, adapted and improved from <see href="https://stackoverflow.com/questions/31138179/asynchronous-locking-based-on-a-key/31194647#31194647">Stephen Cleary's solution</see>.
    /// </summary>
    public class AsyncKeyedLocker : AsyncKeyedLocker<object>
    {
        /// <summary>
        /// Constructor for AsyncKeyedLock.
        /// </summary>
        /// <param name="maxCount">The maximum number of requests for the semaphore that can be granted concurrently. Defaults to 1.</param>
        public AsyncKeyedLocker(int maxCount = 1)
        {
            MaxCount = maxCount;
        }
    }
}
