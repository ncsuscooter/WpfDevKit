using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WpfDevKit.Hosting;

namespace WpfDevKit.Connectivity
{
    /// <summary>
    /// Provides a service for monitoring the connectivity status of a system.
    /// This service checks the connectivity status at regular intervals and raises an event when the status changes.
    /// </summary>
    [DebuggerStepThrough]
    internal class ConnectivityService : HostedService, IConnectivityService
    {
        private readonly ConnectivityServiceOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectivityService"/> class.
        /// </summary>
        /// <param name="options">The options used to configure the connectivity service.</param>
        public ConnectivityService(ConnectivityServiceOptions options) => this.options = options;

        /// <inheritdoc/>
        public event Action ConnectionChanged;

        /// <inheritdoc/>
        public bool IsConnected { get; private set; }

        /// <inheritdoc/>
        public string Status { get; private set; }

        /// <inheritdoc/>
        public string Host => options.Host;

        /// <inheritdoc/>
        public void OnConnectionChanged() => ConnectionChanged?.Invoke();

        /// <inheritdoc/>
        /// <remarks>
        /// Monitors the connectivity status asynchronously, with the option to cancel the operation.
        /// This method allows ongoing monitoring of the system's connection status.
        /// </remarks>
        protected override Task ExecuteAsync(CancellationToken cancellationToken) => ExponentialRetry.ExecuteAsync(async status =>
        {
            try
            {
                // Set the status to "Connecting"
                Status = options.ConnectingStatusMessage;
                OnConnectionChanged();

                // Check if the system is ready (connected)
                IsConnected = await options.IsReadyAsync(cancellationToken);
            }
            catch
            {
                // INTENTIONALLY LEFT EMPTY - no handling of exceptions to retry
            }
            finally
            {
                // Update the status based on the connection state
                Status = IsConnected ? options.ConnectedStatusMessage : status.ToLower();
                OnConnectionChanged();
            }

            return IsConnected;
        }, options.MiniumumRetryMilliseconds, options.MaxiumumRetryMilliseconds, cancellationToken);
    }
}
