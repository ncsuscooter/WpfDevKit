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
        /// 
        /// </summary>
        /// <param name="metrics"></param>
        public LogQueue(LogMetrics metrics) => this.metrics = metrics;

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
            var result = message != null && messages.TryAdd(message);
            if (result)
                metrics.IncrementQueued();
            else
                metrics.IncrementLost();
            return result;
        }
    }
}
