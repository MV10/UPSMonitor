namespace UPSMonitor
{
    internal static class Program
    {
        public static MessageBuffer MessageHistory = new();

        // the Windows Service connects to this
        internal static readonly string PipeServerName = "UPSMonitor";

        // optional; separates the pop-up title line (in boldface) from the detail text
        internal static readonly string TitleSeparator = "~";

        // a message prefixed with this is only stored to history
        internal static readonly string NoPopupPrefix = "@";

        private static SystemTrayApp trayApp = null;
        private static CancellationTokenSource ctsMessageServer = new();

        [STAThread] 
        static void Main() 
            => MainAsync().GetAwaiter().GetResult();

        static async Task MainAsync()
        {
            MessageStorage.ReadHistory();
            MessageHistory.Enqueue(new()
            {
                Content = "UPSMonitor system tray app starting"
            });
            MessageStorage.WriteHistory();

            List<Task> tasks = new()
            {
                // start the UI
                Task.Run(() =>
                {
                    ApplicationConfiguration.Initialize();
                    trayApp = new SystemTrayApp();
                    Application.Run(trayApp);
                }),

                // start the named pipe message server
                Task.Run(async Task () =>
                {
                    await MessageServer.RunServer(ctsMessageServer.Token).ConfigureAwait(false);
                })
            };

            // spin
            await Task.WhenAll(tasks);

            // goodbye
            Environment.Exit(0);
        }

        public static void Exit()
        {
            MessageHistory.Enqueue(new()
            {
                Content = "UPSMonitor system tray app exiting"
            });
            MessageStorage.WriteHistory();

            ctsMessageServer.Cancel();
            trayApp.Dispose();
            Application.Exit();
        }
    }
}
