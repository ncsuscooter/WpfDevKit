using System;
using System.Threading;
using System.Threading.Tasks;

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
        event Action ConnectionChanged;

        /// <summary>
        /// Gets a value indicating whether the system is currently connected.
        /// </summary>
        /// <value>
        /// <c>true</c> if the system is connected; otherwise, <c>false</c>.
        /// </value>
        bool IsConnected { get; }

        /// <summary>
        /// Gets the current status of the system's connectivity.
        /// This could be information such as "Connected", "Disconnected", or error messages.
        /// </summary>
        /// <value>
        /// The connectivity status message.
        /// </value>
        string Status { get; }

        /// <summary>
        /// Gets the host or endpoint to which the system is connected, if applicable.
        /// This could be a server or network address.
        /// </summary>
        /// <value>
        /// The host address.
        /// </value>
        string Host { get; }

        /// <summary>
        /// Invoked when the connection status changes.
        /// Triggers the <see cref="ConnectionChanged"/> event to notify subscribers of the status change.
        /// </summary>
        void OnConnectionChanged();

        /// <summary>
        /// Monitors the connectivity status asynchronously, with the option to cancel the operation.
        /// This method allows ongoing monitoring of the system's connection status.
        /// </summary>
        /// <param name="cancellationToken">A token used to cancel the operation if needed.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task MonitorAsync(CancellationToken cancellationToken);
    }
}
