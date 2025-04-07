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
            services.AddSingleton<ILogMetricsFactory, LogMetricsFactory>();
            services.AddSingleton<ILogProviderCollection, LogProviderCollection>();
            services.AddSingleton<LogBackgroundService, LogBackgroundService>();
            services.AddOptions(p => new LogOptions()
            {
                LogMessage = (message, attributes, type) => p.GetService<ILogService>().LogTrace(message, attributes, type),
                LogException = (exception, type) => p.GetService<ILogService>().LogTrace(exception, type)
            });
            return services;
        }
    }
}
