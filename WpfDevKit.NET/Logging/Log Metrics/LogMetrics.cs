using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Tracks metrics related to logging, such as counts of logs by category, and provides methods for incrementing these metrics.
    /// </summary>
    [DebuggerStepThrough]
    internal class LogMetrics : ILogMetricsReader, ILogMetricsWriter
    {
        private readonly ConcurrentDictionary<TLogCategory, int> categoryCounts = new ConcurrentDictionary<TLogCategory, int>();
        private long elapsed = 0;
        private int totalCount = 0;
        private int queuedCount = 0;
        private int lostCount = 0;
        private int nullCount = 0;

        /// <inheritdoc/>
        public int Total => totalCount;

        /// <inheritdoc/>
        public int Queued => queuedCount;

        /// <inheritdoc/>
        public int Lost => lostCount;

        /// <inheritdoc/>
        public int Null => nullCount;

        /// <inheritdoc/>
        public IReadOnlyDictionary<TLogCategory, int> CategoryCounts => new ReadOnlyDictionary<TLogCategory, int>(categoryCounts);

        /// <inheritdoc/>
        public TimeSpan Elapsed => TimeSpan.FromMilliseconds(elapsed);

        /// <inheritdoc/>
        public void IncrementElapsed(long value) => Interlocked.Add(ref elapsed, value);

        /// <inheritdoc/>
        public void IncrementTotal() => Interlocked.Increment(ref totalCount);

        /// <inheritdoc/>
        public void IncrementQueued() => Interlocked.Increment(ref queuedCount);

        /// <inheritdoc/>
        public void IncrementLost() => Interlocked.Increment(ref lostCount);

        /// <inheritdoc/>
        public void IncrementNull() => Interlocked.Increment(ref nullCount);

        /// <inheritdoc/>
        public void IncrementCategory(TLogCategory category)
        {
            if (!categoryCounts.ContainsKey(category))
                categoryCounts[category] = 0;
            categoryCounts[category]++;
        }

        /// <inheritdoc/>
        public IDisposable StartStop(ILogMessage message)
        {
            IncrementTotal();
            if (message is null)
                IncrementNull();
            else
                IncrementCategory(message.Category);
            return new StartStopRegistration(default, IncrementElapsed);
        }
    }

    /// <inheritdoc/>
    /// <typeparam name="T">The type associated with the log metrics.</typeparam>
    [DebuggerStepThrough]
    internal class LogMetrics<T> : LogMetrics, ILogMetricsReader<T>, ILogMetricsWriter<T>
    {
        /// <summary>
        /// Gets the name of the type associated with the log metrics.
        /// </summary>
        public string Name => typeof(T).Name;
    }
}
