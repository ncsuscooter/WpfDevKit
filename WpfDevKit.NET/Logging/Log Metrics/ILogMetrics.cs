using System;
using System.Collections.Generic;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Defines the contract for logging metrics, including counters for different log categories
    /// and overall statistics such as the total count, queued logs, and unhandled exceptions.
    /// </summary>
    public interface ILogMetricsReader
    {
        /// <summary>
        /// Gets the total count of log messages processed.
        /// </summary>
        int Total { get; }

        /// <summary>
        /// Gets the count of log messages that are queued but not yet processed.
        /// </summary>
        int Queued { get; }

        /// <summary>
        /// Gets the count of log messages that were lost due to some failure or constraint.
        /// </summary>
        int Lost { get; }

        /// <summary>
        /// Gets the count of null log messages that were encountered.
        /// </summary>
        int Null { get; }

        /// <summary>
        /// Gets a read-only dictionary of log category counts, representing the number of logs per category.
        /// </summary>
        IReadOnlyDictionary<TLogCategory, int> CategoryCounts { get; }

        /// <summary>
        /// Gets the total elapsed time spent logging in milliseconds.
        /// </summary>
        TimeSpan Elapsed { get; }
    }

    /// <summary>
    /// A generic version of the <see cref="ILogMetrics"/> interface for use with specific log providers.
    /// </summary>
    public interface ILogMetricsReader<out T> : ILogMetricsReader
    {
        // No additional members, used for type-safety with specific log providers.
    }

    /// <summary>
    /// Defines the contract for logging metrics, including counters for different log categories
    /// and overall statistics such as the total count, queued logs, and unhandled exceptions.
    /// </summary>
    internal interface ILogMetricsWriter
    {
        /// <summary>
        /// Increments the elapsed time by the specified value.
        /// </summary>
        /// <param name="value">The value to increment the elapsed time by.</param>
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

    /// <summary>
    /// A generic version of the <see cref="ILogMetrics"/> interface for use with specific log providers.
    /// </summary>
    internal interface ILogMetricsWriter<out T> : ILogMetricsWriter
    {
        // No additional members, used for type-safety with specific log providers.
    }
}
