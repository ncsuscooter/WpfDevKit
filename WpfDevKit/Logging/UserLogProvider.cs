using System.Collections.Generic;
using System.Diagnostics;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// A log provider that extends the <see cref="MemoryLogProvider"/> and provides user-specific logging functionality.
    /// It manages a collection of log messages with custom behavior for enabled/disabled categories and optional log clearing.
    /// </summary>
    [DebuggerStepThrough]
    internal class UserLogProvider : MemoryLogProvider, IUserLogProvider
    {
        /// <summary>
        /// The options for configuring the user log provider.
        /// </summary>
        private readonly UserLogProviderOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserLogProvider"/> class.
        /// </summary>
        /// <param name="metrics">The metrics associated with this log provider.</param>
        /// <param name="options">The options for configuring the user log provider.</param>
        public UserLogProvider(UserLogProviderOptions options)
            : base(new LogMetrics<UserLogProvider>(), options) => this.options = options;

        /// <summary>
        /// Gets or sets the log categories that are enabled for logging.
        /// The default value is <c>TLogCategory.Fatal | TLogCategory.Error | TLogCategory.Warning</c>,
        /// meaning these categories are enabled for logging by default.
        /// </summary>
        public override TLogCategory EnabledCategories { get; set; } = TLogCategory.Fatal | TLogCategory.Error | TLogCategory.Warning;

        /// <summary>
        /// Gets the log categories that are disabled for logging.
        /// The default value is <c>~(TLogCategory.Fatal | TLogCategory.Error | TLogCategory.Warning)</c>,
        /// meaning these categories are disabled by default and no other categories are explicitly disabled.
        /// </summary>
        public override TLogCategory DisabledCategories => ~(TLogCategory.Fatal | TLogCategory.Error | TLogCategory.Warning);

        /// <summary>
        /// Retrieves the stored log messages.
        /// Optionally clears the stored messages based on the <see cref="UserLogProviderOptions.IsClearLogsOnGet"/> setting.
        /// </summary>
        /// <returns>A read-only collection of the stored log messages.</returns>
        public IReadOnlyCollection<ILogMessage> GetLogMessages()
        {
            lock (this)
            {
                // Create a read-only copy of the stored log messages
                var result = new List<ILogMessage>(items).AsReadOnly();

                // Optionally clear the logs after retrieval based on the user's option
                if (options.IsClearLogsOnGet)
                    items.Clear();

                return result;
            }
        }
    }
}
