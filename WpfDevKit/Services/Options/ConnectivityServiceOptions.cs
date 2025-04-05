using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WpfDevKit.Services.Options
{
    /// <summary>
    /// Represents the options for configuring the connectivity service, including retry logic, status messages, and a readiness check.
    /// </summary>
    [DebuggerStepThrough]
    public class ConnectivityServiceOptions
    {
        private int minimumRetryMilliseconds = 0;
        private int maximumRetryMilliseconds = 30000;

        /// <summary>
        /// Gets or sets the host name or address to which the connectivity service will connect.
        /// Default is "localhost".
        /// </summary>
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// Gets or sets the status message displayed when the service is in the process of connecting.
        /// Default is "connecting".
        /// </summary>
        public string ConnectingStatusMessage { get; set; } = "connecting";

        /// <summary>
        /// Gets or sets the status message displayed when the service is successfully connected.
        /// Default is "connected".
        /// </summary>
        public string ConnectedStatusMessage { get; set; } = "connected";

        /// <summary>
        /// Gets or sets the minimum retry interval (in milliseconds) between connection attempts.
        /// This value cannot be less than 0 or greater than the <see cref="MaxiumumRetryMilliseconds"/>.
        /// </summary>
        public int MiniumumRetryMilliseconds
        {
            get => minimumRetryMilliseconds;
            set => minimumRetryMilliseconds = Math.Min(MaxiumumRetryMilliseconds, Math.Max(0, value));
        }

        /// <summary>
        /// Gets or sets the maximum retry interval (in milliseconds) between connection attempts.
        /// This value cannot be less than the <see cref="MiniumumRetryMilliseconds"/>.
        /// </summary>
        public int MaxiumumRetryMilliseconds
        {
            get => maximumRetryMilliseconds;
            set => maximumRetryMilliseconds = Math.Min(int.MaxValue, Math.Max(MiniumumRetryMilliseconds, value));
        }

        /// <summary>
        /// Gets or sets a function that performs an asynchronous check to determine if the connectivity service is ready.
        /// The function should accept a <see cref="CancellationToken"/> and return a <see cref="Task{bool}"/> indicating readiness.
        /// </summary>
        public Func<CancellationToken, Task<bool>> IsReadyAsync { get; set; }
    }
}
