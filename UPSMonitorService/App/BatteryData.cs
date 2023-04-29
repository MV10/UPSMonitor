
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;

namespace UPSMonitorService
{
    /// <summary>
    /// Information about the battery being monitored by the service.
    /// </summary>
    public class BatteryData
    {
        /// <summary>
        /// User-friendly name of the device. Maps to the WMI Win32_Battery Name property.
        /// </summary>
        public string Name { get; private set; } = string.Empty;

        /// <summary>
        /// Hardware condition; anything other than "OK" is reported as a warning. Maps to the WMI Win32_Battery Status property.
        /// </summary>
        public string Health { get; private set; } = string.Empty;

        /// <summary>
        /// Status seems minimally tracked by WMI, decode with AvailabilityValues dictionary. Maps to the WMI Win32_Battery BatteryStatus property.
        /// </summary>
        public int Status { get; private set; } = 0;

        /// <summary>
        /// Estimated remaining charge in percentage (0-100). Maps to the WMI Win32_Battery EstimatedChargeRemaining property.
        /// </summary>
        public int ChargePct { get; private set; } = 0;

        /// <summary>
        /// Estimated remaining runtime in minutes as "est. ## min.", or if greater than 1440 (24hrs) is reported,
        /// the value will be "time unknown". Maps to the WMI Win32_Battery EstimatedRunTime property.
        /// </summary>
        public string Runtime { get; private set; } = "time unknown";

        /// <summary>
        /// A three-line summary of battery properties, or "No battery data available."
        /// </summary>
        public string Summary { get; private set; } = "No battery data available.";

        /// <summary>
        /// The moment when the properties were last updated.
        /// </summary>
        public DateTimeOffset Timestamp { get; private set; } = DateTimeOffset.Now;

        private readonly Config config = null;

        public BatteryData(Config appConfig)
        {
            config = appConfig;
            RefreshData();
        }

        public void RefreshData()
        {
            Timestamp = DateTimeOffset.Now;

            List<CimInstance> data = null;

            try
            {
                data = QueryCIM("select Name, Status, BatteryStatus, EstimatedChargeRemaining, EstimatedRunTime from Win32_Battery");
                if (data.Count > 0)
                {
                    var props = 
                        (data.Count == 1 || string.IsNullOrEmpty(config.Settings.BatteryName)) 
                        ? data.FirstOrDefault().CimInstanceProperties 
                        : FindBatteryName(data);

                    if(props != null)
                    {
                        Name = (string)props["Name"].Value;
                        Health = (string)props["Status"].Value;
                        Status = (ushort)props["BatteryStatus"].Value;
                        ChargePct = (ushort)props["EstimatedChargeRemaining"].Value;
                        
                        var min = Convert.ToInt32((UInt32)props["EstimatedRunTime"].Value);
                        Runtime = (min > 1440) ? "time unknown" : $"est. {min} min.";
                    }
                }
            }
            catch(Exception ex)
            {
                // only SystemStatus.Init looks for this; we don't want to log
                // exceptions every second, or whatever the polling rate is...
                Health = $"Exception querying CIM for Win32_Battery data: {ex.Message}";
            }
            finally
            {
                if(data != null)
                {
                    foreach (var i in data)
                        i.Dispose();
                }
            }

            Summary = (string.IsNullOrEmpty(Name))
                ? "No battery data available."
                : $"Name: {Name}\n" +
                $"Status: {Health}, {BatteryStatusValues[Status]} ({Status})\n" +
                $"Charge: {ChargePct}%, {Runtime}";
        }

        private CimKeyedCollection<CimProperty> FindBatteryName(List<CimInstance> data)
        {
            foreach(var inst in data)
            {
                if (inst.CimInstanceProperties["Name"].Value.Equals(config.Settings.BatteryName)) 
                    return inst.CimInstanceProperties;
            }
            return null;
        }

        private List<CimInstance> QueryCIM(string query, string namespc = @"root\cimv2")
        {
            // Although the MMI assembly has async versions, they do not return normal Task
            // objects (they're "Observables"), so they can't be awaited as you'd expect.
            using var session = CimSession.Create("localhost");
            return session.QueryInstances(namespc, "WQL", query).ToList();
        }

        /// <summary>
        /// Corresponds to BatteryData.BatteryStatus and Win32_Battery BatteryStatus. In practice,
        /// it appears only values 1 and 2 are used (Discharging and AC Power).
        /// </summary>
        public static readonly Dictionary<int, string> BatteryStatusValues = new()
        {
            { 0, "CIM Query Failed" },
            { 1, "Discharging" },   // officially "Other"
            { 2, "AC Power" },      // officially "Unknown"
            { 3, "Fully Charged" },
            { 4, "Low" },
            { 5, "Critical" },
            { 6, "Charging" },
            { 7, "Charging / High" },
            { 8, "Charging / Low" },
            { 9, "Charging / Critical" },
            { 10, "Undefined / Not Installed" },
            { 11, "Partially Charged" },
        };
    }
}
