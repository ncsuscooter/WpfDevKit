using System;
using System.Diagnostics;
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
        private readonly object sync;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleLogProvider"/> class.
        /// </summary>
        /// <param name="options">The options for configuring the console log provider.</param>
        public ConsoleLogProvider(IOptions<ConsoleLogProviderOptions> options) => (this.options, sync) = (options.Value, new object());

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
            // Format the log message using the configured options
            var s = options.FormattedOutput(message);
            try
            {
                // Log to the console if it's selected
                if (options.LogOutputWriter == Console.Out)
                {
                    if (!options.CategoryConsoleColors.TryGetValue(message.Category, out var c))
                        c = options.CategoryConsoleColors[TLogCategory.None];

                    // Lock to ensure thread safety when writing to the console, specifically so that the colors are set properly
                    lock (sync)
                    {
                        var orig = Console.ForegroundColor;
                        try
                        {
                            Console.ForegroundColor = c;
                            Console.WriteLine(s);
                        }
                        finally
                        {
                            Console.ForegroundColor = orig;
                        }
                    }
                }
                else
                {
                    options.LogOutputWriter.WriteLine(s);
                }
            }
            catch
            {
                Trace.WriteLine(s);
            }
            return Task.CompletedTask;
        }
    }
}
