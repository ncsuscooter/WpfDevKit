using System;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Represents a read-only descriptor for a registered logging provider,
    /// including its unique key, implementation type, and associated metrics.
    /// </summary>
    public interface ILogProviderDescriptor
    {
        /// <summary>
        /// Gets the runtime type of the logging provider instance.
        /// </summary>
        Type ProviderType { get; }

        /// <summary>
        /// Gets a metrics reader for accessing logging statistics related to this provider.
        /// </summary>
        ILogMetricsReader Metrics { get; }

        /// <summary>
        /// 
        /// </summary>
        ILogProviderOptions Options { get; }
    }

}