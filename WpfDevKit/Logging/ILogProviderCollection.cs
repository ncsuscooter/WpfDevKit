using System.Collections.Generic;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Defines a manager for handling logging service providers at runtime.
    /// </summary>
    public interface ILogProviderCollection
    {
        /// <summary>
        /// Attempts to add a logging provider to the manager if it is not already present.
        /// </summary>
        /// <param name="provider">The logging provider to add.</param>
        /// <param name="key">The key associated with the logging provider.</param>
        /// <returns><c>true</c> if the provider was added; otherwise, <c>false</c>.</returns>
        bool TryAddProvider(ILogProvider provider, string key = default);

        /// <summary>
        /// Attempts to remove a logging provider from the manager.
        /// </summary>
        /// <param name="provider">The logging provider to remove.</param>
        /// <param name="key">The key associated with the logging provider.</param>
        /// <returns><c>true</c> if the provider was removed; otherwise, <c>false</c>.</returns>
        bool TryRemoveProvider(ILogProvider provider, string key = default);

        /// <summary>
        /// Retrieves the currently managed logging service providers.
        /// </summary>
        /// <returns>A collection of managed logging service providers with their keys.</returns>
        IEnumerable<(ILogProvider Provider, string Key)> GetProviders();
    }
}
