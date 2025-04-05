using System.Linq;
using WpfDevKit.Controls.Dialogs;
using WpfDevKit.Controls.Dialogs.Interfaces;
using WpfDevKit.Controls.Services;
using WpfDevKit.DependencyInjection.Extensions;
using WpfDevKit.DependencyInjection.Interfaces;
using WpfDevKit.Interfaces;
using WpfDevKit.Logging.Interfaces;
using WpfDevKit.Mvvm;

namespace WpfDevKit.Controls.Extensions
{
    /// <summary>
    /// Provides extension methods for registering WpfDevKit.Controls services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers WpfDevKit Controls services.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <returns>The current IServiceCollection instance for chaining.</returns>
        public static IServiceCollection AddControls(this IServiceCollection services)
        {
            services.AddSingleton<ICollectionSynchronizationService, CollectionSynchronizationService>();
            services.AddSingleton<IContextService, ContextService>();
            services.AddSingleton<ICommandFactory, CommandFactory>();
            services.AddSingleton<IDialogService>(provider =>
            {
                var commandFactory = provider.GetService<ICommandFactory>();
                var logService = provider.GetService<ILogService>();
                var busyService = provider.GetService<IBusyService>();
                var userLogProvider = provider.GetService<ILogProviderCollection>()
                                              .GetProviders()
                                              .OfType<IUserLogProvider>()
                                              .FirstOrDefault();
                return new DialogService(commandFactory, logService, busyService, userLogProvider);
            });
            return services;
        }
    }
}
