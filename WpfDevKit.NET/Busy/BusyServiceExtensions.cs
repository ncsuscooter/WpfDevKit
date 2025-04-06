using WpfDevKit.DependencyInjection;

namespace WpfDevKit.Busy
{
    /// <summary>
    /// Provides extension methods for registering WpfDevKit core services.
    /// </summary>
    public static class BusyServiceExtensions
    {
        /// <summary>
        /// Registers the busy service.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <returns>The current IServiceCollection instance for chaining.</returns>
        public static IServiceCollection AddBusyService(this IServiceCollection services) => services.AddSingleton<IBusyService, BusyService>();
    }
}
