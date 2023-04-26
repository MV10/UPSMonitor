namespace UPSMonitorService.Models
{
    public class SettingsConfig
    {
        /// <summary>
        /// How often the service reads battery status. Expressed in seconds. Default is 1.
        /// </summary>
        public int PollingSeconds { get; set; } = 1;

        /// <summary>
        /// The CIM/WMI Win32_Battery Name property to look for. Default is an empty string,
        /// which uses the first battery returned by the CIM query.
        /// </summary>
        public string BatteryName { get; set; } = string.Empty;

        /// <summary>
        /// Whether or not email notifications are sent. Default is false.
        /// </summary>
        public bool NotificationEmails { get; set; } = false;

        /// <summary>
        /// Whether or not notifications are written to the Event Log. Service
        /// startup and shutdown is always logged. Default is true.
        /// </summary>
        public bool NotificationEventLog { get; set; } = true;

        /// <summary>
        /// Whether or not Windows pop-up notifications are shown. Default is true.
        /// </summary>
        public bool NotificationPopups { get; set; } = true;

        /// <summary>
        /// If true, when the service is run interactively (from a command prompt), pop-up
        /// messages will still try to use the system tray UPSMonitor app via Named Pipes
        /// instead of the service directly sending "toast" pop-up messages. Default is
        /// false. No effect if NotificationPopups is false.
        /// </summary>
        public bool RemotePopupOnly { get; set; } = false;
    }
}
