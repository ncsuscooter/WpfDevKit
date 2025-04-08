using System;

/// <summary>
/// Represents a lightweight logging delegate for trace-level messages used internally by the logging infrastructure.
/// </summary>
/// <param name="message">The main log message or description of the event.</param>
/// <param name="attributes">
/// Optional context or metadata related to the log message, such as source information, tags, or state values.
/// This should be formatted as a readable string (e.g., key-value pairs or plain text).
/// </param>
/// <param name="type">Optional instance of the type of the calling method's class.</param>
internal delegate void LogInternalMessageDelegate(string message, string attributes, Type type);

/// <summary>
/// Represents a lightweight logging delegate for trace-level exceptions used internally by the logging infrastructure.
/// </summary>
/// <param name="exception">The exception of the event.</param>
/// <param name="type">Optional instance of the type of the calling method's class.</param>
internal delegate void LogInternalExceptionDelegate(Exception exception, Type type);

/// <summary>
/// Provides access to delegates for logging internal trace messages and exceptions.
/// Instance can be used by any service with access to the <see cref="IServiceProvider" />.
/// </summary>
internal class InternalLogger
{
    /// <summary>
    /// Gets or sets the delegate used to log diagnostic messages.
    /// </summary>
    /// <remarks>
    /// This delegate is typically used for tracing internal events.
    /// It should not be used for general application logging.
    /// </remarks>
    public LogInternalMessageDelegate LogMessage { get; set; }

    /// <summary>
    /// Gets or sets the delegate used to log diagnostic exceptions.
    /// </summary>
    /// <remarks>
    /// This delegate is typically used for tracing internal events.
    /// It should not be used for general application logging.
    /// </remarks>
    public LogInternalExceptionDelegate LogException { get; set; }
}
