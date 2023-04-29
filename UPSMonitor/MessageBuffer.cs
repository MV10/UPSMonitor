namespace UPSMonitor
{
    /// <summary>
    /// Circular buffer of 100 messages. Use Enqueue to add a message,
    /// and use ToList to read all messages (which creates a copy).
    /// </summary>
    internal class MessageBuffer : Queue<Message>
    {
        private static readonly int capacity = 100;
        private static readonly object _lock = new();

        // Reaching capacity would cause the queue to allocate another 100 slots
        public MessageBuffer() : base(capacity + 1)
        { }

        new public void Enqueue(Message msg)
        {
            lock(_lock)
            {
                if (Count == capacity) Dequeue();
                base.Enqueue(msg);
            }
        }
    }
}
