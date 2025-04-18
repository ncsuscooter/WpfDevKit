using System.Collections.Generic;

using WpfDevKit.Logging;

/// <summary>
/// Defines a manager for handling logging service providers at runtime.
/// Supports dynamic addition, removal, and inspection of logging providers and their associated metadata.
/// </summary>
public interface ILogProviderCollection
{
    /// <summary>
    /// Attempts to add a logging provider to the manager if it is not already present.
    /// </summary>
    /// <param name="provider">The logging provider to add.</param>
    /// <param name="key">An optional key to uniquely identify the provider instance.</param>
    /// <returns><c>true</c> if the provider was added; otherwise, <c>false</c>.</returns>
    bool TryAddProvider(ILogProvider provider, string key = default);

    /// <summary>
    /// Attempts to remove a logging provider from the manager.
    /// </summary>
    /// <param name="provider">The logging provider to remove.</param>
    /// <param name="key">The key associated with the provider to remove.</param>
    /// <returns><c>true</c> if the provider was removed; otherwise, <c>false</c>.</returns>
    bool TryRemoveProvider(ILogProvider provider, string key = default);

    /// <summary>
    /// Retrieves metadata about all currently registered logging providers.
    /// </summary>
    /// <returns>A collection of provider descriptors containing type, key, and metrics information.</returns>
    IEnumerable<ILogProviderDescriptor> GetProviderInfos();
}
