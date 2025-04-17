using System.Diagnostics;
using WpfDevKit.DependencyInjection;

namespace WpfDevKit.Factory
{
    /// <summary>
    /// Provides extension methods for registering WpfDevKit core services.
    /// </summary>
    [DebuggerStepThrough]
    public static class ObjectFactoryExtensions
    {
        /// <summary>
        /// Registers the object factory.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <returns>The current IServiceCollection instance for chaining.</returns>
        public static IServiceCollection AddObjectFactory(this IServiceCollection services) => 
            services.AddSingleton<IObjectFactory>(p => new ObjectFactory(p as IObjectResolver));
    }
}
