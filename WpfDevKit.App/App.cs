using System.Net;
using System.Windows;
using System.Windows.Threading;
using WpfDevKit.Controls.Dialogs.Interfaces;
using WpfDevKit.Controls.Extensions;
using WpfDevKit.DependencyInjection.Extensions;
using WpfDevKit.Hosting;
using WpfDevKit.Interfaces;
using WpfDevKit.Logging.Interfaces;

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
                    services.AddBusyService();
                    services.AddConnectivityService(options =>
                    {
                        var collection = Dns.GetHostAddresses("dbc");
                        if (collection != null && collection.Length > 0)
                            options.Host = collection[0].ToString();
                    });
                    services.AddLoggingService();
                    services.AddControls();
                })
                .Build();

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
