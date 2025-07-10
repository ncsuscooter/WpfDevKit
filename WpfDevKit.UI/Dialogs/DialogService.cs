using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using WpfDevKit.Busy;
using WpfDevKit.Logging;
using WpfDevKit.UI.Command;
using WpfDevKit.UI.Synchronization.Context;

namespace WpfDevKit.UI.Dialogs
{
    /// <summary>
    /// Provides functionality to display dialog windows.
    /// </summary>
    [DebuggerStepThrough]
    internal class DialogService : IDialogService
    {
        private readonly IBusyService busyService;
        private readonly ICommandFactory commandFactory;
        private readonly IContextSynchronizationService contextService;
        private readonly ILogService logService;
        private readonly ILogSnapshot userLogProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogService"/> class.
        /// </summary>
        /// <param name="busyService">The busy service used to indicate background activity.</param>
        /// <param name="commandFactory">The command factory used to create objects that implment the ICommand interface.</param>
        /// <param name="contextService">The context synchronization service used to synchronize background activity to the UI thread.</param>
        /// <param name="logService">The logging service used for dialog events.</param>
        /// <param name="userLogProvider">The user log provider used to fetch error log details.</param>
        public DialogService(IBusyService busyService,
                             ICommandFactory commandFactory,
                             IContextSynchronizationService contextService,
                             ILogService logService,
                             ILogSnapshot userLogProvider)
        {
            this.busyService = busyService ?? throw new ArgumentNullException(nameof(busyService));
            this.commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            this.contextService = contextService ?? throw new ArgumentNullException(nameof(contextService));
            this.logService = logService ?? throw new ArgumentNullException(nameof(logService));
            this.userLogProvider = userLogProvider ?? throw new ArgumentNullException(nameof(userLogProvider));
        }

        /// <inheritdoc/>
        public void ShowDialog(IDialogContext dialogContext)
        {
            dialogContext.DialogWindow = new DialogWindow
            {
                Owner = DialogWindowContext.GetCurrentOwner(),
                DataContext = dialogContext
            };
            dialogContext.DialogWindow.ShowDialog();
        }

        /// <inheritdoc/>
        public void ShowDialog(Exception exception, Type type = null, [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = null)
        {
            logService.LogError(exception, type, fileName, memberName);
            logService.FlushAsync(TimeSpan.FromMilliseconds(500)).Wait();
            ShowDialog(new DialogBase(busyService, commandFactory, contextService, logService)
            {
                Message = "An error was reported. Please see the error log below for more details.",
                Title = "Error Log",
                DialogImage = TDialogImage.Error,
                DialogButtons = TDialogButtons.Ok,
                Width = 999,
                Height = 550,
                MessageFontSize = 24,
                MessageFontWeight = FontWeights.Bold,
                Logs = userLogProvider?.GetSnapshot()
            });
        }

        /// <inheritdoc/>
        public TDialogResult ShowDialog(string message, string title, TDialogImage image, TDialogButtons buttons)
        {
            var dialog = new DialogBase(busyService, commandFactory, contextService, logService)
            {
                Message = message,
                Title = title,
                DialogImage = image,
                DialogButtons = buttons,
                Width = 800,
                Height = 200,
                MessageFontSize = 18,
                MessageFontWeight = FontWeights.Normal
            };
            ShowDialog(dialog);
            return dialog.DialogResult;
        }

        /// <inheritdoc/>
        public TDialogResult ShowDialog(string message, TDialogImage image, TDialogButtons buttons) => ShowDialog(message, image.ToString(), image, buttons);

        /// <inheritdoc/>
        public TDialogResult ShowDialog(string message, string title, TDialogButtons buttons) => ShowDialog(message, title, TDialogImage.Default, buttons);

        /// <inheritdoc/>
        public TDialogResult ShowDialog(string message, TDialogButtons buttons) => ShowDialog(message, TDialogImage.Default.ToString(), TDialogImage.Default, buttons);

        /// <inheritdoc/>
        public TDialogResult ShowDialog(string message, string title, TDialogImage image) => ShowDialog(message, title, image, TDialogButtons.Ok);

        /// <inheritdoc/>
        public TDialogResult ShowDialog(string message, TDialogImage image) => ShowDialog(message, image.ToString(), image, TDialogButtons.Ok);

        /// <inheritdoc/>
        public TDialogResult ShowDialog(string message, string title) => ShowDialog(message, title, TDialogImage.Default);

        /// <inheritdoc/>
        public TDialogResult ShowDialog(string message) => ShowDialog(message, "Message Box Dialog");
    }
}
