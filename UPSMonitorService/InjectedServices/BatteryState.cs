
using System.Diagnostics;

namespace UPSMonitorService
{
    // TODO: Warn about apparent recharge failure?

    public class BatteryState : IAsyncSingleton
    {
        private readonly Config config = null;
        private readonly Notify notify = null;

        private BatteryData Battery { get; set; } = null;

        public BatteryState(Config appConfig, Notify notifier)
        {
            config = appConfig;
            notify = notifier;
        }

        public async Task InitializeAsync()
        {
            var batt = new BatteryData(config);

            // no battery
            if (string.IsNullOrEmpty(batt.Name))
            {
                if (!string.IsNullOrEmpty(batt.Health))
                {
                    await notify.Send(EventLogEntryType.Error, "Invalid BatteryState Data", "Service initialization may have failed.");
                    return;
                }

                await notify.Send(EventLogEntryType.Information, "No Battery", "Service has started, but no battery was found.");
                return;
            }

            await BatteryChanged(batt);
        }

        /// <summary>
        /// Called at periodic intervals to update the system status
        /// and send notifications when necessary.
        /// </summary>
        public async Task Poll()
        {
            Console.WriteLine($"Polling CIM/WMI battery data at {DateTimeOffset.Now}");

            var newBatt = new BatteryData(config);

            // Name change
            if(!Battery.Name.Equals(newBatt.Name))
            {
                await BatteryChanged(newBatt);
                return;
            }

            // Health change
            if(!Battery.Health.Equals(newBatt.Health))
            {
                if (newBatt.Health.Equals("OK"))
                {
                    await notify.Send(EventLogEntryType.Information, "Battery Health", $"Battery health has returned to normal.\nHealth changed from {Battery.Health} to {newBatt.Health}.");
                }
                else
                {
                    await notify.Send(EventLogEntryType.Warning, "Battery Health", $"Battery health has changed from {Battery.Health} to {newBatt.Health}.\nService may be required.");
                }
            }

            // Status change
            if (Battery.Status != newBatt.Status)
            {
                await StatusChanged(newBatt);
            }

            // Charge level notifications, most severe to least severe
            if (Battery.ChargePct > config.ChargeLevels.Critical && newBatt.ChargePct <= config.ChargeLevels.Critical)
            {
                await ChargeNotification(newBatt, EventLogEntryType.Warning, "Battery Charge CRITICAL, Final Warning");
            }
            else if (Battery.ChargePct > config.ChargeLevels.Reserve && newBatt.ChargePct <= config.ChargeLevels.Reserve)
            {
                await ChargeNotification(newBatt, EventLogEntryType.Warning, "Battery Charge RESERVE");
            }
            else if (Battery.ChargePct > config.ChargeLevels.Advisory && newBatt.ChargePct <= config.ChargeLevels.Advisory)
            {
                await ChargeNotification(newBatt, EventLogEntryType.Warning, "Battery Charge LOW");
            }
            else if (Battery.ChargePct > config.ChargeLevels.Advisory && newBatt.ChargePct <= config.ChargeLevels.Advisory)
            {
                await ChargeNotification(newBatt, EventLogEntryType.Information, "Battery Charge Advisory");
            }

            // Update the stored battery info
            Battery = newBatt;
        }

        private async Task BatteryChanged(BatteryData batt)
        {
            // output summary
            if(string.IsNullOrEmpty(Battery?.Name))
            {
                await notify.Send(EventLogEntryType.Information, "Monitoring Started", batt.Summary);
            }
            else if(string.IsNullOrEmpty(batt.Name))
            {
                await notify.Send(EventLogEntryType.Information, "Monitoring Ended", "Service is running, but is no longer monitoring any battery.");
            }
            else
            {
                await notify.Send(EventLogEntryType.Information, "Monitoring Changed", batt.Summary);
            }

            // warn if battery is not healthy
            if (!string.IsNullOrEmpty(batt.Name) && !batt.Health.Equals("OK"))
            {
                await notify.Send(EventLogEntryType.Warning, "Battery Health", $"Battery may require service.\nHealth: {batt.Health}");
            }

            Battery = batt;
        }

        private async Task StatusChanged(BatteryData batt)
        {
            // It appears only values 1 and 2 are used (Discharging and AC Power).
            await notify.Send(EventLogEntryType.Information, "Battery State", $"State changed from {BatteryData.BatteryStatusValues[Battery.Status]} to {BatteryData.BatteryStatusValues[batt.Status]}.");
        }

        private async Task ChargeNotification(BatteryData batt, EventLogEntryType eventType, string title)
        {
            await notify.Send(eventType, title, $"Charge: {Battery.ChargePct}%, {Battery.Runtime}");
        }
    }
}
