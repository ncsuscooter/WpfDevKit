using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Provides a dynamic, thread-safe manager for handling logging service providers.
    /// </summary>
    internal class LogProviderCollection : ILogProviderCollection
    {
        private readonly List<(ILogProvider Provider, string Key)> providers = new List<(ILogProvider, string)>();
        private readonly ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();
        private readonly LogOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogProviderCollection"/> class.
        /// </summary>
        /// <param name="LogProviderCollectionOptions">The options used for service configuration.</param>
        public LogProviderCollection(LogOptions options) => this.options = options;

        /// <summary>
        /// Attempts to add a logging provider to the manager if it is not already present.
        /// </summary>
        /// <param name="provider">The logging provider to add.</param>
        /// <param name="key">The key associated with the logging provider.</param>
        /// <returns><c>true</c> if the provider was added; otherwise, <c>false</c>.</returns>
        public bool TryAddProvider(ILogProvider provider, string key = default)
        {
            if (provider == null)
                return false;
            readerWriterLock.EnterWriteLock();
            try
            {
                bool exists = providers.Any(p => p.Provider.GetType() == provider.GetType() && p.Key == key);
                if (exists)
                {
                    options?.LogMessage?.Invoke("Provider was not added to the collection", $"Name='{provider.GetType().Name}' - Key='{key}'", default);
                    return false;
                }
                providers.Add((provider, key));
                options?.LogMessage?.Invoke("Provider was successfully added to the collection", $"Name='{provider.GetType().Name}' - Key='{key}'", default);
                return true;
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Attempts to remove a logging provider from the manager.
        /// </summary>
        /// <param name="provider">The logging provider to remove.</param>
        /// <param name="key">The key associated with the logging provider.</param>
        /// <returns><c>true</c> if the provider was removed; otherwise, <c>false</c>.</returns>
        public bool TryRemoveProvider(ILogProvider provider, string key = default)
        {
            if (provider == null)
                return false;
            readerWriterLock.EnterWriteLock();
            try
            {
                var existing = providers.FirstOrDefault(p => p.Provider.GetType() == provider.GetType() && p.Key == key);
                if (existing.Provider != null)
                {
                    providers.Remove(existing);
                    options?.LogMessage?.Invoke("Provider was successfully removed from the collection", $"Name='{provider.GetType().Name}' - Key='{key}'", default);
                    return true;
                }
                options?.LogMessage?.Invoke("Provider was not removed from the collection", $"Name='{provider.GetType().Name}' - Key='{key}'", default);
                return false;
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Retrieves the currently managed logging service providers.
        /// </summary>
        /// <returns>A collection of managed logging service providers with their keys.</returns>
        public IEnumerable<(ILogProvider Provider, string Key)> GetProviders()
        {
            readerWriterLock.EnterReadLock();
            try
            {
                return providers.ToList();
            }
            finally
            {
                readerWriterLock.ExitReadLock();
            }
        }
    }
}
