using System.Diagnostics;
using WpfDevKit.DependencyInjection;

namespace WpfDevKit.UI.CollectionView
{
    /// <summary>
    /// Provides extension methods for registering WpfDevKit UI services.
    /// </summary>
    [DebuggerStepThrough]
    public static class CollectionViewExtensions
    {
        /// <summary>
        /// Registers the <see cref="ICollectionViewService"/> service.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <returns>The current IServiceCollection instance for chaining.</returns>
        public static IServiceCollection AddCollectionViewService(this IServiceCollection services) =>
            services.AddSingleton<CollectionViewService>()
                    .AddSingleton<ICollectionViewService>(p => p.GetRequiredService<CollectionViewService>());
    }
}
