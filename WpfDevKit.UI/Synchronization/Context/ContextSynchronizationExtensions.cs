using System.Diagnostics;
using WpfDevKit.DependencyInjection;

namespace WpfDevKit.UI.Synchronization.Context
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
        public static IServiceCollection AddContextSynchronization(this IServiceCollection services) =>
            services.AddSingleton<ContextSynchronizationService>()
                    .AddSingleton<IContextSynchronizationService>(p => p.GetRequiredService<ContextSynchronizationService>());
    }
}
