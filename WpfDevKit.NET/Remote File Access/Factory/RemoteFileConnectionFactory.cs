using System;
using System.Diagnostics;

namespace WpfDevKit.RemoteFileAccess
{
    /// <summary>
    /// Provides a factory for creating disposable remote file share connections using network credentials.
    /// </summary>
    [DebuggerStepThrough]
    internal class RemoteFileConnectionFactory : IRemoteFileConnectionFactory
    {
        /// <summary>
        /// Creates a new <see cref="IRemoteFileConnection"/> (or <see cref="IDisposable"/>) instance for accessing a remote file share.
        /// </summary>
        /// <param name="path">The UNC path to the remote share (e.g., <c>\\server\share</c>).</param>
        /// <param name="username">The user name used to authenticate the remote connection.</param>
        /// <param name="password">The password used to authenticate the remote connection.</param>
        /// <param name="domain">An optional domain or machine name associated with the user. Defaults to <c>null</c>.</param>
        /// <returns>A disposable object that represents the connection to the remote file share.</returns>
        /// <exception cref="OpenRemoteConnectionException">Thrown if the remote connection could not be established.</exception>
        public IRemoteFileConnection Create(string path, string username, string password, string domain = null) =>
            new RemoteFileConnection(path, username, password, domain);
    }
}
