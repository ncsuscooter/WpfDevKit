using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WpfDevKit.DependencyInjection;
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
        public TConnectivityState State { get; private set; }

        /// <inheritdoc/>
        public int Attempts { get; private set; }

        /// <inheritdoc/>
        public Exception Error { get; private set; }

        /// <inheritdoc/>
        public DateTime? LastSuccessfulConnection { get; private set; }

        /// <inheritdoc/>
        public TimeSpan? Uptime => LastSuccessfulConnection.HasValue ? DateTime.UtcNow - LastSuccessfulConnection.Value : default;

        /// <inheritdoc/>
        public DateTime NextAttempt { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectivityService"/> class.
        /// </summary>
        /// <param name="options">The options used to configure the connectivity service.</param>
        public ConnectivityService(IOptions<ConnectivityServiceOptions> options)
        {
            this.options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
            this.options.Validate();
        }

        /// <inheritdoc/>
        public void TriggerImmediateExecution() => reset.Signal();

        /// <inheritdoc/>
        public string GetStatus() => options.GetStatus(this);

        /// <inheritdoc/>
        /// <remarks>
        /// Monitors the connectivity status asynchronously, with the option to cancel the operation.
        /// This method allows ongoing monitoring of the system's connection status.
        /// </remarks>
        protected override Task ExecuteAsync(CancellationToken cancellationToken) => Task.Run(async () =>
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    State = TConnectivityState.Connecting;
                    RaiseStatusChanged();
                    bool connected = false;
                    try
                    {
                        connected = await options.ValidateConnectionAsync(cancellationToken);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Error = ex;
                    }
                    if (connected)
                    {
                        Error = null;
                        Attempts = 0;
                        State = TConnectivityState.Connected;
                        if (!LastSuccessfulConnection.HasValue)
                            LastSuccessfulConnection = DateTime.UtcNow;
                    }
                    else
                    {
                        Attempts++;
                        State = TConnectivityState.Disconnected;
                        LastSuccessfulConnection = null;
                    }
                    var delay = Attempts > 0
                                ? ExponentialRetry.CalculateDelay(Attempts, options.MinimumRetryMilliseconds, options.MaximumRetryMilliseconds, 10, 2.5)
                                : TimeSpan.FromMilliseconds(options.ExecutionIntervalMilliseconds);
                    NextAttempt = DateTime.UtcNow.Add(delay);
                    RaiseStatusChanged();
                    try
                    {
                        await Task.WhenAny(Task.Delay(delay, cancellationToken), reset.WaitAsync(cancellationToken));
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }
            finally
            {
                State = TConnectivityState.Disconnected;
                RaiseStatusChanged();
            }
        }, cancellationToken);

        private void RaiseStatusChanged() => StatusChanged?.Invoke(this, EventArgs.Empty);
    }
}
