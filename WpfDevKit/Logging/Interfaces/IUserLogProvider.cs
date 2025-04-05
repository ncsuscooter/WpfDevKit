using System.Collections.Generic;

namespace WpfDevKit.Logging.Interfaces
{
    public interface IUserLogProvider
    {
        IReadOnlyCollection<ILogMessage> GetLogMessages();
    }
}
