namespace UPSMonitor
{
    internal class Message
    {
        /// <summary>
        /// When the message was received
        /// </summary>
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;
        
        /// <summary>
        /// The message; see Program class for linebreak constant
        /// </summary>
        public string Content { get; set; } = string.Empty;
    }
}
