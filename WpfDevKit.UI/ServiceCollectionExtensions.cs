using WpfDevKit.DependencyInjection;
using WpfDevKit.UI.Command;
using WpfDevKit.UI.Dialogs;
using WpfDevKit.UI.Synchronization.Collections;
using WpfDevKit.UI.Synchronization.Context;

namespace WpfDevKit.UI
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUIServices<TMainViewModel>(this IServiceCollection services) where TMainViewModel : class => 
            services.AddCollectionSynchronization()
                    .AddCommandFactory()
                    .AddContextSynchronization()
                    .AddDialogService()
                    .AddSingleton<TMainViewModel>();
    }
}
