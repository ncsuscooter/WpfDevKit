using System.Net;
using System.Windows;
using System.Windows.Threading;
using WpfDevKit.Busy;
using WpfDevKit.Connectivity;
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
            var builder = HostBuilder.CreateHostBuilder();
            builder.Services.AddContextSynchronization()
                            .AddCollectionSynchronization()
                            .AddCommandFactory()
                            .AddDialogService()
                            .AddMemoryLogProvider()
                            .AddConsoleLogProvider()
                            .AddUserLogProvider()
                            .AddConnectivityService(options =>
                            {
                                var collection = Dns.GetHostAddresses("dbc");
                                if (collection != null && collection.Length > 0)
                                    options.Host = collection[0].ToString();
                            });

            using (var host = builder.Build())
            {
                await host.StartAsync();
                Current.MainWindow = new MainWindow
                {
                    DataContext = new MainViewModel(host.Services.GetService<IBusyService>(),
                                                    host.Services.GetService<ICommandFactory>(),
                                                    host.Services.GetService<IDialogService>(),
                                                    host.Services.GetService<ILogService>())
                };
                Current.MainWindow.Show();
            }
        }
    }
}
