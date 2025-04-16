using System;

namespace WpfDevKit.Connectivity
{
    /// <summary>
    /// Provides methods and events for monitoring the connectivity status of a system.
    /// </summary>
    public interface IConnectivityService
    {
        /// <summary>
        /// Occurs when the connection status changes.
        /// </summary>
        event EventHandler StatusChanged;

        /// <summary>
        /// 
        /// </summary>
        bool IsConnecting { get; }

        /// <summary>
        /// 
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets the number of retry attempts that have occurred.
        /// </summary>
        int Attempts { get; }

        /// <summary>
        /// Gets the exception that caused the retry, if available.
        /// </summary>
        Exception Error { get; }

        /// <summary>
        /// Gets the timestamp associated with the next retry attempt.
        /// </summary>
        DateTime NextAttempt { get; }

        /// <summary>
        /// 
        /// </summary>
        string GetStatus();

        /// <summary>
        /// Signals the service to execute immediately, bypassing any current delay.
        /// </summary>
        void TriggerImmediateExecution();
    }
}
