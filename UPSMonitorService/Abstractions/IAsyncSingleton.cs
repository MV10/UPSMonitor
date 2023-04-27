namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Any DI-registered singleton-lifetime service implementing
    /// this needs to be initialized before the host is started.
    /// </summary>
    internal interface IAsyncSingleton
    {
        public Task InitializeAsync();
    }
}
