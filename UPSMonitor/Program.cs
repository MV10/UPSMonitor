using System.Diagnostics;

namespace UPSMonitor
{
    internal static class Program
    {
        public static MessageBuffer MessageHistory = new();

        internal static readonly string PipeServerName = "UPSMonitor";
        internal static readonly string SeparatorControlCode = "\u0014";

        private static SystemTrayApp trayApp = null;
        private static CancellationTokenSource ctsMessageServer = new();

        [STAThread] 
        static void Main() 
            => MainAsync().GetAwaiter().GetResult();

        static async Task MainAsync()
        {
            List<Task> tasks = new();

            // start the UI
            tasks.Add(Task.Run(() =>
            {
                ApplicationConfiguration.Initialize();
                trayApp = new SystemTrayApp();
                Application.Run(trayApp);
            }));

            // start the named pipe message server
            tasks.Add(Task.Run(async Task () =>
            {
                Debug.WriteLine("Starting message server");
                await MessageServer.RunServer(ctsMessageServer.Token).ConfigureAwait(false);
                Debug.WriteLine("Exiting message server");
            }));

            // spin
            Debug.WriteLine("Awaiting UI and message server tasks");
            await Task.WhenAll(tasks);

            // goodbye
            Debug.WriteLine("Terminating process");
            Environment.Exit(0);
        }

        public static void Exit()
        {
            ctsMessageServer.Cancel();
            trayApp.Dispose();
            Application.Exit();
        }

    }
}
