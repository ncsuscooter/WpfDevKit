using System;

namespace WpfDevKit.RemoteFileAccess
{
    /// <summary>
    /// Provides a factory to create instances of <see cref="IRemoteFileConnection"/> at runtime.
    /// </summary>
    public interface IRemoteFileConnectionFactory
    {
        /// <summary>
        /// Creates a connection to the given remote path using the specified credentials.
        /// </summary>
        /// <param name="path">The UNC path to the network share.</param>
        /// <param name="username">The user name.</param>
        /// <param name="password">The password.</param>
        /// <param name="domain">The domain or machine name, if applicable.</param>
        /// <returns>A disposable remote connection instance.</returns>
        IDisposable Create(string path, string username, string password, string domain = null);
    }
}
