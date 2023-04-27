
using UPSMonitorService.Models;

namespace UPSMonitorService
{
    /// <summary>
    /// Application uses this configuration object instead of IConfiguration
    /// because some of the settings can be changed at runtime (such as disabling
    /// email after an error occurs).
    /// </summary>
    public class Config
    {
        public Config(IConfiguration config)
        {
            Settings = config.GetSection("Settings").Get<SettingsConfig>();
            ChargeLevels = config.GetSection("BatteryLevels").Get<ChargeLevelConfig>();
            Email = config.GetSection("Email").Get<EmailConfig>();

            if (Email.Subject.Contains('*'))
                Email.Subject = Email.Subject.Replace("*", Environment.MachineName);
        }

        /// <summary>
        /// Read from appsettings.json.
        /// </summary>
        public SettingsConfig Settings { get; set; } = new();

        /// <summary>
        /// Read from appsettings.json.
        /// </summary>
        public ChargeLevelConfig ChargeLevels { get; set; } = new();

        /// <summary>
        /// Read from appsettings.json.
        /// </summary>
        public EmailConfig Email { get; set; } = new();
    }
}
