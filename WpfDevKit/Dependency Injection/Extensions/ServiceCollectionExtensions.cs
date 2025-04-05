using System;
using WpfDevKit.DependencyInjection.Interfaces;
using WpfDevKit.Hosting.Interfaces;
using WpfDevKit.Interfaces;
using WpfDevKit.Logging;
using WpfDevKit.Logging.Interfaces;
using WpfDevKit.Logging.Providers;
using WpfDevKit.Logging.Providers.Options;
using WpfDevKit.Services;
using WpfDevKit.Services.Options;

namespace WpfDevKit.DependencyInjection.Extensions
{
    /// <summary>
    /// Provides extension methods for registering WpfDevKit core services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the busy service.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <returns>The current IServiceCollection instance for chaining.</returns>
        public static IServiceCollection AddBusyService(this IServiceCollection services) => services.AddSingleton<IBusyService, BusyService>();

        /// <summary>
        /// Registers the connectivity monitoring service.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <returns>The current IServiceCollection instance for chaining.</returns>
        public static IServiceCollection AddConnectivityService(this IServiceCollection services, Action<ConnectivityServiceOptions> configure)
        {
            services.AddOptions(configure);
            services.AddSingleton<IConnectivityService>(provider =>
            {
                return new ConnectivityService(provider.GetService<IOptions<ConnectivityServiceOptions>>().Value);
            });
            return services;
        }

        /// <summary>
        /// Registers logging services.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <returns>The current IServiceCollection instance for chaining.</returns>
        public static IServiceCollection AddLoggingService(this IServiceCollection services)
        {
            services.AddOptions<MemoryLogProviderOptions>();
            services.AddOptions<ConsoleLogProviderOptions>();
            services.AddOptions<UserLogProviderOptions>();
            services.AddSingleton<LogQueue, LogQueue>();
            services.AddSingleton<ILogProviderCollection>(provider =>
            {
                var collection = new LogProviderCollection(provider.GetService<ILogService>());
                collection.TryAddProvider(new MemoryLogProvider(provider.GetService<IOptions<MemoryLogProviderOptions>>().Value));
                collection.TryAddProvider(new ConsoleLogProvider(provider.GetService<IOptions<ConsoleLogProviderOptions>>().Value));
                collection.TryAddProvider(new UserLogProvider(provider.GetService<IOptions<UserLogProviderOptions>>().Value));
                return collection;
            });
            services.AddSingleton<ILogService>(provider =>
            {
                return new LogService(provider.GetService<LogQueue>());
            });
            services.AddSingleton<IBackgroundService>(provider =>
            {
                return new LogBackgroundService(provider.GetService<ILogService>(),
                                                provider.GetService<ILogProviderCollection>(),
                                                provider.GetService<LogQueue>());
            });
            return services;
        }
    }
}
