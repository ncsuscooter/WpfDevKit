using System;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Represents an internal implementation of <see cref="ILogProviderDescriptor"/> that associates a logging provider
    /// with a unique key and its corresponding metrics.
    /// </summary>
    internal class LogProviderDescriptor : ILogProviderDescriptor, IEquatable<ILogProviderDescriptor>
    {
        /// <inheritdoc/>
        public string Key { get; private set; }

        /// <inheritdoc/>
        public Type ProviderType => Provider.GetType();

        /// <inheritdoc/>
        ILogMetricsReader ILogProviderDescriptor.Metrics => Metrics;

        /// <summary>
        /// Gets the actual <see cref="ILogProvider"/> instance.
        /// </summary>
        public ILogProvider Provider { get; private set; }

        /// <summary>
        /// Gets the internal metrics associated with this provider.
        /// </summary>
        public LogMetrics Metrics { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogProviderDescriptor"/> class.
        /// </summary>
        /// <param name="provider">The logging provider instance.</param>
        /// <param name="key">A unique key to identify this provider instance.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="provider"/> or <paramref name="key"/> is null.</exception>
        public LogProviderDescriptor(ILogProvider provider, string key = default)
        {
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            Key = key;
            Metrics = new LogMetrics();
        }

        /// <summary>
        /// Compares this descriptor to another <see cref="ILogProviderDescriptor"/> for equality.
        /// </summary>
        /// <param name="other">The other descriptor to compare to.</param>
        /// <returns><c>true</c> if both provider type and key are equal; otherwise, <c>false</c>.</returns>
        public bool Equals(ILogProviderDescriptor other) => Equals(other);

        /// <summary>
        /// Determines whether the specified object is equal to the current instance.
        /// </summary>
        public override bool Equals(object obj) =>
            obj is ILogProviderDescriptor other &&
            ProviderType == other.ProviderType &&
            Key == other.Key;

        /// <summary>
        /// Returns a hash code for the current instance based on provider type and key.
        /// </summary>
        public override int GetHashCode()
        {
            int hashCode = -1726572946;
            hashCode = hashCode * -1521134295 + ProviderType.GetHashCode();
            hashCode = hashCode * -1521134295 + (!string.IsNullOrWhiteSpace(Key) ? Key.GetHashCode() : 0);
            return hashCode;
        }

        /// <summary>
        /// Returns a string representation of the provider descriptor.
        /// </summary>
        /// <returns>A formatted string with provider type name and key.</returns>
        public override string ToString() => $"Type='{ProviderType.Name}' - Key='{Key}'";
    }
}
