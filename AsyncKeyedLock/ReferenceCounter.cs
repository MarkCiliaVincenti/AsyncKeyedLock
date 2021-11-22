namespace AsyncKeyedLock
{
    internal sealed class ReferenceCounter<T>
    {
        public int ReferenceCount { get; set; } = 1;
        public T Value { get; private set; }

        public ReferenceCounter(T value)
        {
            Value = value;
        }
    }
}
