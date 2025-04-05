using System;
using WpfDevKit.DependencyInjection;

namespace WpfDevKit.Hosting
{
    /// <summary>
    /// Provides functionality to configure services and build a service host.
    /// </summary>
    public class ServiceHostBuilder
    {
        private readonly IServiceCollection services = new ServiceCollection();

        /// <summary>
        /// Gets the collection of registered service descriptors.
        /// </summary>
        public IServiceCollection Services => services;

        /// <summary>
        /// Configures services by invoking the specified delegate.
        /// </summary>
        /// <param name="configure">A delegate used to configure services.</param>
        /// <returns>The current <see cref="ServiceHostBuilder"/> instance.</returns>
        public ServiceHostBuilder ConfigureServices(Action<IServiceCollection> configure)
        {
            configure?.Invoke(services);
            return this;
        }

        /// <summary>
        /// Builds the service host and returns a new <see cref="ServiceHost"/> instance.
        /// </summary>
        /// <returns>A configured <see cref="ServiceHost"/> instance.</returns>
        public ServiceHost Build() => new ServiceHost(services.Build());
    }
}
