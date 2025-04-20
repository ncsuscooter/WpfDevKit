using WpfDevKit.DependencyInjection;
using WpfDevKit.UI.CollectionSynchronization;
using WpfDevKit.UI.Command;
using WpfDevKit.UI.ContextSynchronization;
using WpfDevKit.UI.Dialogs;

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
