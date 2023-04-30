
using Microsoft.Toolkit.Uwp.Notifications;
using System.Diagnostics;
using System.IO.Pipes;
using System.Net.Mail;
using System.Text;

namespace UPSMonitorService
{
    public class Notify
    {
        private readonly Config config = null;

        // provided by UPS Monitor system tray app
        private static readonly string PipeServerName = "UPSMonitor";

        // optional; separates the pop-up title line (in boldface) from the detail text
        private static readonly string TitleSeparator = "~";

        // a message prefixed with this is only stored to history
        internal static readonly string NoPopupPrefix = "@";

        // high, 10 is probably OK for local machine...
        private static readonly int pipeTimeoutMS = 100; 

        public Notify(Config appConfig)
        {
            config = appConfig;
        }

        /// <summary>
        /// Sends a message to all configured notification channels. For pop-ups, the
        /// details field may contain four lines separated by \n newline characters. If
        /// only a title is provided, for pop-up purposes it will be shown as the detail
        /// text and a title of "Message" will be shown. (Pop-up titles are in boldface.)
        /// </summary>
        public async Task Send(EventLogEntryType eventType, string title, string details = "")
        {
            // popup has limited space, don't prefix the eventType
            await SendPopup(title, details);

            // for everything else, prefix the eventType (Information, Warning, Error)
            var flaggedMessage = $"{eventType.ToString().ToUpper()} - {title}";

            SendConsole(flaggedMessage, details);

            SendEventLog(eventType, flaggedMessage, details);

            await SendEmail(flaggedMessage, details);
        }

        /// <summary>
        /// Sends a message to the Windows Application Event Log, if enabled.
        /// </summary>
        public void SendEventLog(EventLogEntryType eventType, string title, string details = "")
        {
            if (!config.Settings.NotificationEventLog) return;

            Task.Run(() =>
                EventLog.WriteEntry("Application", $"UPSMonitor\n{title}\n{details}", eventType, 9001)
                ).FireAndForget();
        }

        /// <summary>
        /// Sends a message via email, if enabled.
        /// </summary>
        public async Task SendEmail(string title, string details = "")
        {
            // MS says don't use this mail client...
            // https://github.com/dotnet/platform-compat/blob/master/docs/DE0005.md
            // MS recommends this:
            // https://github.com/jstedfast/MailKit

            if (!config.Settings.NotificationEmails) return;

            try
            {
                using var email = new MailMessage();
                email.From = new MailAddress(config.Email.SenderName, "UPSMonitor");
                var recipients = config.Email.RecipientList.Split(',');
                foreach (var addr in recipients) email.To.Add(addr);
                email.Subject = config.Email.Subject;
                email.Body = $"UPSMonitor update from {Environment.MachineName} at {DateTimeOffset.Now}\n\n{title}\n{details}";

                using var smtp = new SmtpClient(config.Email.MailServerDomain, config.Email.MailServerPort);
                smtp.EnableSsl = config.Email.UseTLS;
                smtp.Credentials = config.Email;

                await smtp.SendMailAsync(email);
            }
            catch (Exception ex)
            {
                config.Settings.NotificationEmails = false;
                var msg = "Email failed, disabled until next restart.";
                SendEventLog(EventLogEntryType.Error, msg, ex.Message);
                if (config.Settings.NotificationPopups) await SendPopup(msg, ex.Message);
                Console.WriteLine($"\nWARNING: {msg}\n{ex.Message}\n{ex.InnerException?.Message}\n");
            }
        }

        /// <summary>
        /// Sends a message shown as a Windows "toast" pop-up, if enabled. When running
        /// non-interactively (e.g. as a Windows Service), this will send a Named Pipes message
        /// to the UPSMonitor system tray service.
        /// </summary>
        public async Task SendPopup(string title, string details = "")
        {
            if (!config.Settings.NotificationPopups) return;

            if(Environment.UserInteractive && !config.Settings.RemotePopupOnly)
            {
                new ToastContentBuilder()
                    .AddText(title)
                    .AddText(details)
                    .Show();
            }
            else
            {
                await SendNamedPipePopup(title, details);
            }
        }

        /// <summary>
        /// Sends a message to the Console when running interactively.
        /// </summary>
        public void SendConsole(string title, string details = "")
        {
            if (!Environment.UserInteractive) return;

            if(!string.IsNullOrEmpty(details))
            {
                Console.WriteLine($"{title}\n{details}");
            }
            else
            {
                Console.WriteLine(title);
            }
        }

        /// <summary>
        /// Sends a message and details via Named Pipe to the UPSMonitor system tray service.
        /// </summary>
        public async Task SendNamedPipePopup(string title, string details = "", bool noPopUp = false)
        {
            using var client = new NamedPipeClientStream(".", PipeServerName, PipeDirection.Out);

            // try to connect to the server
            try
            {
                await client.ConnectAsync(pipeTimeoutMS);
            }
            catch (TimeoutException)
            {
                Console.WriteLine("Popup notification not sent; UPSMonitor system tray application not listening?");
                return;
            }

            // build the message
            var popupFlag = noPopUp ? NoPopupPrefix: string.Empty;

            var content = string.IsNullOrEmpty(details) 
                ? $"{popupFlag}{title}"
                : $"{popupFlag}{title}{TitleSeparator}{details}";

            // send it!
            await WriteString(client, content);
        }

        private async Task WriteString(PipeStream stream, string content)
        {
            try
            {
                var messageBuffer = Encoding.ASCII.GetBytes(content);

                var sizeBuffer = BitConverter.GetBytes(messageBuffer.Length);
                await stream.WriteAsync(sizeBuffer, 0, sizeBuffer.Length);

                if (content.Length > 0)
                    await stream.WriteAsync(messageBuffer, 0, messageBuffer.Length);

                stream.WaitForPipeDrain();
            }
            catch
            { }
        }
    }
}
