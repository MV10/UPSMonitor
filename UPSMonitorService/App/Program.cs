namespace UPSMonitorService.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)

                // Not clearly documented, but after .NET 6 this turns
                // into AddWindowsService for the services collection
                .UseWindowsService(opt =>
                {
                    opt.ServiceName = "UPSMonitor";
                })

                .ConfigureServices(services =>
                {
                    services.AddSingleton<Config>();
                    services.AddSingleton<Notify>();
                    services.AddSingleton<BatteryState>();
                    services.AddHostedService<ProcessingLoop>();
                })
                .Build();

            // TODO: Async everywhere (email is slow)
            host.Run();
        }
    }
}
