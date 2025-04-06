using System.Diagnostics;
using WpfDevKit.DependencyInjection;

namespace WpfDevKit.UI.ContextSynchronization
{
    /// <summary>
    /// Provides extension methods for registering WpfDevKit UI services.
    /// </summary>
    [DebuggerStepThrough]
    public static class ContextSynchronizationExtensions
    {
        /// <summary>
        /// Registers the <see cref="IContextSynchronizationService"/> service.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <returns>The current IServiceCollection instance for chaining.</returns>
        public static IServiceCollection AddContextSynchronization(this IServiceCollection services)
        {
            services.AddSingleton<IContextSynchronizationService, ContextSynchronizationService>();
            return services;
        }
    }
}
