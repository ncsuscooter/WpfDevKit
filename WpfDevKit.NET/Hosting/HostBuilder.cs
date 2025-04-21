using System.Diagnostics;
using WpfDevKit.DependencyInjection;

namespace WpfDevKit.Hosting
{
    /// <summary>
    /// Provides functionality to configure services and build a service host.
    /// </summary>
    [DebuggerStepThrough]
    public class HostBuilder
    {
        private readonly IServiceCollection services;

        /// <summary>
        /// Creates and returns a new instance of the <see cref="HostBuilder"/> class.
        /// </summary>
        /// <returns>A configured <see cref="HostBuilder"/> instance.</returns>
        public static HostBuilder CreateHostBuilder() => new HostBuilder();

        /// <summary>
        /// Initializes a new instance of the <see cref="HostBuilder"/> class.
        /// </summary>
        private HostBuilder() => services = new ServiceCollection();

        /// <summary>
        /// Gets the collection of registered service descriptors.
        /// </summary>
        public IServiceCollection Services => services;

        /// <summary>
        /// Builds the service host and returns a new <see cref="IHost"/> instance.
        /// </summary>
        /// <returns>A configured <see cref="IHost"/> instance.</returns>
        public IHost Build() => new Host(services.Build());
    }
}
