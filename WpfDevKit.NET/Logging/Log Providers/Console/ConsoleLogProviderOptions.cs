using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Configuration options for the <see cref="ConsoleLogProvider"/> class, which handles logging to the console.
    /// </summary>
    [DebuggerStepThrough]
    public class ConsoleLogProviderOptions
    {
        /// <summary>
        /// A dictionary that associates each log category with a specific console color.
        /// Default values are provided for each <see cref="TLogCategory"/>.
        /// </summary>
        public Dictionary<TLogCategory, ConsoleColor> CategoryConsoleColors { get; } = new Dictionary<TLogCategory, ConsoleColor>()
        {
            [TLogCategory.None] = ConsoleColor.Gray,             // Default for unspecified categories
            [TLogCategory.Trace] = ConsoleColor.DarkYellow,      // Trace messages
            [TLogCategory.Debug] = ConsoleColor.DarkGray,        // Debug messages
            [TLogCategory.Info] = ConsoleColor.White,            // Informational messages
            [TLogCategory.StartStop] = ConsoleColor.DarkMagenta, // Start/Stop log category
            [TLogCategory.Warning] = ConsoleColor.DarkCyan,      // Warning messages
            [TLogCategory.Error] = ConsoleColor.DarkRed,         // Error messages
            [TLogCategory.Fatal] = ConsoleColor.Red,             // Fatal error messages
        };

        /// <summary>
        /// A function that formats the log message for console output.
        /// The default format includes the log index, timestamp, thread ID, category, class, method, message, attributes, and exception details.
        /// </summary>
        public Func<ILogMessage, string> FormattedOutput { get; set; } = logMessage =>
        {
            var s = $"{logMessage.Index}\t{logMessage.Timestamp:M/d/yy HH:mm:ss.fff}\t[0x{logMessage.ThreadId:X2}]\t{logMessage.Category,-17} {logMessage.ClassName}.{logMessage.MethodName}";
            if (!string.IsNullOrWhiteSpace(logMessage.Message))
                s = $"{s}: {logMessage.Message}";
            if (!string.IsNullOrWhiteSpace(logMessage.Attributes))
                s = $"{s} [{logMessage.Attributes}]";
            if (logMessage.ExceptionLevel > 0)
                s = $"{s}\n\r{logMessage.ExceptionStackTrace}";
            return s;
        };
        
        /// <summary>
        /// Gets or sets the text writer used for outputting log messages.
        /// Defaults to <see cref="Console.Out"/>. If <c>null</c>, logging will fall back to <see cref="Trace"/>.
        /// </summary>
        public TextWriter LogOutputWriter { get; set; } = Console.Out;
    }
}
