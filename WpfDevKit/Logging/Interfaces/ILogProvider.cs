using System.Threading.Tasks;
using WpfDevKit.Logging.Enums;

namespace WpfDevKit.Logging.Interfaces
{
    /// <summary>
    /// Defines the contract for a log provider that handles the logging of messages.
    /// </summary>
    public interface ILogProvider
    {
        /// <summary>
        /// Gets the metrics associated with the log provider.
        /// </summary>
        ILogMetrics Metrics { get; }

        /// <summary>
        /// Gets or sets the categories of log messages that are enabled for logging.
        /// </summary>
        TLogCategory EnabledCategories { get; set; }

        /// <summary>
        /// Gets the categories of log messages that are disabled from logging.
        /// </summary>
        TLogCategory DisabledCategories { get; }

        /// <summary>
        /// Logs a log message asynchronously.
        /// </summary>
        /// <param name="message">The log message to be logged.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LogAsync(ILogMessage message);
    }
}
