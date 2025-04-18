using System;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Defines the contract for logging metrics, including counters for different log categories
    /// and overall statistics such as the total count, queued logs, and unhandled exceptions.
    /// </summary>
    internal interface ILogMetricsWriter
    {
        /// <summary>
        /// Increments the elapsed time by the specified value.
        /// </summary>
        /// <param name="value">The value to increment the elapsed time by, in milliseconds.</param>
        void IncrementElapsed(long value);

        /// <summary>
        /// Increments the total log count by 1.
        /// </summary>
        void IncrementTotal();

        /// <summary>
        /// Increments the queued log count by 1.
        /// </summary>
        void IncrementQueued();

        /// <summary>
        /// Increments the lost log count by 1.
        /// </summary>
        void IncrementLost();

        /// <summary>
        /// Increments the null log count by 1.
        /// </summary>
        void IncrementNull();

        /// <summary>
        /// Increments the count of logs for a specific log category.
        /// </summary>
        /// <param name="category">The log category to increment the count for.</param>
        void IncrementCategory(TLogCategory category);

        /// <summary>
        /// Starts and stops a logging operation and tracks elapsed time.
        /// </summary>
        /// <param name="message">The log message being processed.</param>
        /// <returns>An <see cref="IDisposable"/> that tracks the start and stop of the operation.</returns>
        IDisposable StartStop(ILogMessage message);
    }
}
