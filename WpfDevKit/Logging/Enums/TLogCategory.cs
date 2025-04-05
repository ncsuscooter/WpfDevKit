namespace WpfDevKit.Logging.Enums
{
    /// <summary>
    /// Specifies the categories of logs.
    /// </summary>
    [System.Flags]
    public enum TLogCategory
    {
        /// <summary>
        /// No category.
        /// </summary>
        None = 0,

        /// <summary>
        /// Trace level logging, typically used for detailed diagnostic information.
        /// </summary>
        Trace = 1,

        /// <summary>
        /// Debug level logging, typically used for debugging purposes.
        /// </summary>
        Debug = 2,

        /// <summary>
        /// Information level logging, used for general informational messages.
        /// </summary>
        Info = 4,

        /// <summary>
        /// Start/Stop logging, used for tracking task or operation timing.
        /// </summary>
        StartStop = 8,

        /// <summary>
        /// Warning level logging, used for potentially harmful situations.
        /// </summary>
        Warning = 16,

        /// <summary>
        /// Error level logging, used for error messages indicating failure.
        /// </summary>
        Error = 32,

        /// <summary>
        /// Fatal level logging, used for critical errors that may cause a system crash.
        /// </summary>
        Fatal = 64
    }
}
