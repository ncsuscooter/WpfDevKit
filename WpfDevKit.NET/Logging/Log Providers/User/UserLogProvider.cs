using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using WpfDevKit.DependencyInjection;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// A log provider that provides user-specific logging functionality.
    /// It manages a collection of log messages with custom behavior for enabled/disabled categories and optional log clearing.
    /// </summary>
    [DebuggerStepThrough]
    internal class UserLogProvider : ILogProvider, ILogSnapshot
    {
        private readonly UserLogProviderOptions options;
        private readonly List<ILogMessage> items;
        private readonly object sync;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserLogProvider"/> class.
        /// </summary>
        /// <param name="optionsAccessor">The options for configuring the user log provider.</param>
        public UserLogProvider(IOptions<UserLogProviderOptions> optionsAccessor)
        {
            options = optionsAccessor.Value;
            items = new List<ILogMessage>();
            sync = new object();
        }

        /// <inheritdoc/>
        public Task LogAsync(ILogMessage message)
        {
            lock (sync)
                items.Add(message);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<ILogMessage> GetSnapshot()
        {
            lock (sync)
            {
                // Create a read-only copy of the stored log messages
                var result = new ReadOnlyCollection<ILogMessage>(items);

                // Optionally clear the logs after retrieval based on the user's option
                if (options.IsClearLogsOnGet)
                    items.Clear();

                return result;
            }
        }
    }
}
