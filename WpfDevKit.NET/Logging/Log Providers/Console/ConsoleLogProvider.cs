using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using WpfDevKit.DependencyInjection;

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
        /// <param name="options">The options for configuring the console log provider.</param>
        public ConsoleLogProvider(IOptions<ConsoleLogProviderOptions> options) => this.options = options.Value;

        /// <inheritdoc/>
        /// <remarks>
        /// The default value is <see cref="TLogCategory.None"/> bitwise complemented 
        /// (<c>~TLogCategory.None</c>), which means all categories are enabled by default.
        /// </remarks>
        public TLogCategory EnabledCategories { get; set; } = ~TLogCategory.None;

        /// <inheritdoc/>
        /// <remarks>
        /// The default value is <see cref="TLogCategory.None"/>, meaning no categories 
        /// are disabled by default.
        /// </remarks>
        public TLogCategory DisabledCategories => TLogCategory.None;

        /// <inheritdoc/>
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

            return Task.CompletedTask;
        }
    }
}
