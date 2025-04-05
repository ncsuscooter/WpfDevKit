using System;
using System.Collections.Generic;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Defines the contract for logging metrics, including counters for different log categories
    /// and overall statistics such as the total count, queued logs, and unhandled exceptions.
    /// </summary>
    public interface ILogMetrics
    {
        /// <summary>
        /// Gets the name of the log metrics, typically the name of the log provider or class.
        /// </summary>
        string Name { get; }

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
        /// Gets the count of unhandled exceptions that were encountered during logging.
        /// </summary>
        int Unhandled { get; }

        /// <summary>
        /// Gets a read-only collection of unhandled exceptions.
        /// </summary>
        IReadOnlyCollection<Exception> UnhandledExceptions { get; }

        /// <summary>
        /// Gets a read-only dictionary of log category counts, representing the number of logs per category.
        /// </summary>
        IReadOnlyDictionary<TLogCategory, int> CategoryCounts { get; }

        /// <summary>
        /// Gets the total elapsed time spent logging in milliseconds.
        /// </summary>
        TimeSpan Elapsed { get; }

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
        /// Increments the unhandled exception count by 1 and adds the exception to the collection.
        /// </summary>
        /// <param name="ex">The exception to be added to the collection.</param>
        void IncrementUnhandled(Exception ex);

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
    public interface ILogMetrics<out T> : ILogMetrics
    {
        // No additional members, used for type-safety with specific log providers.
    }
}
