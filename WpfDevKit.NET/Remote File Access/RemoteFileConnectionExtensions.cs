using WpfDevKit.DependencyInjection;

namespace WpfDevKit.RemoteFileAccess
{
    /// <summary>
    /// Provides extension methods for registering WpfDevKit core services.
    /// </summary>
    public static class RemoteFileConnectionExtensions
    {
        /// <summary>
        /// Registers the remote file connection factory.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <returns>The current IServiceCollection instance for chaining.</returns>
        public static IServiceCollection AddRemoteFileConnectionFactory(this IServiceCollection services) => 
            services.AddSingleton<IRemoteFileConnectionFactory, RemoteFileConnectionFactory>();
    }
}
