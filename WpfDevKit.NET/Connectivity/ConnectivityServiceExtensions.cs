using System;
using WpfDevKit.DependencyInjection;

namespace WpfDevKit.Connectivity
{
    /// <summary>
    /// Provides extension methods for registering WpfDevKit core services.
    /// </summary>
    public static class ConnectivityServiceExtensions
    {
        /// <summary>
        /// Registers the connectivity monitoring service.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <returns>The current IServiceCollection instance for chaining.</returns>
        public static IServiceCollection AddConnectivityService(this IServiceCollection services, Action<ConnectivityServiceOptions> configure)
        {
            services.AddOptions(configure);
            services.AddSingleton<IConnectivityService, ConnectivityService>();
            return services;
        }
    }
}
