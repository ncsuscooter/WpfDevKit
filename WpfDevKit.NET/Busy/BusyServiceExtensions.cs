using System.Diagnostics;
using WpfDevKit.DependencyInjection;

namespace WpfDevKit.Busy
{
    /// <summary>
    /// Provides extension methods for registering WpfDevKit core services.
    /// </summary>
    [DebuggerStepThrough]
    public static class BusyServiceExtensions
    {
        /// <summary>
        /// Registers the busy service.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <returns>The current IServiceCollection instance for chaining.</returns>
        public static IServiceCollection AddBusyService(this IServiceCollection services) => 
            services.AddSingleton<BusyService>()
                    .AddSingleton<IBusyService>(p => p.GetRequiredService<BusyService>());
    }
}
