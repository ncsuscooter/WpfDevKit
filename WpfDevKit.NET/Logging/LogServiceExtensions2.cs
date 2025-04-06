using WpfDevKit.DependencyInjection;

namespace WpfDevKit.Logging
{
    public static partial class LogServiceExtensions
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
    }
}
