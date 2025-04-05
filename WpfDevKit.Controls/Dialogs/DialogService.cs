using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using WpfDevKit.Controls.Dialogs.Enums;
using WpfDevKit.Controls.Dialogs.Interfaces;
using WpfDevKit.Interfaces;
using WpfDevKit.Logging.Extensions;
using WpfDevKit.Logging.Interfaces;

namespace WpfDevKit.Controls.Dialogs
{
    /// <summary>
    /// Provides functionality to display dialog windows.
    /// </summary>
    [DebuggerStepThrough]
    internal class DialogService : IDialogService
    {
        private readonly ICommandFactory commandFactory;
        private readonly ILogService logService;
        private readonly IBusyService busyService;
        private readonly IUserLogProvider userLogProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogService"/> class.
        /// </summary>
        /// <param name="logService">The logging service used for dialog events.</param>
        /// <param name="busyService">The busy service used to indicate background activity.</param>
        /// <param name="userLogProvider">The user log provider used to fetch error log details.</param>
        public DialogService(ICommandFactory commandFactory, ILogService logService, IBusyService busyService, IUserLogProvider userLogProvider) =>
            (this.commandFactory, this.logService, this.busyService, this.userLogProvider) = (commandFactory, logService, busyService, userLogProvider);

        /// <inheritdoc/>
        public void ShowDialog(IDialogContext dialogContext)
        {
            dialogContext.DialogWindow = new DialogWindow
            {
                Owner = new Window(), // TODO: Pass MainWindow object here
                DataContext = dialogContext
            };
            dialogContext.DialogWindow.ShowDialog();
        }

        /// <inheritdoc/>
        public void ShowDialog(Exception exception, Type type = null, [CallerFilePath] string fileName = null, [CallerMemberName] string memberName = null)
        {
            logService.LogError(exception, type, fileName, memberName);
            
            // logService.Flush() is the better solution, but not yet created
            // Sleep to allow background logging to process
            Thread.Sleep(500);

            ShowDialog(new DialogBase(commandFactory, logService, busyService)
            {
                Message = "An error was reported. Please see the error log below for more details.",
                Title = "Error Log",
                DialogImage = TDialogImage.Error,
                DialogButtons = TDialogButtons.Ok,
                Width = 999,
                Height = 550,
                MessageFontSize = 24,
                MessageFontWeight = FontWeights.Bold,
                Logs = userLogProvider?.GetLogMessages()
            });
        }

        /// <inheritdoc/>
        public TDialogResult ShowDialog(string message, string title, TDialogImage image, TDialogButtons buttons)
        {
            var dialog = new DialogBase(commandFactory, logService, busyService)
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
