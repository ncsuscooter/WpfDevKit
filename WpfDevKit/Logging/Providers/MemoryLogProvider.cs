using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using WpfDevKit.Logging.Enums;
using WpfDevKit.Logging.Interfaces;
using WpfDevKit.Logging.Providers.Options;

namespace WpfDevKit.Logging.Providers
{
    /// <summary>
    /// A log provider that stores log messages in memory.
    /// </summary>
    [DebuggerStepThrough]
    internal class MemoryLogProvider : ILogProvider
    {
        /// <summary>
        /// List to store log messages.
        /// </summary>
        protected readonly List<ILogMessage> items = new List<ILogMessage>();

        /// <summary>
        /// The options for configuring the memory log provider.
        /// </summary>
        private readonly MemoryLogProviderOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryLogProvider"/> class.
        /// </summary>
        /// <param name="options">The options for configuring the memory log provider.</param>
        public MemoryLogProvider(MemoryLogProviderOptions options) : this(new LogMetrics<MemoryLogProvider>(), options) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryLogProvider"/> class.
        /// </summary>
        /// <param name="metrics">The metrics associated with this log provider.</param>
        /// <param name="options">The options for configuring the memory log provider.</param>
        protected MemoryLogProvider(ILogMetrics metrics, MemoryLogProviderOptions options) => (Metrics, this.options) = (metrics, options);

        /// <summary>
        /// Gets or privately sets the metrics associated with this log provider.
        /// </summary>
        public ILogMetrics Metrics { get; private set; }

        /// <summary>
        /// Gets or sets the log categories that are enabled for logging.
        /// The default value is <see cref="TLogCategory.None"/> bitwise complemented 
        /// (<c>~TLogCategory.None</c>), meaning all categories are enabled by default.
        /// </summary>
        public virtual TLogCategory EnabledCategories { get; set; } = ~TLogCategory.None;

        /// <summary>
        /// Gets the log categories that are disabled for logging.
        /// The default value is <see cref="TLogCategory.None"/>, meaning no categories 
        /// are disabled by default.
        /// </summary>
        public virtual TLogCategory DisabledCategories => TLogCategory.None;

        /// <summary>
        /// Logs a log message asynchronously to the in-memory collection.
        /// </summary>
        /// <param name="message">The log message to be logged.</param>
        /// <returns>A task representing the asynchronous log operation.</returns>
        public Task LogAsync(ILogMessage message)
        {
            using (var disposable = Metrics.StartStop(message))
            {
                lock (items)
                {
                    // If the capacity is reached, remove old messages based on the fill factor.
                    if (items.Count >= options.Capacity && options.Capacity > 0 && options.FillFactor > 0)
                    {
                        // Calculate the number of items to remove based on the fill factor.
                        items.RemoveRange(0, (int)Math.Round(options.Capacity * (1d - options.FillFactor / 100d), 0));
                    }
                    items.Add(message); // Add the new message to the list.
                }
            }
            return Task.CompletedTask;
        }
    }
}
