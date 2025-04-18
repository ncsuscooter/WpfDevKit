using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Provides a dynamic, thread-safe manager for handling logging service providers.
    /// </summary>
    internal partial class LogProviderCollection : ILogProviderCollection
    {
        private readonly HashSet<LogProviderDescriptor> descriptors = new HashSet<LogProviderDescriptor>();
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
            var descriptor = new LogProviderDescriptor(provider, key);
            readerWriterLock.EnterWriteLock();
            try
            {
                if (!descriptors.Add(descriptor))
                {
                    logService.LogTrace("Provider was not added to the collection", descriptor.ToString(), default);
                    return false;
                }
                logService.LogTrace("Provider was successfully added to the collection", descriptor.ToString(), default);
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
            var descriptor = new LogProviderDescriptor(provider, key);
            readerWriterLock.EnterWriteLock();
            try
            {
                if (!descriptors.Remove(descriptor))
                {
                    logService.LogTrace("Provider was not removed from the collection", descriptor.ToString(), default);
                    return false;
                }
                logService.LogTrace("Provider was successfully removed from the collection", descriptor.ToString(), default);
                return true;
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
        public IEnumerable<LogProviderDescriptor> GetProviders()
        {
            readerWriterLock.EnterReadLock();
            try
            {
                return descriptors.ToList();
            }
            finally
            {
                readerWriterLock.ExitReadLock();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ILogProviderDescriptor> GetProviderInfos() => GetProviders().Cast<ILogProviderDescriptor>();
    }
}
