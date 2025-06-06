using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Defines the contract for logging services that log messages and exceptions to various outputs.
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        /// Logs an exception with the specified category, type, file, and method information.
        /// </summary>
        /// <param name="category">The log category, such as <see cref="TLogCategory.Error"/>, <see cref="TLogCategory.Warning"/>, etc.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="type">The type of the caller (optional). Used to get the class name where the log originates.</param>
        /// <param name="fileName">The path to the file where the log originates (optional). This is provided automatically by the compiler.</param>
        /// <param name="memberName">The name of the method or property where the log is called (optional). This is provided automatically by the compiler.</param>
        void Log(TLogCategory category,
                 Exception exception,
                 Type type = default,
                 [CallerFilePath] string fileName = default,
                 [CallerMemberName] string memberName = default);

        /// <summary>
        /// Logs a message with the specified category, attributes, type, file, and method information.
        /// </summary>
        /// <param name="category">The log category, such as <see cref="TLogCategory.Info"/>, <see cref="TLogCategory.Debug"/>, etc.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="attributes">Additional attributes related to the log (optional).</param>
        /// <param name="type">The type of the caller (optional). Used to get the class name where the log originates.</param>
        /// <param name="fileName">The path to the file where the log originates (optional). This is provided automatically by the compiler.</param>
        /// <param name="memberName">The name of the method or property where the log is called (optional). This is provided automatically by the compiler.</param>
        void Log(TLogCategory category,
                 string message,
                 string attributes = default,
                 Type type = default,
                 [CallerFilePath] string fileName = default,
                 [CallerMemberName] string memberName = default);

        /// <summary>
        /// Waits until the log queue is empty or until the timeout expires.
        /// </summary>
        /// <param name="timeout">The maximum time to wait for the queue to flush.</param>
        /// <param name="cancellationToken">Optional token to cancel early.</param>
        /// <returns><c>true</c> if the queue was flushed; otherwise, <c>false</c>.</returns>
        Task<bool> FlushAsync(TimeSpan timeout, CancellationToken cancellationToken = default);
    }
}
