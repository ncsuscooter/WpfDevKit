using System;
using System.ComponentModel;
using WpfDevKit.Busy;
using WpfDevKit.Logging;
using WpfDevKit.UI.Command;
using WpfDevKit.UI.Dialogs;

namespace WpfDevKit.UI.Core
{
    /// <summary>
    /// Provides a base class for pages that support commands and handle selected item changes.
    /// Inherits from <see cref="CommandPageBase"/> and implements <see cref="IDisposable"/>.
    /// </summary>
    public abstract class PageBase : CommandPageBase, IDisposable
    {
        /// <summary>
        /// The message to indicate that the base method should be overridden to prevent execution.
        /// </summary>
        protected const string OVERRIDE_MESSAGE = "Override the base method to prevent execution";

        private readonly IBusyService busyService;
        private readonly ICommandFactory commandFactory;
        private readonly IDialogService dialogService;
        private readonly ILogService logService;
        private IObservable selectedItem;

        /// <summary>
        /// Gets or sets the currently selected item.
        /// </summary>
        public IObservable SelectedItem
        {
            get => selectedItem;
            set => SetValue(ref selectedItem, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageBase"/> class.
        /// </summary>
        protected PageBase(IBusyService busyService, ICommandFactory commandFactory, IDialogService dialogService, ILogService logService)
            : base(busyService, commandFactory, logService)
        {
            this.busyService = busyService ?? throw new ArgumentNullException(nameof(busyService));
            this.commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.logService = logService ?? throw new ArgumentNullException(nameof(logService));
            this.logService.LogDebug(type: GetType());
            RegisterPropertyChangingAction(nameof(SelectedItem), SelectedItemChanging);
            RegisterPropertyChangedAction(nameof(SelectedItem), SelectedItemChanged);
        }

        /// <summary>
        /// 
        /// </summary>
        private void SelectedItemChanging()
        {
            try
            {
                if (SelectedItem is IDisposable disposable)
                    disposable.Dispose();
                if (SelectedItem != null)
                    SelectedItem.PropertyChanged -= OnSelectedItemPropertyChanged;
            }
            catch (Exception ex)
            {
                dialogService.ShowDialog(ex, GetType());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SelectedItemChanged()
        {
            try
            {
                if (SelectedItem != null)
                    SelectedItem.PropertyChanged += OnSelectedItemPropertyChanged;
            }
            catch (Exception ex)
            {
                dialogService.ShowDialog(ex, GetType());
            }
        }

        /// <summary>
        /// Disposes the page and cleans up any resources.
        /// </summary>
        public override void Dispose()
        {
            if (isDisposed)
                return;
            logService.LogDebug(type: GetType());
            try
            {
                SelectedItem = null;
            }
            finally
            {
                base.Dispose();
            }
        }

        private void OnSelectedItemPropertyChanged(object sender, PropertyChangedEventArgs e) => OnPropertyChanged($"{nameof(SelectedItem)}.{e.PropertyName}");
    }
}
