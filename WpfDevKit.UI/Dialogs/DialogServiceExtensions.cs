using WpfDevKit.DependencyInjection;

namespace WpfDevKit.UI.Dialogs
{
    /// <summary>
    /// Provides extension methods for registering WpfDevKit UI services.
    /// </summary>
    public static class DialogServiceExtensions
    {
        /// <summary>
        /// Registers the <see cref="IDialogService"/> service.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <returns>The current IServiceCollection instance for chaining.</returns>
        public static IServiceCollection AddDialogService(this IServiceCollection services) => 
            services.AddSingleton<DialogService>()
                    .AddSingleton<IDialogService>(p => p.GetRequiredService<DialogService>());
    }
}
