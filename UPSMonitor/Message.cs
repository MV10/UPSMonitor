namespace UPSMonitor
{
    internal class Message
    {
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;
        public string Content { get; set; } = string.Empty;
    }
}
