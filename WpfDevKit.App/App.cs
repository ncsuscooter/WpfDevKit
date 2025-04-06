using System;
using System.Net;
using System.Windows;
using System.Windows.Threading;
using WpfDevKit.Busy;
using WpfDevKit.Connectivity;
using WpfDevKit.DependencyInjection;
using WpfDevKit.Hosting;
using WpfDevKit.Logging;
using WpfDevKit.UI.CollectionSynchronization;
using WpfDevKit.UI.Command;
using WpfDevKit.UI.ContextSynchronization;
using WpfDevKit.UI.Dialogs;

namespace WpfDevKit.App
{
    public partial class App
    {
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) => MessageBox.Show(e.Exception.ToString());
        private async void OnApplicationStartup(object sender, StartupEventArgs e)
        {
            var host = new ServiceHostBuilder()
                .ConfigureServices(services =>
                {
                    services
                    .AddBusyService()
                    .AddConnectivityService(options =>
                    {
                        var collection = Dns.GetHostAddresses("dbc");
                        if (collection != null && collection.Length > 0)
                            options.Host = collection[0].ToString();
                    })
                    .AddLoggingService()
                    .AddContextSynchronization()
                    .AddCollectionSynchronization()
                    .AddCommandFactory()
                    .AddDialogService();
                })
                .Build();
            //host.ServiceProvider.AddLogProvider()
            //public static IServiceProvider AddLogProvider<TOptions>(this IServiceProvider provider, ILogProvider logProvider, Action<TOptions> configure, string key = null)
            //{
            //    var collection = provider.GetService<ILogProviderCollection>();
            //    collection.TryAddProvider(logProvider, key);
            //    services.AddOptions<MemoryLogProviderOptions>();
            //    services.AddOptions<ConsoleLogProviderOptions>();
            //    services.AddOptions<UserLogProviderOptions>();
            //    var collection = new LogProviderCollection(provider.GetService<ILogService>());
            //    collection.TryAddProvider(new MemoryLogProvider(provider.GetService<IOptions<MemoryLogProviderOptions>>().Value));
            //    collection.TryAddProvider(new ConsoleLogProvider(provider.GetService<IOptions<ConsoleLogProviderOptions>>().Value));
            //    collection.TryAddProvider(new UserLogProvider(provider.GetService<IOptions<UserLogProviderOptions>>().Value));
            //}
            //public static IServiceProvider AddLogProvider<TOptions>(this IServiceProvider provider, ILogProvider logProvider, Action<TOptions> configure, string key = null)
            //{
            //    var collection = provider.GetService<ILogProviderCollection>();
            //    collection.TryAddProvider(logProvider, key);
            //    return provider;
            //}
            await host.RunAsync();
            Current.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(host.ServiceProvider.GetService<IBusyService>(),
                                                host.ServiceProvider.GetService<ICommandFactory>(),
                                                host.ServiceProvider.GetService<IDialogService>(),
                                                host.ServiceProvider.GetService<ILogService>())
            };
            Current.MainWindow.Show();
        }
    }
}
