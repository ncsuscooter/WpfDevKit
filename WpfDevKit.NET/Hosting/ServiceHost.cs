using System;
using System.Threading;
using System.Threading.Tasks;

namespace WpfDevKit.Hosting
{
    /// <summary>
    /// Provides an application host for managing and running services.
    /// </summary>
    public class ServiceHost
    {
        /// <summary>
        /// Gets the application's service provider.
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceHost"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        internal ServiceHost(IServiceProvider serviceProvider) => ServiceProvider = serviceProvider;

        /// <summary>
        /// Runs all registered background services synchronously.
        /// </summary>
        public void Run() => RunAsync().Wait();

        /// <summary>
        /// Runs all registered background services asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous run operation.</returns>
        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            var backgroundServices = ServiceProvider.GetServices<IBackgroundService>();
            foreach (var service in backgroundServices)
                await service.StartAsync(cancellationToken);
        }
    }
}
