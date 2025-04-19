using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Provides a thread-safe queue for storing and retrieving log messages.
    /// </summary>
    [DebuggerStepThrough]
    internal class LogQueue
    {
        private readonly BlockingCollection<ILogMessage> messages = new BlockingCollection<ILogMessage>(8196);
        private readonly LogMetrics metrics;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogQueue"/> class with the specified metrics tracker.
        /// </summary>
        /// <param name="metrics">
        /// The <see cref="LogMetrics"/> instance used to record logging statistics such as total messages, category counts,
        /// queued or lost entries, and null messages. This enables real-time monitoring and diagnostics of the logging pipeline.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="metrics"/> parameter is <c>null</c>.
        /// </exception>
        public LogQueue(LogMetrics metrics) => this.metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));

        /// <summary>
        /// Attempts to read a log message from the queue.
        /// </summary>
        /// <param name="message">The log message read from the queue, if successful.</param>
        /// <returns><c>true</c> if a log message was successfully read; otherwise, <c>false</c>.</returns>
        public bool TryRead(out ILogMessage message) => messages.TryTake(out message);

        /// <summary>
        /// Attempts to write a log message to the queue.
        /// </summary>
        /// <param name="message">The log message to add to the queue.</param>
        /// <returns><c>true</c> if the message was successfully added to the queue; otherwise, <c>false</c>.</returns>
        public bool TryWrite(ILogMessage message)
        {
            // StartStop method increments total, null, and category metrics internally
            using (metrics.StartStop(message))
            {
                var result = message != null && messages.TryAdd(message);
                if (result)
                    metrics.IncrementQueued();
                else
                    metrics.IncrementLost();
                return result;
            }
        }
    }
}
