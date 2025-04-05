using System.Collections.Generic;

namespace WpfDevKit.Logging.Interfaces
{
    /// <summary>
    /// Defines a contract for a user-facing log provider that supports capturing,
    /// storing, and retrieving logs specific to end-user interactions or visibility.
    /// </summary>
    public interface IUserLogProvider
    {
        IReadOnlyCollection<ILogMessage> GetLogMessages();
    }
}
