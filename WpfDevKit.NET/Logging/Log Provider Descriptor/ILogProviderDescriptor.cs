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
        /// Gets the unique key associated with the logging provider.
        /// Used to differentiate multiple instances of the same provider type.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Gets the runtime type of the logging provider instance.
        /// </summary>
        Type ProviderType { get; }

        /// <summary>
        /// Gets a metrics reader for accessing logging statistics related to this provider.
        /// </summary>
        ILogMetricsReader Metrics { get; }
    }

}