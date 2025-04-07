using System;
using System.Diagnostics;
using System.IO;

namespace WpfDevKit.RemoteFileAccess
{
    /// <summary>
    /// Provides a disposable mechanism to open and close a connection to a remote file share using native Windows APIs.
    /// </summary>
    [DebuggerStepThrough]
    internal class RemoteFileConnection : IDisposable
    {
        private readonly string remotePath;
        private readonly bool isAlreadyConnected;

        /// <summary>
        /// Initializes a new connection to the specified remote file share if it is not already connected.
        /// </summary>
        /// <param name="path">The UNC path to the remote share (e.g., <c>\\server\share</c>).</param>
        /// <param name="username">The username used to authenticate the connection.</param>
        /// <param name="password">The password used to authenticate the connection.</param>
        /// <param name="domainname">The domain name of the user, or <c>null</c> for local accounts.</param>
        /// <exception cref="OpenRemoteConnectionException">Thrown if the connection attempt fails.</exception>
        public RemoteFileConnection(string path, string username, string password, string domainname = null)
        {
            remotePath = path;
            isAlreadyConnected = Directory.Exists(path);
            if (!isAlreadyConnected)
            {
                var useInfo = new USE_INFO_2
                {
                    ui2_remote = path,
                    ui2_username = username,
                    ui2_domainname = domainname,
                    ui2_password = password,
                    ui2_asg_type = 0xFFFFFFFF
                };
                var result = Imports.NetUseAdd(null, 2, ref useInfo, out uint paramErrorIndex);
                if (result != 0)
                    throw new OpenRemoteConnectionException(path, username, result, paramErrorIndex);
            }
        }

        /// <summary>
        /// Closes the remote connection if it was opened by this instance.
        /// </summary>
        /// <exception cref="CloseRemoteConnectionException">Thrown if the disconnection fails.</exception>
        public void Dispose()
        {
            if (isAlreadyConnected)
                return;
            var result = Imports.NetUseDel(null, remotePath, TForceCond.USE_LOTS_OF_FORCE);
            if (result != 0)
                throw new CloseRemoteConnectionException(remotePath, result);
        }
    }
}
