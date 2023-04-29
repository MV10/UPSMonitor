
using Microsoft.Win32;

namespace UPSMonitor
{
    /// <summary>
    /// Reads and writes Program.MessageHistory
    /// </summary>
    internal static class MessageStorage
    {
        /// <summary>
        /// Clear and reload Program.MessageHistory from storage
        /// </summary>
        public static void ReadHistory()
        {
            Program.MessageHistory.Clear();

            // although HKLM would be preferable, it requires admin rights
            using var regkey = Registry.CurrentUser
                .CreateSubKey(@"SOFTWARE\mcguirev10\UPSMonitor\History", true);
            if (regkey == null) return;

            var count = (int)regkey.GetValue("Count", 5);
            if (count == 0) return;

            for (int i = 0; i < count; i++)
            {
                var tsKey = $"{i:000} ts";
                var msgKey = $"{i:000} msg";
                var tsValue = (string)regkey.GetValue(tsKey, string.Empty);
                var msgValue = (string)regkey.GetValue(msgKey, string.Empty);
                if (!string.IsNullOrEmpty(msgValue) && !string.IsNullOrEmpty(tsValue))
                {
                    DateTimeOffset.TryParse(tsValue, out var timestamp);
                    if (timestamp != DateTimeOffset.MinValue)
                    {
                        var msg = new Message()
                        {
                            Timestamp = timestamp,
                            Content = msgValue
                        };
                        Program.MessageHistory.Enqueue(msg);
                    }
                }
            }
        }

        /// <summary>
        /// Write contents of Program.MessageHistory queue to storage
        /// </summary>
        public static void WriteHistory()
        {
            // always work from a local copy
            var messages = Program.MessageHistory.ToList();

            using var regkey = Registry.CurrentUser
                .OpenSubKey(@"SOFTWARE\mcguirev10\UPSMonitor\History", true);
            if (regkey == null) return;

            regkey.SetValue("Count", messages.Count);
            if (messages.Count == 0) return;

            for (int i = 0; i < messages.Count; i++)
            {
                var tsKey = $"{i:000} ts";
                var msgKey = $"{i:000} msg";
                regkey.SetValue(tsKey, messages[i].Timestamp.ToString("O"));
                regkey.SetValue(msgKey, messages[i].Content);
            }
        }
    }
}
