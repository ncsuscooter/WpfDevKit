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
        /// Gets the total elapsed time spent logging in milliseconds.
        /// </summary>
        TimeSpan Elapsed { get; }

        /// <summary>
        /// Gets a read-only dictionary of log category counts, representing the number of logs per category.
        /// </summary>
        IReadOnlyDictionary<TLogCategory, int> CategoryCounts { get; }
    }
}
