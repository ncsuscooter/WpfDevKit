using System;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Represents a lightweight logging delegate for trace-level messages used internally by the logging infrastructure.
    /// </summary>
    /// <param name="message">The main log message or description of the event.</param>
    /// <param name="attributes">
    /// Optional context or metadata related to the log message, such as source information, tags, or state values.
    /// This should be formatted as a readable string (e.g., key-value pairs or plain text).
    /// </param>
    /// <param name="type">Optional instance of the type of the calling method's class.</param>
    internal delegate void LogMessageDelegate(string message, string attributes, Type type);

    /// <summary>
    /// Represents a lightweight logging delegate for trace-level exceptions used internally by the logging infrastructure.
    /// </summary>
    /// <param name="exception">The exception of the event.</param>
    /// <param name="type">Optional instance of the type of the calling method's class.</param>
    internal delegate void LogExceptionDelegate(Exception exception, Type type);

    /// <summary>
    /// Provides configuration options for a <see cref="ILogProviderCollection"/> instance.
    /// </summary>
    internal class LogOptions
    {
        /// <summary>
        /// Gets or sets the delegate used to log diagnostic messages related to provider management,
        /// such as provider registration, removal, or internal errors.
        /// </summary>
        /// <remarks>
        /// This delegate is typically used for tracing internal events like duplicate provider detection
        /// or failed insertions. It should not be used for general application logging.
        /// </remarks>
        public LogMessageDelegate LogMessage { get; set; }

        /// <summary>
        /// Gets or sets the delegate used to log diagnostic exceptions related to provider management,
        /// such as provider registration, removal, or internal errors.
        /// </summary>
        /// <remarks>
        /// This delegate is typically used for tracing internal events like duplicate provider detection
        /// or failed insertions. It should not be used for general application logging.
        /// </remarks>
        public LogExceptionDelegate LogException { get; set; }
    }
}
