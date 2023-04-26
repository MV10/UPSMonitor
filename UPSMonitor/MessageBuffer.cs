namespace UPSMonitor
{
    /// <summary>
    /// Circular buffer of 100 messages. Use Enqueue to add a message,
    /// and use ToList to read all messages (which creates a copy).
    /// </summary>
    internal class MessageBuffer : Queue<Message>
    {
        private readonly int capacity = 100;
        private readonly object _lock = new();

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
