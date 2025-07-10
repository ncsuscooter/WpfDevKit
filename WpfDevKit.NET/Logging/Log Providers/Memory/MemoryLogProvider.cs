using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using WpfDevKit.DependencyInjection;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// A log provider that stores log messages in memory.
    /// </summary>
    [DebuggerStepThrough]
    internal class MemoryLogProvider : ILogProvider, ILogSnapshot
    {
        private readonly MemoryLogProviderOptions options;
        private readonly List<ILogMessage> items;
        private readonly object sync;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryLogProvider"/> class.
        /// </summary>
        /// <param name="optionsAccessor">The options for configuring the memory log provider.</param>
        public MemoryLogProvider(IOptions<MemoryLogProviderOptions> optionsAccessor)
        {
            options = optionsAccessor.Value;
            items = new List<ILogMessage>();
            sync = new object();
        }

        /// <inheritdoc/>
        public Task LogAsync(ILogMessage message)
        {
            lock (sync)
            {
                // If the capacity is reached, remove old messages based on the fill factor.
                if (items.Count >= options.Capacity && options.Capacity > 0 && options.FillFactor > 0)
                {
                    // Calculate the number of items to remove based on the fill factor.
                    items.RemoveRange(0, Math.Max(1, (int)Math.Round(options.Capacity * (1.0 - options.FillFactor / 100.0), 0)));
                }
                // Add the new message to the list.
                items.Add(message);
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<ILogMessage> GetSnapshot()
        {
            lock (sync)
            {
                // Create a read-only copy of the stored log messages
                return new ReadOnlyCollection<ILogMessage>(items);
            }
        }
    }
}
