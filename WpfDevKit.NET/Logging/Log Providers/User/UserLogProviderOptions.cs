using System.Diagnostics;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Configuration options for the <see cref="UserLogProvider"/> class, which extends <see cref="MemoryLogProvider"/> for user-specific logging.
    /// </summary>
    [DebuggerStepThrough]
    public sealed class UserLogProviderOptions : ILogProviderOptions
    {
        /// <inheritdoc/>
        /// <remarks>
        /// The default value is <c>TLogCategory.Fatal | TLogCategory.Error | TLogCategory.Warning</c>,
        /// meaning these categories are enabled for logging by default.
        /// </remarks>
        public TLogCategory EnabledCategories { get; set; } = TLogCategory.Fatal | TLogCategory.Error | TLogCategory.Warning;

        /// <inheritdoc/>
        /// <remarks>
        /// The default value is <c>~(TLogCategory.Fatal | TLogCategory.Error | TLogCategory.Warning)</c>,
        /// meaning these categories are disabled by default and no other categories are explicitly disabled.
        /// </remarks>
        public TLogCategory DisabledCategories => ~(TLogCategory.Fatal | TLogCategory.Error | TLogCategory.Warning);

        /// <summary>
        /// Indicates whether to clear the log entries when they are retrieved.
        /// If set to <c>true</c>, the log items will be cleared when <see cref="UserLogProvider.GetItems"/> is called.
        /// The default value is <c>true</c>.
        /// </summary>
        public bool IsClearLogsOnGet { get; set; } = true;
    }
}
