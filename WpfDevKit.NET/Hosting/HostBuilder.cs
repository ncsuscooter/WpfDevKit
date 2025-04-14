using WpfDevKit.Busy;
using WpfDevKit.DependencyInjection;
using WpfDevKit.Logging;

namespace WpfDevKit.Hosting
{
    /// <summary>
    /// Provides functionality to configure services and build a service host.
    /// </summary>
    public class HostBuilder
    {
        private readonly IServiceCollection services = new ServiceCollection();

        /// <summary>
        /// Gets the collection of registered service descriptors.
        /// </summary>
        public IServiceCollection Services => services;

        /// <summary>
        /// 
        /// </summary>
        public HostBuilder() => Services.AddLoggingService().AddBusyService();

        /// <summary>
        /// Builds the service host and returns a new <see cref="IHost"/> instance.
        /// </summary>
        /// <returns>A configured <see cref="IHost"/> instance.</returns>
        public IHost Build() => new Host(services.Build());
    }
}
