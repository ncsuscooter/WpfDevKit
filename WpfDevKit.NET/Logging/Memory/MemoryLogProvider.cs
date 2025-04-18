using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using WpfDevKit.DependencyInjection;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// A log provider that stores log messages in memory.
    /// </summary>
    [DebuggerStepThrough]
    internal class MemoryLogProvider : ILogProvider
    {
        private readonly MemoryLogProviderOptions options;
        protected readonly List<ILogMessage> items = new List<ILogMessage>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryLogProvider"/> class,
        /// but meant for subclasses, like the <see cref="UserLogProvider"/>.
        /// </summary>
        /// <param name="options">The options for configuring the memory log provider.</param>
        protected MemoryLogProvider(MemoryLogProviderOptions options) => this.options = options;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryLogProvider"/> class.
        /// </summary>
        /// <param name="options">The options for configuring the memory log provider.</param>
        public MemoryLogProvider(IOptions<MemoryLogProviderOptions> options) : this(options.Value) { }

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
            lock (items)
            {
                // If the capacity is reached, remove old messages based on the fill factor.
                if (items.Count >= options.Capacity && options.Capacity > 0 && options.FillFactor > 0)
                {
                    // Calculate the number of items to remove based on the fill factor.
                    items.RemoveRange(0, Math.Max(1, (int)Math.Round(options.Capacity * (1.0 - options.FillFactor / 100.0), 0)));
                }
                items.Add(message); // Add the new message to the list.
            }
            return Task.CompletedTask;
        }
    }
}
