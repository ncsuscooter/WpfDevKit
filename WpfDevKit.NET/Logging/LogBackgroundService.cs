using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WpfDevKit.Hosting;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Represents a background service responsible for processing and logging log messages asynchronously.
    /// </summary>
    [DebuggerStepThrough]
    internal class LogBackgroundService : HostedService
    {
        private readonly ILogProviderCollection logProviderCollection;
        private readonly LogQueue logQueue;
        private readonly InternalLogger options;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogBackgroundService"/> class.
        /// </summary>
        /// <param name="logProviderManager">The manager used to resolve logging providers.</param>
        /// <param name="queue">The queue that holds the log messages to be processed.</param>
        /// <param name="options">The options for configuring the log background service.</param>
        public LogBackgroundService(ILogProviderCollection logProviderCollection, LogQueue logQueue, InternalLogger options) => 
            (this.logProviderCollection, this.logQueue, this.options) = (logProviderCollection, logQueue, options);

        /// <summary>
        /// Executes the background service, processing and logging messages from the queue asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken) => 
            await Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Continuously attempt to read messages from the queue
                    while (logQueue.TryRead(out var message))
                    {
                        // Iterate over all available logging providers and log the message if appropriate
                        foreach (var (provider, key) in logProviderCollection.GetProviders())
                        {
                            try
                            {
                                // Retrieve the logging provider and check if it is enabled for the given category
                                if ((provider.EnabledCategories & message.Category) == 0 ||
                                    (provider.DisabledCategories & message.Category) > 0)
                                    continue;
                                // Log the message asynchronously using the provider
                                await provider.LogAsync(message);
                            }
                            catch (OperationCanceledException)
                            {
                                // INTENTIONALLY LEFT EMPTY: Ignore operation cancellation
                            }
                            catch (Exception ex)
                            {
                                logProviderCollection.TryRemoveProvider(provider);
                                options?.LogException?.Invoke(ex, provider.GetType());
                            }
                        }
                    }
                }
            }, cancellationToken);
    }
}
