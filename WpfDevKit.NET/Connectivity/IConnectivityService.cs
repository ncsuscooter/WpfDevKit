using System;

namespace WpfDevKit.Connectivity
{
    /// <summary>
    /// Provides methods and events for monitoring the connectivity status of a system.
    /// </summary>
    public interface IConnectivityService
    {
        /// <summary>
        /// Occurs when the connection status changes, such as transitioning between connected, connecting, or disconnected states.
        /// </summary>
        event EventHandler StatusChanged;

        /// <summary>
        /// Gets a value indicating connection state.
        /// </summary>
        TConnectivityState State { get; }

        /// <summary>
        /// Gets the number of retry attempts that have occurred.
        /// </summary>
        int Attempts { get; }

        /// <summary>
        /// Gets the exception that caused the retry, if available.
        /// </summary>
        Exception Error { get; }

        /// <summary>
        /// Gets the expected timestamp of the next retry attempt.
        /// </summary>
        DateTime NextAttempt { get; }

        /// <summary>
        /// Gets the timestamp for the last successful connection attempt.
        /// </summary>
        DateTime? LastSuccessfulConnection { get; }

        /// <summary>
        /// Gets the connection uptime since the last successful connection.
        /// </summary>
        TimeSpan? Uptime { get; }

        /// <summary>
        /// Returns a user-friendly status message representing the current connection state.
        /// </summary>
        /// <returns>A string describing the current status.</returns>
        string GetStatus();

        /// <summary>
        /// Signals the service to execute immediately, bypassing any current delay.
        /// </summary>
        void TriggerImmediateExecution();
    }
}
