using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using WpfDevKit.Logging.Enums;
using WpfDevKit.Logging.Interfaces;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Provides logging functionality, including message logging and exception handling.
    /// </summary>
    [DebuggerStepThrough]
    internal class LogService : ILogService
    {
        private static long index = 0;
        private readonly LogQueue queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogService"/> class.
        /// </summary>
        /// <param name="queue">The logging queue for message storage.</param>
        public LogService(LogQueue queue) => this.queue = queue;

        /// <summary>
        /// Gets the metrics associated with the log service.
        /// </summary>
        public ILogMetrics Metrics { get; private set; } = new LogMetrics<LogService>();

        /// <summary>
        /// Logs a message to the logging queue.
        /// </summary>
        /// <param name="message">The log message to store.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
        private void Log(LogMessage message)
        {
            if (message is null)
            using (var disposable = Metrics.StartStop(message))
            {
                if (queue.TryWrite(message))
                    Metrics.IncrementQueued();
                else
                    Metrics.IncrementLost();
            }
        }

        /// <summary>
        /// Logs an exception with additional contextual details.
        /// </summary>
        /// <param name="category">The log category (must be Error, Warning, or Trace).</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="type">The type where the exception originated (optional).</param>
        /// <param name="fileName">The file where the exception occurred (auto-captured).</param>
        /// <param name="memberName">The method where the exception occurred (auto-captured).</param>
        public void Log(TLogCategory category,
                        Exception exception,
                        Type type = default,
                        [CallerFilePath] string fileName = default,
                        [CallerMemberName] string memberName = default)
        {
            try
            {
                var level = 1;
                while (exception != null)
                {
                    string attributes = default;
                    if (exception.Data.Count > 0)
                        attributes = string.Join(" - ", exception.Data.Cast<DictionaryEntry>().Select(x => $"{x.Key}='{x.Value ?? "N/A"}'"));
                    Log(new LogMessage(
                        Index: Interlocked.Increment(ref index),
                        Timestamp: DateTime.Now,
                        MachineName: Environment.MachineName,
                        UserName: Environment.UserName,
                        ApplicationName: Assembly.GetEntryAssembly().GetName().Name,
                        ApplicationVersion: Assembly.GetEntryAssembly().GetName().Version,
                        ClassName: $"{type?.Name}{(type is null ? string.Empty : "\\")}{Path.GetFileNameWithoutExtension(fileName)}",
                        MethodName: memberName,
                        ThreadId: Environment.CurrentManagedThreadId,
                        Category: category,
                        Message: exception.Message,
                        Attributes: attributes,
                        ExceptionLevel: level,
                        ExceptionStackTrace: exception.StackTrace));
                    exception = exception.InnerException;
                    level++;
                }
            }
            catch (Exception ex)
            {
                Metrics.IncrementUnhandled(ex);
            }
        }

        /// <summary>
        /// Logs a message with optional attributes and contextual details.
        /// </summary>
        /// <param name="category">The log category (must not be Error).</param>
        /// <param name="message">The message to log.</param>
        /// <param name="attributes">Additional attributes (optional).</param>
        /// <param name="type">The type where the message originated (optional).</param>
        /// <param name="fileName">The file where the message was logged (auto-captured).</param>
        /// <param name="memberName">The method where the message was logged (auto-captured).</param>
        public void Log(TLogCategory category,
                        string message,
                        string attributes = default,
                        Type type = default,
                        [CallerFilePath] string fileName = default,
                        [CallerMemberName] string memberName = default)
        {
            try
            {
                Log(new LogMessage(
                    Index: Interlocked.Increment(ref index),
                    Timestamp: DateTime.Now,
                    MachineName: Environment.MachineName,
                    UserName: Environment.UserName,
                    ApplicationName: Assembly.GetEntryAssembly().GetName().Name,
                    ApplicationVersion: Assembly.GetEntryAssembly().GetName().Version,
                    ClassName: $"{type?.Name}{(type is null ? string.Empty : "\\")}{Path.GetFileNameWithoutExtension(fileName)}",
                    MethodName: memberName,
                    ThreadId: Environment.CurrentManagedThreadId,
                    Category: category,
                    Message: message?.Replace(Environment.NewLine, string.Empty),
                    Attributes: attributes,
                    ExceptionLevel: default,
                    ExceptionStackTrace: default));
            }
            catch (Exception ex)
            {
                Metrics.IncrementUnhandled(ex);
            }
        }
    }
}
