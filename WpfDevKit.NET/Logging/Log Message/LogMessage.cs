using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Represents a log message with detailed information about the log event.
    /// </summary>
    [DebuggerStepThrough]
    internal class LogMessage : ILogMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogMessage"/> class.
        /// </summary>
        /// <param name="Index">The index of the log message.</param>
        /// <param name="Timestamp">The timestamp when the log message was created.</param>
        /// <param name="MachineName">The name of the machine where the log was generated.</param>
        /// <param name="UserName">The name of the user who generated the log.</param>
        /// <param name="ApplicationName">The name of the application generating the log.</param>
        /// <param name="ApplicationVersion">The version of the application generating the log.</param>
        /// <param name="ClassName">The class where the log was created.</param>
        /// <param name="MethodName">The method where the log was created.</param>
        /// <param name="ThreadId">The ID of the thread that generated the log message.</param>
        /// <param name="Category">The log category (e.g., Error, Warning, Info, etc.).</param>
        /// <param name="Message">The actual log message.</param>
        /// <param name="Attributes">Additional attributes or context related to the log message.</param>
        /// <param name="ExceptionLevel">The level of the exception, if the log message pertains to an exception.</param>
        /// <param name="ExceptionStackTrace">The stack trace for the exception, if applicable.</param>
        public LogMessage(long Index,
                          DateTime Timestamp,
                          string MachineName,
                          string UserName,
                          string ApplicationName,
                          Version ApplicationVersion,
                          string ClassName,
                          string MethodName,
                          int ThreadId,
                          TLogCategory Category,
                          string Message,
                          string Attributes,
                          int? ExceptionLevel,
                          string ExceptionStackTrace)
        {
            this.Index = Index;
            this.Timestamp = Timestamp;
            this.MachineName = MachineName;
            this.UserName = UserName;
            this.ApplicationName = ApplicationName;
            this.ApplicationVersion = ApplicationVersion;
            this.ClassName = ClassName;
            this.MethodName = MethodName;
            this.ThreadId = ThreadId;
            this.Category = Category;
            this.Message = Message;
            this.Attributes = Attributes;
            this.ExceptionLevel = ExceptionLevel;
            this.ExceptionStackTrace = ExceptionStackTrace;
        }

        /// <inheritdoc/>
        public long Index { get; }

        /// <inheritdoc/>
        public DateTime Timestamp { get; }

        /// <inheritdoc/>
        public string MachineName { get; }

        /// <inheritdoc/>
        public string UserName { get; }

        /// <inheritdoc/>
        public string ApplicationName { get; }

        /// <inheritdoc/>
        public Version ApplicationVersion { get; }

        /// <inheritdoc/>
        public string ClassName { get; }

        /// <inheritdoc/>
        public string MethodName { get; }

        /// <inheritdoc/>
        public int ThreadId { get; }

        /// <inheritdoc/>
        public TLogCategory Category { get; }

        /// <inheritdoc/>
        public string Message { get; }

        /// <inheritdoc/>
        public string Attributes { get; }

        /// <inheritdoc/>
        public int? ExceptionLevel { get; }

        /// <inheritdoc/>
        public string ExceptionStackTrace { get; }

        /// <inheritdoc/>
        public override string ToString() => GetType().GetProperties()
                                                      .Select(info => (info.Name, Value: info.GetValue(this, null) ?? "(null)"))
                                                      .Aggregate(new StringBuilder(), (sb, pair) => sb.AppendLine($"{pair.Name}: {pair.Value}"), sb => sb.ToString());
    }
}
