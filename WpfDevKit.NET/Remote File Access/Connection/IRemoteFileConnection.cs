using System;

namespace WpfDevKit.RemoteFileAccess
{
    /// <summary>
    /// Represents an authenticated connection to a remote network share (e.g. \\server\share).
    /// Implementations are expected to manage the connection lifecycle and ensure proper cleanup.
    /// </summary>
    public interface IRemoteFileConnection : IDisposable
    {
        /// <summary>
        /// Gets the remote path that this connection was established to.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets a value indicating whether the remote connection is currently active.
        /// </summary>
        bool IsConnected { get; }
    }
}
