namespace UPSMonitor
{
    internal static class Program
    {
        public static MessageBuffer MessageHistory = new();

        internal static readonly string PipeServerName = "UPSMonitor";
        internal static readonly string SeparatorControlCode = "\u0014";
        internal static readonly string NoPopupControlCode = "\u0020";

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
