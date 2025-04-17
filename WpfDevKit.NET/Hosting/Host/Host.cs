using System;
using System.Threading;
using System.Threading.Tasks;

namespace WpfDevKit.Hosting
{
    /// <summary>
    /// Provides an application host for managing and running services.
    /// </summary>
    internal class Host : IHost
    {
        private bool disposed;

        /// <inheritdoc/>
        public IServiceProvider Services { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Host"/> class.
        /// </summary>
        /// <param name="provider">The service provider.</param>
        public Host(IServiceProvider provider) => Services = provider;

        /// <summary>
        /// Starts all registered background services synchronously.
        /// </summary>
        public void Start() => StartAsync().Wait();

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            var backgroundServices = Services.GetServices<IHostedService>();
            foreach (var service in backgroundServices)
            {
                try
                {
                    await service.StartAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    Services.GetService<InternalLogger>()?.LogMessage?.Invoke($"[Host] StartAsync failed", default, GetType());
                    Services.GetService<InternalLogger>()?.LogException?.Invoke(ex, GetType());
                }
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            var backgroundServices = Services.GetServices<IHostedService>();
            foreach (var service in backgroundServices)
            {
                try
                {
                    await service.StopAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    Services.GetService<InternalLogger>()?.LogMessage?.Invoke($"[Host] StopAsync failed", default, GetType());
                    Services.GetService<InternalLogger>()?.LogException?.Invoke(ex, GetType());
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            StopAsync(new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token).GetAwaiter().GetResult();
            var backgroundServices = Services.GetServices<IHostedService>();
            foreach (var service in backgroundServices)
            {
                try
                {
                    service.Dispose();
                }
                catch (Exception ex)
                {
                    Services.GetService<InternalLogger>()?.LogMessage?.Invoke($"[Host] Dispose failed", default, GetType());
                    Services.GetService<InternalLogger>()?.LogException?.Invoke(ex, GetType());
                }
            }
            if (Services is IDisposable disposable)
                disposable.Dispose();
        }
    }
}
