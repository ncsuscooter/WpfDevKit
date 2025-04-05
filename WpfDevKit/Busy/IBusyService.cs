using System;

namespace WpfDevKit.Busy
{
    /// <summary>
    /// Defines a service that tracks busy state changes and notifies subscribers.
    /// </summary>
    public interface IBusyService
    {
        /// <summary>
        /// Occurs when the busy state changes.
        /// </summary>
        event Action IsBusyChanged;

        /// <summary>
        /// Gets a value indicating whether the service is currently busy.
        /// </summary>
        bool IsBusy { get; }

        /// <summary>
        /// Marks the service as busy and returns a disposable registration  
        /// that decrements the counter when disposed.
        /// </summary>
        /// <returns>An <see cref="IDisposable"/> that decrements the busy counter on disposal.</returns>
        IDisposable Busy();
    }
}
