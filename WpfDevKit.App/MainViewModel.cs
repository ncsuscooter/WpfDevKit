using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WpfDevKit.Busy;
using WpfDevKit.Logging;
using WpfDevKit.UI.Command;
using WpfDevKit.UI.Core;
using WpfDevKit.UI.Dialogs;

namespace WpfDevKit.App
{
    [DebuggerStepThrough]
    public sealed class MainViewModel : PageBase
    {

        private readonly Timer timer;

        public MainViewModel(IBusyService busyService, ICommandFactory commandFactory, IDialogService dialogService, ILogService logService) : base(busyService, commandFactory, dialogService, logService)
        {
            busyService.IsBusyChanged += () =>
            {
                OnPropertyChanged(nameof(IsBusy));
                OnPropertyChanged(nameof(IsEnabled));
            };
            timer = new Timer(o => OnPropertyChanged(nameof(CurrentTime)), default, 500, 500);
        }

        public string CurrentTime => DateTime.Now.ToString("M/d/yy h:mm:ss tt");
        public string CurrentVersion => $"version: {GetType().Assembly.GetName().Version}";
        public bool IsEnabled => !busyService.IsBusy;
        public bool IsBusy => busyService.IsBusy;

        protected override async Task DoPerformCommandAsync(string commandName, CancellationToken cancellationToken = default)
        {
            try
            {
                logService.LogDebug(null, $"{nameof(commandName)}='{commandName}'");
                using (busyService.Busy())
                {
                    await Task.Delay(50);
                    switch (commandName)
                    {
                        default:
                            logService.LogWarning(NO_ACTION_MESSAGE);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                dialogService.ShowDialog(ex);
            }
        }
    }
}
