using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WpfDevKit.Busy;
using WpfDevKit.Connectivity;
using WpfDevKit.Hosting;
using WpfDevKit.Logging;
using WpfDevKit.UI.CollectionSynchronization;
using WpfDevKit.UI.Command;
using WpfDevKit.UI.ContextSynchronization;
using WpfDevKit.UI.Core;
using WpfDevKit.UI.Dialogs;

namespace WpfDevKit.App
{
    [DebuggerStepThrough]
    public sealed class MainViewModel : PageBase
    {
        private readonly Timer timer;

        public MainViewModel(IBusyService busyService,
                             ICommandFactory commandFactory,
                             IDialogService dialogService,
                             ILogService logService) : base(busyService, commandFactory, dialogService, logService)
        {
            RegisterPropertyChangedAction(nameof(IsBusy), () =>
            {
                OnPropertyChanged(nameof(IsEnabled));
            });

            timer = new Timer(o => OnPropertyChanged(nameof(CurrentTime)), default, 500, 500);
        }

        public string CurrentTime => DateTime.Now.ToString("M/d/yy h:mm:ss tt");
        public string CurrentVersion => $"version: {GetType().Assembly.GetName().Version}";
        public bool IsEnabled => !IsBusy;

        private async Task foo()
        {
            var builder = HostBuilder.CreateHostBuilder();
            builder.Services.AddContextSynchronization()
                            .AddCollectionSynchronization()
                            .AddCommandFactory()
                            .AddDialogService()
                            .AddConnectivityService(options =>
                            {
                                var collection = Dns.GetHostAddresses("dbc");
                                if (collection != null && collection.Length > 0)
                                    options.Host = collection[0].ToString();
                            });

            using (var host = builder.Build())
            {
                await host.StartAsync();
                Application.Current.MainWindow = new MainWindow
                {
                    DataContext = new MainViewModel(host.Services.GetRequiredService<IBusyService>(),
                                                    host.Services.GetRequiredService<ICommandFactory>(),
                                                    host.Services.GetRequiredService<IDialogService>(),
                                                    host.Services.GetRequiredService<ILogService>())
                };
                Application.Current.MainWindow.Show();
            }
        }
    }
}
