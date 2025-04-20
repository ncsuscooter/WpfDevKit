using System.Diagnostics;
using WpfDevKit.DependencyInjection;

namespace WpfDevKit.UI.CollectionSynchronization
{
    /// <summary>
    /// Provides extension methods for registering WpfDevKit UI services.
    /// </summary>
    [DebuggerStepThrough]
    public static class CollectionSynchronizationExtensions
    {
        /// <summary>
        /// Registers the <see cref="ICollectionSynchronizationService"/> service.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <returns>The current IServiceCollection instance for chaining.</returns>
        public static IServiceCollection AddCollectionSynchronization(this IServiceCollection services) =>
            services.AddSingleton<CollectionSynchronizationService>()
                    .AddSingleton<ICollectionSynchronizationService>(p => p.GetRequiredService<CollectionSynchronizationService>());
    }
}
