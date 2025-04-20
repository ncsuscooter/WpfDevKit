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
        private readonly LogProviderDescriptorCollection logProviderDescriptorCollection;
        private readonly LogService logService;
        private readonly LogQueue logQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogBackgroundService"/> class.
        /// </summary>
        /// <param name="logProviderDescriptorCollection">The collection of log provider descriptors to help resolve providers, metrics, and options.</param>
        /// <param name="logService">The service used to log messages or exceptions while processing messages.</param>
        /// <param name="logQueue">The queue that holds the log messages to be processed.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the object's required arguments are null</exception>
        public LogBackgroundService(LogProviderDescriptorCollection logProviderDescriptorCollection, LogService logService, LogQueue logQueue)
        {
            this.logProviderDescriptorCollection = logProviderDescriptorCollection ?? throw new ArgumentNullException(nameof(logProviderDescriptorCollection));
            this.logService = logService ?? throw new ArgumentNullException(nameof(logService));
            this.logQueue = logQueue ?? throw new ArgumentNullException(nameof(logQueue));
        }

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
                        foreach (var descriptor in logProviderDescriptorCollection.GetDescriptors())
                        {
                            using (descriptor.Metrics.StartStop(message))
                            {
                                try
                                {
                                    // Retrieve the logging provider and check if it is enabled for the given category
                                    if ((descriptor.Options.EnabledCategories & message.Category) == 0 ||
                                        (descriptor.Options.DisabledCategories & message.Category) > 0)
                                    {
                                        descriptor.Metrics.IncrementLost();
                                        continue;
                                    }
                                    descriptor.Metrics.IncrementQueued();
                                    await descriptor.Provider.LogAsync(message); // Log the message asynchronously using the provider
                                }
                                catch (OperationCanceledException)
                                {
                                    // INTENTIONALLY LEFT EMPTY
                                }
                                catch (Exception ex)
                                {
                                    descriptor.Options.EnabledCategories = TLogCategory.None;
                                    logService.LogError(ex, descriptor.ProviderType);
                                }
                            }
                        }
                    }
                }
            }, cancellationToken);
    }
}
