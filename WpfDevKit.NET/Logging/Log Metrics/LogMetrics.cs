using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Tracks metrics related to logging, such as counts of logs by category, and provides methods for incrementing these metrics.
    /// </summary>
    /// <typeparam name="T">The type associated with the log metrics.</typeparam>
    [DebuggerStepThrough]
    internal class LogMetrics<T> : ILogMetrics<T>
    {
        private readonly ConcurrentDictionary<TLogCategory, int> categoryCounts = new ConcurrentDictionary<TLogCategory, int>();
        private readonly ConcurrentBag<Exception> unhandledExceptions = new ConcurrentBag<Exception>();
        private long elapsed = 0;
        private int totalCount = 0;
        private int queuedCount = 0;
        private int lostCount = 0;
        private int nullCount = 0;

        /// <summary>
        /// Gets the name of the type associated with the log metrics.
        /// </summary>
        public string Name => typeof(T).Name;

        /// <summary>
        /// Gets the total number of log entries.
        /// </summary>
        public int Total => totalCount;

        /// <summary>
        /// Gets the total number of queued log messages.
        /// </summary>
        public int Queued => queuedCount;

        /// <summary>
        /// Gets the total number of lost log messages.
        /// </summary>
        public int Lost => lostCount;

        /// <summary>
        /// Gets the total number of null log messages.
        /// </summary>
        public int Null => nullCount;

        /// <summary>
        /// Gets the total number of unhandled exceptions.
        /// </summary>
        public int Unhandled => unhandledExceptions.Count;

        /// <summary>
        /// Gets a collection of unhandled exceptions.
        /// </summary>
        public IReadOnlyCollection<Exception> UnhandledExceptions => new ReadOnlyCollection<Exception>(unhandledExceptions.ToList());

        /// <summary>
        /// Gets a dictionary of log categories and their associated counts.
        /// </summary>
        public IReadOnlyDictionary<TLogCategory, int> CategoryCounts => new ReadOnlyDictionary<TLogCategory, int>(categoryCounts);

        /// <summary>
        /// Gets the total elapsed time for logging operations.
        /// </summary>
        public TimeSpan Elapsed => TimeSpan.FromMilliseconds(elapsed);

        /// <summary>
        /// Increments the elapsed time by the specified value.
        /// </summary>
        /// <param name="value">The value to increment the elapsed time by, in milliseconds.</param>
        public void IncrementElapsed(long value) => ExecuteIfInternalOrThrow(() => Interlocked.Add(ref elapsed, value));

        /// <summary>
        /// Increments the total count of log entries by 1.
        /// </summary>
        public void IncrementTotal() => ExecuteIfInternalOrThrow(() => Interlocked.Increment(ref totalCount));

        /// <summary>
        /// Increments the count of queued log messages by 1.
        /// </summary>
        public void IncrementQueued() => ExecuteIfInternalOrThrow(() => Interlocked.Increment(ref queuedCount));

        /// <summary>
        /// Increments the count of lost log messages by 1.
        /// </summary>
        public void IncrementLost() => ExecuteIfInternalOrThrow(() => Interlocked.Increment(ref lostCount));

        /// <summary>
        /// Increments the count of null log messages by 1.
        /// </summary>
        public void IncrementNull() => ExecuteIfInternalOrThrow(() => Interlocked.Increment(ref nullCount));

        /// <summary>
        /// Adds an unhandled exception to the collection of unhandled exceptions.
        /// </summary>
        /// <param name="ex">The exception to add.</param>
        public void IncrementUnhandled(Exception ex) => ExecuteIfInternalOrThrow(() =>
        {
            Debug.WriteLine(ex);
            unhandledExceptions.Add(ex);
        });

        /// <summary>
        /// Increments the count of logs for a specified category.
        /// </summary>
        /// <param name="category">The log category to increment.</param>
        public void IncrementCategory(TLogCategory category) => ExecuteIfInternalOrThrow(() =>
        {
            if (!categoryCounts.ContainsKey(category))
                categoryCounts[category] = 0;
            categoryCounts[category]++;
        });

        /// <summary>
        /// Starts and stops a logging operation, incrementing the appropriate metrics.
        /// </summary>
        /// <param name="message">The log message associated with the operation.</param>
        /// <returns>An <see cref="IDisposable"/> object to stop the operation.</returns>
        public IDisposable StartStop(ILogMessage message)
        {
            IncrementTotal();
            if (message is null)
                IncrementNull();
            else
                IncrementCategory(message.Category);
            return new StartStopRegistration(default, IncrementElapsed);
        }

        /// <summary>
        /// Executes the specified action only if called from the internal assembly, otherwise throws an exception.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <exception cref="NotSupportedException">Thrown when the action is not called from the internal assembly.</exception>
        private void ExecuteIfInternalOrThrow(Action action)
        {
            if (Assembly.GetCallingAssembly().FullName.Equals(Assembly.GetExecutingAssembly().FullName))
                action();
            else
                throw new NotSupportedException("Not allowed to increment counters from calling assembly");
        }
    }
}
