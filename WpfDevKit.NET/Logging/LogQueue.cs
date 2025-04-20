using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Provides a thread-safe queue for storing and retrieving log messages.
    /// </summary>
    [DebuggerStepThrough]
    internal class LogQueue
    {
        private readonly BlockingCollection<ILogMessage> messages = new BlockingCollection<ILogMessage>(8196);
        private readonly AsyncAutoResetEvent reset = new AsyncAutoResetEvent();
        private readonly LogMetrics metrics;

        /// <summary>
        /// Indicates whether the log queue is currently empty.
        /// </summary>
        public bool IsEmpty => messages.Count == 0;

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
        public bool TryRead(out ILogMessage message)
        {
            var result = messages.TryTake(out message);
            if (result)
                reset.Signal();
            return result;
        }

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
                reset.Signal();
                return result;
            }
        }

        /// <summary>
        /// Asynchronously waits for a change in the log queue (either a message was added or removed).
        /// </summary>
        /// <param name="token">A cancellation token to cancel the wait operation.</param>
        /// <returns>
        /// A task that completes when the log queue signals a change. 
        /// This allows consumers to react to queue state changes without polling.
        /// </returns>
        /// <remarks>
        /// This is useful for scenarios like flushing the log queue, where external components 
        /// need to wait until the queue becomes empty or is modified.
        /// </remarks>
        public Task WaitAsync(CancellationToken token = default) => reset.WaitAsync(token);
    }
}
