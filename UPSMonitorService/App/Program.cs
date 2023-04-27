namespace UPSMonitorService.App
{
    public class Program
    {
        public static async Task Main(string[] args)
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

            // Because C# doesn't have async constructors...
            await InitializeAsyncSingleton<BatteryState>(host);
            
            await host.RunAsync();
        }

        // I considered writing an IHost extension method to find implementations of the
        // interface by reflection, but this is already overkill for just one of them.
        private static async Task InitializeAsyncSingleton<ServiceType>(IHost host)
            => await 
            (host.Services
            .GetRequiredService(typeof(ServiceType)) 
            as IAsyncSingleton)
            .InitializeAsync();
    }
}
