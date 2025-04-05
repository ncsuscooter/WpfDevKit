namespace WpfDevKit.Logging.Enums
{
    /// <summary>
    /// Represents the elements that can be part of a log message.
    /// </summary>
    public enum TLogElement
    {
        /// <summary>
        /// The index of the log entry.
        /// </summary>
        Index,

        /// <summary>
        /// The timestamp when the log message was generated.
        /// </summary>
        Timestamp,

        /// <summary>
        /// The name of the machine where the log message was generated.
        /// </summary>
        MachineName,

        /// <summary>
        /// The name of the user who generated the log message.
        /// </summary>
        UserName,

        /// <summary>
        /// The name of the application that generated the log message.
        /// </summary>
        ApplicationName,

        /// <summary>
        /// The version of the application that generated the log message.
        /// </summary>
        ApplicationVersion,

        /// <summary>
        /// The name of the class that generated the log message.
        /// </summary>
        ClassName,

        /// <summary>
        /// The name of the method that generated the log message.
        /// </summary>
        MethodName,

        /// <summary>
        /// The ID of the thread that generated the log message.
        /// </summary>
        ThreadId,

        /// <summary>
        /// The category of the log message (e.g., Info, Error, etc.).
        /// </summary>
        Category,

        /// <summary>
        /// The actual log message content.
        /// </summary>
        Message,

        /// <summary>
        /// Additional attributes related to the log message.
        /// </summary>
        Attributes,

        /// <summary>
        /// The level of the exception (if any) associated with the log message.
        /// </summary>
        ExceptionLevel,

        /// <summary>
        /// The stack trace of the exception (if any) associated with the log message.
        /// </summary>
        ExceptionStackTrace
    }
}
