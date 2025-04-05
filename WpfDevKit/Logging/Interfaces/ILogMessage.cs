using System;
using WpfDevKit.Logging.Enums;

namespace WpfDevKit.Logging.Interfaces
{
    /// <summary>
    /// Defines the contract for a log message with detailed information about the log event.
    /// </summary>
    public interface ILogMessage
    {
        /// <summary>
        /// Gets the index of the log message.
        /// </summary>
        long Index { get; }

        /// <summary>
        /// Gets the timestamp when the log message was created.
        /// </summary>
        DateTime Timestamp { get; }

        /// <summary>
        /// Gets the name of the machine where the log was generated.
        /// </summary>
        string MachineName { get; }

        /// <summary>
        /// Gets the name of the user who generated the log.
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// Gets the name of the application generating the log.
        /// </summary>
        string ApplicationName { get; }

        /// <summary>
        /// Gets the version of the application generating the log.
        /// </summary>
        Version ApplicationVersion { get; }

        /// <summary>
        /// Gets the name of the class where the log was created.
        /// </summary>
        string ClassName { get; }

        /// <summary>
        /// Gets the name of the method where the log was created.
        /// </summary>
        string MethodName { get; }

        /// <summary>
        /// Gets the ID of the thread that generated the log message.
        /// </summary>
        int ThreadId { get; }

        /// <summary>
        /// Gets the category of the log message (e.g., Error, Warning, Info, etc.).
        /// </summary>
        TLogCategory Category { get; }

        /// <summary>
        /// Gets the actual log message.
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Gets any additional attributes or context related to the log message.
        /// </summary>
        string Attributes { get; }

        /// <summary>
        /// Gets the level of the exception, if the log message pertains to an exception.
        /// </summary>
        int? ExceptionLevel { get; }

        /// <summary>
        /// Gets the stack trace for the exception, if applicable.
        /// </summary>
        string ExceptionStackTrace { get; }
    }
}