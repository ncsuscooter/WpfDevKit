using System.Collections.Concurrent;
using System.Diagnostics;
using WpfDevKit.Logging.Interfaces;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Provides a thread-safe queue for storing and retrieving log messages.
    /// </summary>
    [DebuggerStepThrough]
    internal class LogQueue
    {
        private readonly BlockingCollection<ILogMessage> messages = new BlockingCollection<ILogMessage>(8196);

        /// <summary>
        /// Attempts to read a log message from the queue.
        /// </summary>
        /// <param name="logMessage">The log message read from the queue, if successful.</param>
        /// <returns><c>true</c> if a log message was successfully read; otherwise, <c>false</c>.</returns>
        public bool TryRead(out ILogMessage logMessage) => messages.TryTake(out logMessage);

        /// <summary>
        /// Attempts to write a log message to the queue.
        /// </summary>
        /// <param name="message">The log message to add to the queue.</param>
        /// <returns><c>true</c> if the message was successfully added to the queue; otherwise, <c>false</c>.</returns>
        public bool TryWrite(ILogMessage message) => messages.TryAdd(message);
    }
}
