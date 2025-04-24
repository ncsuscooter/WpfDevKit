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
            set => minimum = Math.Min(MaximumRetryMilliseconds, Math.Max(1, value));
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
        public Func<CancellationToken, Task<bool>> ValidateConnectionAsync { get; set; } = token => Task.FromResult(false);

        /// <summary>
        /// Gets or sets a function that formats a status message based on the properties provided by the <see cref="IConnectivityService"/>.
        /// </summary>
        public Func<IConnectivityService, string> GetStatus { get; set; } = 
            x => x.State == TConnectivityState.Connecting ? "connecting" :
            x.State == TConnectivityState.Connected ? "connected" :
            $"retrying in {x.NextAttempt.Subtract(DateTime.UtcNow).ToReadableTime()} after [{x.Attempts}] failed attempts";

        /// <summary>
        /// Validates the current configuration and throws an exception if any required properties are missing or invalid.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if required delegates are not provided.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if timing values are invalid or inconsistent.</exception>
        internal void Validate()
        {
            if (ValidateConnectionAsync == null)
                throw new InvalidOperationException("The ValidateConnectionAsync delegate must be provided.");
            if (GetStatus == null)
                throw new InvalidOperationException("The GetStatusMessage delegate must be provided.");
            if (MinimumRetryMilliseconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(MinimumRetryMilliseconds), "Minimum retry delay must be > 0.");
            if (MaximumRetryMilliseconds < MinimumRetryMilliseconds)
                throw new ArgumentOutOfRangeException(nameof(MaximumRetryMilliseconds),
                    "Maximum retry delay cannot be less than the minimum retry delay.");
            if (ExecutionIntervalMilliseconds < MinimumRetryMilliseconds || ExecutionIntervalMilliseconds > MaximumRetryMilliseconds)
                throw new ArgumentOutOfRangeException(nameof(ExecutionIntervalMilliseconds),
                    "Execution interval must be at least as large as the minimum retry delay and smaller than the maximum retry delay.");
        }
    }
}
