
using Microsoft.Extensions.Options;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net.Mail;
using System.Text;

namespace UPSMonitorService
{
    public class Notify
    {
        private readonly Config config = null;

        private static readonly string pipeServerName = "UPSMonitor";
        private static readonly string separatorControlCode = "\u0014";
        private static readonly int pipeTimeoutMS = 100; // high, 10 is probably OK for local machine...

        public Notify(Config appConfig)
        {
            config = appConfig;
        }

        /// <summary>
        /// Sends a message and details to all configured notification channels.
        /// </summary>
        public void Send(EventLogEntryType eventType, string message, string details = "")
        {
            // popup has limited space, don't prefix the eventType
            SendPopup(message, details);

            var flaggedMessage = $"{eventType.ToString().ToUpper()} - {message}";
            SendConsole(flaggedMessage, details);
            SendEventLog(eventType, flaggedMessage, details);
            SendEmail(flaggedMessage, details);
        }

        /// <summary>
        /// Sends a message and details to the Windows Application Event Log, if enabled.
        /// </summary>
        public void SendEventLog(EventLogEntryType eventType, string message, string details = "")
        {
            if (!config.Settings.NotificationEventLog) return;

            EventLog.WriteEntry("Application", $"UPSMonitor\n{message}\n{details}", eventType, 9001);
        }

        /// <summary>
        /// Sends a message and details via Email, if enabled.
        /// </summary>
        public void SendEmail(string message, string details = "")
        {
            // MS says don't use this mail client...
            // https://github.com/dotnet/platform-compat/blob/master/docs/DE0005.md
            // GMail error - no STARTTLS issued
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
                email.Body = $"UPSMonitor update from {Environment.MachineName} at {DateTimeOffset.Now}\n\n{message}\n{details}";

                var smtp = new SmtpClient(config.Email.MailServerDomain, config.Email.MailServerPort);
                smtp.EnableSsl = config.Email.UseTLS;
                smtp.Credentials = config.Email;
                smtp.Send(email);
            }
            catch (Exception ex)
            {
                config.Settings.NotificationEmails = false;
                var msg = "Email failed, disabled until next restart.";
                SendEventLog(EventLogEntryType.Error, msg, ex.Message);
                if (config.Settings.NotificationPopups) SendPopup(msg, ex.Message);
                Console.WriteLine($"\nWARNING: {msg}\n{ex.Message}\n{ex.InnerException?.Message}\n");
            }
        }

        /// <summary>
        /// Sends a message and details via Windows "toast" pop-up, if enabled. When running
        /// non-interactively (e.g. as a Windows Service), this will send a Named Pipes message
        /// to the UPSMonitor system tray service.
        /// </summary>
        public void SendPopup(string message, string details = "")
        {
            if (!config.Settings.NotificationPopups) return;

            if(Environment.UserInteractive && !config.Settings.RemotePopupOnly)
            {
                new ToastContentBuilder()
                    .AddText(message)
                    .AddText(details)
                    .Show();
            }
            else
            {
                SendNamedPipePopup(message, details);
            }
        }

        /// <summary>
        /// Sends a message and details to the Console when running interactively.
        /// </summary>
        public void SendConsole(string message, string details = "")
        {
            if (!Environment.UserInteractive) return;

            if(!string.IsNullOrEmpty(details))
            {
                Console.WriteLine($"{message}\n{details}");
            }
            else
            {
                Console.WriteLine(message);
            }
        }

        /// <summary>
        /// Sends a message and details via Named Pipe to the UPSMonitor system tray service.
        /// </summary>
        public void SendNamedPipePopup(string message, string details = "")
        {
            // TODO: Add no-popup-flag for silently logging service start/stop to system tray history

            using (var client = new NamedPipeClientStream(".", pipeServerName, PipeDirection.Out))
            {
                // try to connect to the server
                try
                {
                    client.Connect(pipeTimeoutMS);
                }
                catch (TimeoutException)
                {
                    Console.WriteLine("Popup notification not sent; UPSMonitor system tray application not listening?");
                    return;
                }

                // TODO: use commented-out async WriteString
                // write the message
                var msg = (string.IsNullOrEmpty(details)) 
                    ? message 
                    : $"{message}{separatorControlCode}{details}";
                try
                {
                    var messageBuffer = Encoding.ASCII.GetBytes(msg);
                    var sizeBuffer = BitConverter.GetBytes(messageBuffer.Length);
                    client.Write(sizeBuffer, 0, sizeBuffer.Length);
                    client.Write(messageBuffer, 0, messageBuffer.Length);
                    client.WaitForPipeDrain();
                }
                catch (Exception ex)
                {
                    //Output(LogLevel.Warning, $"{ex.GetType().Name} while writing stream");
                }
            }
        }

        //private static async Task WriteString(PipeStream stream, string message)
        //{
        //    try
        //    {
        //        var messageBuffer = Encoding.ASCII.GetBytes(message);

        //        var sizeBuffer = BitConverter.GetBytes(messageBuffer.Length);
        //        await stream.WriteAsync(sizeBuffer, 0, sizeBuffer.Length);

        //        if (message.Length > 0)
        //            await stream.WriteAsync(messageBuffer, 0, messageBuffer.Length);

        //        stream.WaitForPipeDrain();
        //    }
        //    catch (Exception ex)
        //    {
        //        //Output(LogLevel.Warning, $"{ex.GetType().Name} while writing stream");
        //    }
        //}
    }
}
