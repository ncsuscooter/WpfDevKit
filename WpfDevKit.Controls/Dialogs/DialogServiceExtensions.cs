using WpfDevKit.DependencyInjection;

namespace WpfDevKit.UI.Dialogs
{
    /// <summary>
    /// Provides extension methods for registering WpfDevKit UI services.
    /// </summary>
    public static class DialogServiceExtensions
    {
        /// <summary>
        /// Registers WpfDevKit Controls services.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <returns>The current IServiceCollection instance for chaining.</returns>
        public static IServiceCollection AddDialogService(this IServiceCollection services)
        {
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
