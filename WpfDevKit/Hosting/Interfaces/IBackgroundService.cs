using System;
using System.Threading;
using System.Threading.Tasks;

namespace WpfDevKit.Hosting.Interfaces
{
    /// <summary>
    /// Defines a contract for long-running background services that support start, stop, and disposal operations.
    /// </summary>
    public interface IBackgroundService : IDisposable
    {
        /// <summary>
        /// Starts the background service asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous start operation.</returns>
        Task StartAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Requests cancellation and stops the background service asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token used to cancel the stop operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous stop operation.</returns>
        Task StopAsync(CancellationToken cancellationToken);
    }
}
