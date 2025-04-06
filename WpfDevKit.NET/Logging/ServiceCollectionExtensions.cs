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
            services.AddSingleton<LogQueue, LogQueue>();
            services.AddSingleton<ILogService, LogService>();
            services.AddSingleton<ILogProviderCollection, LogProviderCollection>();
            services.AddSingleton<LogBackgroundService, LogBackgroundService>();
            return services;
        }
        //public static IServiceProvider AddLogProvider<TOptions>(this IServiceProvider provider, ILogProvider logProvider, Action<TOptions> configure, string key = null)
        //{
        //    var collection = provider.GetService<ILogProviderCollection>();
        //    collection.TryAddProvider(logProvider, key);
        //    services.AddOptions<MemoryLogProviderOptions>();
        //    services.AddOptions<ConsoleLogProviderOptions>();
        //    services.AddOptions<UserLogProviderOptions>();
        //    var collection = new LogProviderCollection(provider.GetService<ILogService>());
        //    collection.TryAddProvider(new MemoryLogProvider(provider.GetService<IOptions<MemoryLogProviderOptions>>().Value));
        //    collection.TryAddProvider(new ConsoleLogProvider(provider.GetService<IOptions<ConsoleLogProviderOptions>>().Value));
        //    collection.TryAddProvider(new UserLogProvider(provider.GetService<IOptions<UserLogProviderOptions>>().Value));
        //}
    }
}
