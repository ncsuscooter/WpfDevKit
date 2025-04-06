using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// A log provider that logs messages to the console or trace, depending on the environment.
    /// </summary>
    [DebuggerStepThrough]
    internal class ConsoleLogProvider : ILogProvider
    {
        private readonly ConsoleLogProviderOptions options;
        private bool isTrace;
        private bool isConsole;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleLogProvider"/> class.
        /// </summary>
        /// <param name="metrics">The metrics associated with this log provider.</param>
        /// <param name="options">The options for configuring the console log provider.</param>
        public ConsoleLogProvider(ConsoleLogProviderOptions options) => (Metrics, this.options) = (new LogMetrics<ConsoleLogProvider>(), options);

        /// <summary>
        /// Gets the metrics associated with this log provider.
        /// </summary>
        public ILogMetrics Metrics { get; private set; }

        /// <summary>
        /// Gets or sets the log categories that are enabled for logging.
        /// The default value is <see cref="TLogCategory.None"/> bitwise complemented 
        /// (<c>~TLogCategory.None</c>), which means all categories are enabled by default.
        /// </summary>
        public TLogCategory EnabledCategories { get; set; } = ~TLogCategory.None;

        /// <summary>
        /// Gets the log categories that are disabled for logging.
        /// The default value is <see cref="TLogCategory.None"/>, meaning no categories 
        /// are disabled by default.
        /// </summary>
        public TLogCategory DisabledCategories => TLogCategory.None;

        /// <summary>
        /// Logs a log message asynchronously to the console or trace, depending on the environment.
        /// </summary>
        /// <param name="message">The log message to be logged.</param>
        /// <returns>A task representing the asynchronous log operation.</returns>
        public Task LogAsync(ILogMessage message)
        {
            // Determine if we are in a console environment or tracing environment
            if (!isConsole && !isTrace)
            {
                try
                {
                    isConsole = Console.OpenStandardInput(1) != Stream.Null;
                    isTrace = !isConsole;
                }
                catch
                {
                    isTrace = true;
                }
            }

            using (var disposable = Metrics.StartStop(message))
            {
                // Format the log message using the configured options
                var s = options.FormattedOutput(message);

                // Log to the console if it's available
                if (isConsole)
                {
                    if (!options.CategoryConsoleColors.TryGetValue(message.Category, out var c))
                        c = options.CategoryConsoleColors[TLogCategory.None];

                    // Lock to ensure thread safety when writing to the console
                    lock (this)
                    {
                        Console.ForegroundColor = c;
                        Console.WriteLine(s);
                        Console.ResetColor();
                    }
                }
                // Log to trace if console isn't available
                else if (isTrace)
                {
                    Trace.WriteLine(s);
                }
            }

            return Task.CompletedTask;
        }
    }
}
