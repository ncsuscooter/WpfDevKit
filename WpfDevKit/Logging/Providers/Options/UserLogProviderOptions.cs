using System.Diagnostics;

namespace WpfDevKit.Logging.Providers.Options
{
    /// <summary>
    /// Configuration options for the <see cref="UserLogProvider"/> class, which extends <see cref="MemoryLogProvider"/> for user-specific logging.
    /// </summary>
    [DebuggerStepThrough]
    public class UserLogProviderOptions : MemoryLogProviderOptions
    {
        /// <summary>
        /// Indicates whether to clear the log entries when they are retrieved.
        /// If set to <c>true</c>, the log items will be cleared when <see cref="UserLogProvider.GetItems"/> is called.
        /// The default value is <c>true</c>.
        /// </summary>
        public bool IsClearLogsOnGet { get; set; } = true;
    }
}
