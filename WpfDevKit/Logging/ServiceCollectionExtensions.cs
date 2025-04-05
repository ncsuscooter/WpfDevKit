using WpfDevKit.DependencyInjection;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Provides extension methods for registering WpfDevKit core services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
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
            services.AddSingleton<LogBackgroundService>(provider =>
            {
                return new LogBackgroundService(provider.GetService<ILogService>(),
                                                provider.GetService<ILogProviderCollection>(),
                                                provider.GetService<LogQueue>());
            });
            return services;
        }
        //public static IServiceProvider AddLogProvider<TOptions>(this IServiceProvider provider, ILogProvider logProvider, Action<TOptions> configure, string key = null)
        //{
        //    var collection = provider.GetService<ILogProviderCollection>();
        //    collection.TryAddProvider(logProvider, key);

        //}
    }
}
