namespace WpfDevKit.Connectivity
{
    /// <summary>
    /// Represents the current connectivity status of a monitored system or service.
    /// </summary>
    public enum TConnectivityState
    {
        /// <summary>
        /// The system is not currently connected and is not attempting to connect.
        /// </summary>
        Disconnected,

        /// <summary>
        /// The system is actively attempting to establish a connection.
        /// </summary>
        Connecting,

        /// <summary>
        /// The system is successfully connected.
        /// </summary>
        Connected
    }
}
