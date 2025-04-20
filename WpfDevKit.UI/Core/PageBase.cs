using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
        protected readonly ILogService logService;
        protected readonly IDialogService dialogService;

        private IObservable selectedItem;

        /// <summary>
        /// The message to indicate that the base method should be overridden to prevent execution.
        /// </summary>
        protected const string OVERRIDE_MESSAGE = "Override the base method to prevent execution";

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
            (this.dialogService, this.logService) = (dialogService, logService);
            logService.LogDebug(type: GetType());
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanging"/> event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that is changing.</param>
        public override void OnPropertyChanging([CallerMemberName] string propertyName = null)
        {
            try
            {
                if (propertyName.Equals(nameof(SelectedItem)))
                {
                    logService.LogDebug(null, $"{nameof(propertyName)}='{propertyName}'", GetType());
                    if (SelectedItem is IDisposable disposable)
                        disposable.Dispose();
                    if (SelectedItem != null)
                        SelectedItem.PropertyChanged -= OnSelectedItemPropertyChanged;
                }
            }
            catch (Exception ex)
            {
                dialogService.ShowDialog(ex, GetType());
            }
            base.OnPropertyChanging(propertyName);
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        public override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            try
            {
                if (propertyName.Equals(nameof(SelectedItem)))
                {
                    logService.LogDebug(null, $"{nameof(propertyName)}='{propertyName}'", GetType());
                    if (SelectedItem != null)
                        SelectedItem.PropertyChanged += OnSelectedItemPropertyChanged;
                }
            }
            catch (Exception ex)
            {
                dialogService.ShowDialog(ex, GetType());
            }
            base.OnPropertyChanged(propertyName);
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
