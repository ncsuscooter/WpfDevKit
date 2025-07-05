using System;
using System.Diagnostics;

namespace WpfDevKit.RemoteFileAccess
{
    /// <summary>
    /// Represents a base exception for remote share connection failures.
    /// </summary>
    [DebuggerStepThrough]
    public abstract class RemoteConnectionException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteConnectionException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="path">The path associated with the connection.</param>
        /// <param name="code">The native error code returned by the API.</param>
        protected RemoteConnectionException(string message, string path, uint code)
            : base(message)
        {
            Data["Path"] = path;
            Data["Error"] = code;
        }
    }

    /// <summary>
    /// Thrown when a remote file connection fails to open.
    /// </summary>
    [DebuggerStepThrough]
    public sealed class OpenRemoteConnectionException : RemoteConnectionException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenRemoteConnectionException"/> class.
        /// </summary>
        public OpenRemoteConnectionException(string path, string user, uint code, uint index)
            : base("Failed to open connection to remote file share", path, code)
        {
            Data["User"] = user;
            Data["Index"] = index;
        }
    }

    /// <summary>
    /// Thrown when a remote file connection fails to close.
    /// </summary>
    [DebuggerStepThrough]
    public sealed class CloseRemoteConnectionException : RemoteConnectionException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CloseRemoteConnectionException"/> class.
        /// </summary>
        public CloseRemoteConnectionException(string path, uint code)
            : base("Failed to close connection to remote file share", path, code) { }
    }
}
