
using Microsoft.Toolkit.Uwp.Notifications;
using System.Diagnostics;

namespace UPSMonitorService.App
{
    /// <summary>
    /// A hosted service which runs the main processing loop.
    /// </summary>
    public class ProcessingLoop : BackgroundService
    {
        private readonly Config config = null;
        private readonly BatteryState batteryState = null;
        private readonly Notify notify = null;

        public ProcessingLoop(Config appConfig, BatteryState state, Notify notifier)
        {
            config = appConfig;
            batteryState = state;
            notify = notifier;

            LogConfiguration(notify);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await batteryState.Poll();
                    await Task.Delay(config.Settings.PollingSeconds * 1000, stoppingToken);
                }
            }
            catch (TaskCanceledException)
            {
                // Expected, app is exiting
            }
            finally
            {
                // Remove any pop-up or Activity Center notifications, otherwise clicking one would re-start the program
                if (Environment.UserInteractive && config.Settings.NotificationPopups)
                    ToastNotificationManagerCompat.Uninstall();
            }
        }

        private void LogConfiguration(Notify notifier)
        {
            var details =
                $"Polling interval: {config.Settings.PollingSeconds} sec.\n" +
                $"Target Battery: {(string.IsNullOrEmpty(config.Settings.BatteryName) ? "(unspecified)" : config.Settings.BatteryName)}\n" +
                $"Charge Notifications: {config.ChargeLevels.Advisory}%, {config.ChargeLevels.Low}%, {config.ChargeLevels.Reserve}%, {config.ChargeLevels.Critical}%\n" +
                $"Email to: {(config.Settings.NotificationEmails ? config.Email.RecipientList : "(disabled)")}";

            notifier.SendEventLog(EventLogEntryType.Information, "Loaded configuration:", details);
        }
    }
}
