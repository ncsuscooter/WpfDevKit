using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WpfDevKit.Logging;

namespace WpfDevKit.Hosting
{
    /// <summary>
    /// Provides an application host for managing and running services.
    /// </summary>
    //[DebuggerStepThrough]
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
                    Debug.WriteLine($"[HOST] HostedService '{service.GetType().FullName}' starting.");
                    await service.StartAsync(cancellationToken);
                    Debug.WriteLine($"[HOST] HostedService '{service.GetType().FullName}' started.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[HOST] HostedService '{service.GetType().FullName}' failed to start.");
                    Services.GetService<ILogService>()?.LogTrace($"[Host] StartAsync failed", default, GetType());
                    Services.GetService<ILogService>()?.LogTrace(ex, GetType());
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
                    Debug.WriteLine($"[HOST] HostedService '{service.GetType().FullName}' stopping.");
                    await service.StopAsync(cancellationToken);
                    Debug.WriteLine($"[HOST] HostedService '{service.GetType().FullName}' stopped.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[HOST] HostedService '{service.GetType().FullName}' failed to stop.");
                    Services.GetService<ILogService>()?.LogTrace($"[Host] StopAsync failed", default, GetType());
                    Services.GetService<ILogService>()?.LogTrace(ex, GetType());
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            Task.Run(() => StopAsync(new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token)).GetAwaiter().GetResult();
            var backgroundServices = Services.GetServices<IHostedService>();
            foreach (var service in backgroundServices)
            {
                try
                {
                    Debug.WriteLine($"[HOST] HostedService '{service.GetType().FullName}' disposing.");
                    service.Dispose();
                    Debug.WriteLine($"[HOST] HostedService '{service.GetType().FullName}' disposed.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[HOST] HostedService '{service.GetType().FullName}' failed to dispose.");
                    Services.GetService<ILogService>()?.LogTrace($"[Host] Dispose failed", default, GetType());
                    Services.GetService<ILogService>()?.LogTrace(ex, GetType());
                }
            }
            if (Services is IDisposable disposable)
                disposable.Dispose();
        }
    }
}
