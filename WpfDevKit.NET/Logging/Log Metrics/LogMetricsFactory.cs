using System.Diagnostics;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Default factory for creating typed instances of <see cref="ILogMetrics{T}"/>.
    /// </summary>
    [DebuggerStepThrough]
    internal class LogMetricsFactory : ILogMetricsFactory
    {
        /// <inheritdoc/>
        public ILogMetrics<TLogProvider> Create<TLogProvider>() => new LogMetrics<TLogProvider>();
    }
}
