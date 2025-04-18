using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Provides logging functionality, including message logging and exception handling.
    /// </summary>
    [DebuggerStepThrough]
    internal class LogService : ILogService
    {
        private static long index = 0;
        private readonly LogQueue logQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogService"/> class.
        /// </summary>
        /// <param name="logQueue">The logging queue for message storage.</param>
        public LogService(LogQueue logQueue) => this.logQueue = logQueue ?? throw new ArgumentNullException(nameof(logQueue));

        /// <inheritdoc/>
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
                Debug.WriteLine(ex);
            }
        }

        /// <inheritdoc/>
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
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Logs a message to the logging queue.
        /// </summary>
        /// <param name="message">The log message to store.</param>
        private void Log(LogMessage message) => logQueue.TryWrite(message);
    }
}
