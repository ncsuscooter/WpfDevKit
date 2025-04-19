using System.Threading.Tasks;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Defines the contract for a log provider that handles the logging of messages.
    /// </summary>
    public interface ILogProvider
    {
        /// <summary>
        /// Logs a log message asynchronously.
        /// </summary>
        /// <param name="message">The log message to be logged.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LogAsync(ILogMessage message);
    }
}
