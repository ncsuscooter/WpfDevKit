using System.Collections.Generic;
using System.Diagnostics;
using WpfDevKit.DependencyInjection;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// A log provider that extends the <see cref="MemoryLogProvider"/> and provides user-specific logging functionality.
    /// It manages a collection of log messages with custom behavior for enabled/disabled categories and optional log clearing.
    /// </summary>
    [DebuggerStepThrough]
    internal class UserLogProvider : MemoryLogProvider, IUserLogProvider
    {
        private readonly UserLogProviderOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserLogProvider"/> class.
        /// </summary>
        /// <param name="options">The options for configuring the user log provider.</param>
        public UserLogProvider(IOptions<UserLogProviderOptions> options) : base(options.Value) => this.options = options.Value;

        /// <inheritdoc/>
        /// <remarks>
        /// The default value is <c>TLogCategory.Fatal | TLogCategory.Error | TLogCategory.Warning</c>,
        /// meaning these categories are enabled for logging by default.
        /// </remarks>
        public override TLogCategory EnabledCategories { get; set; } = TLogCategory.Fatal | TLogCategory.Error | TLogCategory.Warning;

        /// <inheritdoc/>
        /// <remarks>
        /// The default value is <c>~(TLogCategory.Fatal | TLogCategory.Error | TLogCategory.Warning)</c>,
        /// meaning these categories are disabled by default and no other categories are explicitly disabled.
        /// </remarks>
        public override TLogCategory DisabledCategories => ~(TLogCategory.Fatal | TLogCategory.Error | TLogCategory.Warning);

        /// <inheritdoc/>
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
