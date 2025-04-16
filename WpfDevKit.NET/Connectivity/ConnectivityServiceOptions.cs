using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WpfDevKit.Connectivity
{
    /// <summary>
    /// Represents the options for configuring the connectivity service, including retry logic, status messages, and a readiness check.
    /// </summary>
    [DebuggerStepThrough]
    public class ConnectivityServiceOptions
    {
        private int minimum = 250;
        private int maximum = 60000;
        private int interval = 60000;

        /// <summary>
        /// Gets or sets the host name or address to which the connectivity service will connect.
        /// Default is "localhost".
        /// </summary>
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// Gets or sets the minimum retry interval (in milliseconds) between connection attempts.
        /// This value cannot be less than 0 or greater than the <see cref="MaximumRetryMilliseconds"/>.
        /// </summary>
        public int MinimumRetryMilliseconds
        {
            get => minimum;
            set => minimum = Math.Min(MaximumRetryMilliseconds, Math.Max(250, value));
        }

        /// <summary>
        /// Gets or sets the maximum retry interval (in milliseconds) between connection attempts.
        /// This value cannot be less than the <see cref="MinimumRetryMilliseconds"/>.
        /// </summary>
        public int MaximumRetryMilliseconds
        {
            get => maximum;
            set => maximum = Math.Min(int.MaxValue, Math.Max(MinimumRetryMilliseconds, value));
        }

        /// <summary>
        /// Gets or sets the standard interval (in milliseconds) between successful executions and is used when no retry is needed.
        /// This value cannot be less than the <see cref="MinimumRetryMilliseconds"/> nor can it be larger than the <see cref="MaximumRetryMilliseconds"/> .
        /// </summary>
        public int ExecutionIntervalMilliseconds
        {
            get => interval;
            set => interval = Math.Min(MaximumRetryMilliseconds, Math.Max(MinimumRetryMilliseconds, value));
        }

        /// <summary>
        /// Gets or sets a function that performs an asynchronous check to determine if the connectivity service is ready.
        /// The function should accept a <see cref="CancellationToken"/> and return a <see cref="Task{bool}"/> indicating readiness.
        /// </summary>
        public Func<CancellationToken, Task<bool>> ConnectionValidationFunctionAsync { get; set; } = token => Task.FromResult(false);

        /// <summary>
        /// 
        /// </summary>
        public Func<IConnectivityService, string> GetStatusMessage { get; set; } = 
            x => x.IsConnecting ? "connecting" :
            x.IsConnected ? "connected" :
            $"retrying in {DateTime.Now.Subtract(x.NextAttempt).ToReadableTime()} after [{x.Attempts}] failed attempts";
    }
}
