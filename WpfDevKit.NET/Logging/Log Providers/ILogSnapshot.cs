using System.Collections.Generic;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Defines a contract for a user-facing log provider that supports capturing,
    /// storing, and retrieving logs specific to end-user interactions or visibility.
    /// </summary>
    public interface ILogSnapshot
    {
        /// <summary>
        /// Retrieves the stored log messages and optionally clears the stored messages 
        /// based on the <see cref="UserLogProviderOptions.IsClearLogsOnGet"/> setting.
        /// </summary>
        /// <returns>A read-only collection of the stored log messages.</returns>
        IReadOnlyCollection<ILogMessage> GetSnapshot();
    }
}
