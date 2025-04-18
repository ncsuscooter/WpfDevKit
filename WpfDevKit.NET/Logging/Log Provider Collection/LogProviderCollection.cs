using System;
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
        private readonly LogService logService;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogProviderCollection"/> class.
        /// </summary>
        /// <param name="logService">The log service.</param>
        public LogProviderCollection(LogService logService) => this.logService = logService ?? throw new ArgumentNullException(nameof(logService));

        /// <inheritdoc/>
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
                    logService.LogTrace("Provider was not added to the collection", $"Name='{provider.GetType().Name}' - Key='{key}'", default);
                    return false;
                }
                providers.Add((provider, key));
                logService.LogTrace("Provider was successfully added to the collection", $"Name='{provider.GetType().Name}' - Key='{key}'", default);
                return true;
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }
        }

        /// <inheritdoc/>
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
                    logService.LogTrace("Provider was successfully removed from the collection", $"Name='{provider.GetType().Name}' - Key='{key}'", default);
                    return true;
                }
                logService.LogTrace("Provider was not removed from the collection", $"Name='{provider.GetType().Name}' - Key='{key}'", default);
                return false;
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }
        }

        /// <inheritdoc/>
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
