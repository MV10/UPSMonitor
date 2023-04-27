namespace UPSMonitorService.Models
{
    public class ChargeLevelConfig
    {
        /// <summary>
        /// Battery reserve in percentage representing a no-action-needed notification level. Default is 30.
        /// </summary>
        public int Advisory { get; set; } = 30;

        /// <summary>
        /// Battery reserve in percentage representing first warning level. Default is 11.
        /// Typically this should be set to 1% more than the Power Settings value by the same name,
        /// which allows notifications to be sent before a configured Power Settings action takes
        /// place (such as hibernation or shutdown).
        /// </summary>
        public int Low { get; set; } = 11;

        /// <summary>
        /// Battery reserve in percentage representing second warning level. Default is 8.
        /// Typically this should be set to 1% more than the Power Settings value by the same name,
        /// which allows notifications to be sent before a configured Power Settings action takes
        /// place (such as hibernation or shutdown).
        /// </summary>
        public int Reserve { get; set; } = 8;

        /// <summary>
        /// Battery reserve in percentage representing final warning level. Default is 6.
        /// Typically this should be set to 1% more than the Power Settings value by the same name,
        /// which allows notifications to be sent before a configured Power Settings action takes
        /// place (such as hibernation or shutdown).
        /// </summary>
        public int Critical { get; set; } = 9;
    }
}
