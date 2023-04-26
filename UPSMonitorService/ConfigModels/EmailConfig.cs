using System.Net;

// TODO: Add email throttle?

namespace UPSMonitorService.Models
{
    public class EmailConfig : ICredentialsByHost
    {
        /// <summary>
        /// The outgoing (SMTP) server domain name. Defaults to GMail. Currently
        /// GMail allows up to 100 emails in any rolling 24-hour period.
        /// </summary>
        public string MailServerDomain { get; set; } = "smtp.gmail.com";

        /// <summary>
        /// The outgoing (SMTP) server port. Typically 465 or 587 for secure
        /// servers. Defaults to 587. Insecure servers are 25 but these are rare.
        /// </summary>
        public int MailServerPort { get; set; } = 587;

        /// <summary>
        /// Whether to use TLS security. Virtually always required.
        /// </summary>
        public bool UseTLS { get; set; } = true;

        /// <summary>
        /// Username portion of SMTP credentials.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Password portion of SMTP credentials.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// The sender address ("From" field) on the email. GMail may require a recognized
        /// email address which matches the Username. The sender display name is always
        /// set to UPSMonitor.
        /// </summary>
        public string SenderName { get; set; } = string.Empty;

        /// <summary>
        /// The subject line on the email message. GMail requires a subject. An asterisk
        /// is replaced with the machine name.
        /// </summary>
        public string Subject { get; set; } = "UPS Notification from *";

        /// <summary>
        /// Comma-separated list of recipient emails. Most mobile phone companies also
        /// provide SMS gateway email addresses, such as xxxxxxxxxx@tmomail.net for
        /// T-Mobile subscribers.
        /// </summary>
        public string RecipientList { get; set; } = string.Empty;

        /// <summary>
        /// How username/password is provided to the .NET SMTP object.
        /// </summary>
        public NetworkCredential GetCredential(string host, int port, string authenticationType)
            => new(Username, Password);
    }
}
