﻿using System;
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
        private readonly AsyncAutoResetEvent reset = new AsyncAutoResetEvent();
        private readonly ConnectivityServiceOptions options;

        /// <inheritdoc/>
        public event EventHandler StatusChanged;

        /// <inheritdoc/>
        public bool IsConnecting { get; private set; }

        /// <inheritdoc/>
        public bool IsConnected { get; private set; }

        /// <inheritdoc/>
        public int Attempts { get; private set; }

        /// <inheritdoc/>
        public Exception Error { get; private set; }

        /// <inheritdoc/>
        public DateTime NextAttempt { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectivityService"/> class.
        /// </summary>
        /// <param name="options">The options used to configure the connectivity service.</param>
        public ConnectivityService(ConnectivityServiceOptions options) => this.options = options ?? throw new ArgumentNullException(nameof(options));

        /// <inheritdoc/>
        public void TriggerImmediateExecution() => reset.Signal();

        /// <inheritdoc/>
        public string GetStatus() => options.GetStatusMessage(this);

        /// <inheritdoc/>
        /// <remarks>
        /// Monitors the connectivity status asynchronously, with the option to cancel the operation.
        /// This method allows ongoing monitoring of the system's connection status.
        /// </remarks>
        protected override Task ExecuteAsync(CancellationToken cancellationToken) => Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Error = null;
                IsConnecting = true;
                RaiseStatusChanged();
                try
                {
                    IsConnected = await options.ConnectionValidationFunctionAsync(cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Error = ex;
                    IsConnected = false;
                }
                IsConnecting = false;
                Attempts = IsConnected ? 0 : Attempts + 1;
                var delay = Attempts > 0
                            ? ExponentialRetry.CalculateDelay(Attempts, options.MinimumRetryMilliseconds, options.MaximumRetryMilliseconds)
                            : TimeSpan.FromMilliseconds(options.ExecutionIntervalMilliseconds);
                NextAttempt = DateTime.UtcNow + delay;
                RaiseStatusChanged();
                await Task.WhenAny(Task.Delay(delay, cancellationToken), reset.WaitAsync(cancellationToken));
            }
        }, cancellationToken);

        private void RaiseStatusChanged() => StatusChanged?.Invoke(this, EventArgs.Empty);
    }
}
