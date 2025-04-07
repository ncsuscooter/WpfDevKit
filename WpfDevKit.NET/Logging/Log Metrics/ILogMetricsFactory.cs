namespace WpfDevKit.Logging
{
    /// <summary>
    /// Provides a factory for creating typed instances of <see cref="ILogMetrics{T}"/> for use in custom log providers.
    /// </summary>
    public interface ILogMetricsFactory
    {
        /// <summary>
        /// Creates a new instance of <see cref="ILogMetrics{TLogProvider}"/> for the specified log provider type.
        /// </summary>
        /// <typeparam name="TLogProvider">The type of the log provider for which to create metrics.</typeparam>
        /// <returns>A new instance of <see cref="ILogMetrics{TLogProvider}"/>.</returns>
        ILogMetrics<TLogProvider> Create<TLogProvider>();
    }
}
