using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using WpfDevKit.Busy;
using WpfDevKit.Logging;
using WpfDevKit.UI.Command;
using WpfDevKit.UI.Core;
using WpfDevKit.UI.Synchronization.Context;

namespace WpfDevKit.UI.Dialogs
{
    /// <summary>
    /// Represents a base dialog window implementation with support for various configurations,
    /// button visibility, and logging.
    /// </summary>
    [DebuggerStepThrough]
    public class DialogBase : CommandPageBase, IDialogContext
    {
        private static readonly FontWeightConverter fontWeightConverter = new FontWeightConverter();

        private double width = 800;
        private double height = 200;
        private string title = "Title";
        private string message = "Message";
        private FontWeight fontWeight = FontWeights.Normal;
        private int fontSize = 18;
        private bool isYesNoVisible;
        private bool isCancelVisible;
        private bool isOKVisible;
        private bool isCloseVisible;
        private bool isButtonBarVisible;
        private bool isMessageBarVisible;
        private bool isMessageLogVisible;
        private IDialogWindow dialogWindow;
        private IReadOnlyCollection<ILogMessage> logs;
        private TDialogResult dialogResult = TDialogResult.None;
        private Uri imageSource = new Uri("Images/Default.png", UriKind.Relative);

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogBase"/> class.
        /// </summary>
        /// <param name="busyService">The busy service used to indicate background activity.</param>
        /// <param name="commandFactory">The command factory used to create objects that implment the ICommand interface.</param>
        /// <param name="contextService">The context synchronization service used to synchronize background activity to the UI thread.</param>
        /// <param name="logService">The logging service used for dialog events.</param>
        public DialogBase(IBusyService busyService,
                          ICommandFactory commandFactory,
                          IContextSynchronizationService contextService,
                          ILogService logService) : base(busyService, commandFactory, contextService, logService)
        {
            RegisterPropertyChangedAction(nameof(Message), () => IsMessageBarVisible = !string.IsNullOrWhiteSpace(Message));
            RegisterPropertyChangedAction(nameof(Logs), () => IsMessageLogVisible = Logs.Count > 0);
            RegisterPropertyChangedAction(nameof(DialogResult), () =>
            {
                if (DialogWindow != null)
                    DialogWindow.DialogResult = true;
            });
            RegisterCommand(nameof(TDialogResult.Cancel), () => DialogResult = TDialogResult.Cancel);
            RegisterCommand(nameof(TDialogResult.Close), () => DialogResult = TDialogResult.Close);
            RegisterCommand(nameof(TDialogResult.No), () => DialogResult = TDialogResult.No);
            RegisterCommand(nameof(TDialogResult.Ok), () => DialogResult = TDialogResult.Ok);
            RegisterCommand(nameof(TDialogResult.Yes), () => DialogResult = TDialogResult.Yes);
        }

        /// <summary>
        /// Gets or sets the height of the dialog window.
        /// </summary>
        public double Height { get => height; set => SetValue(ref height, value); }

        /// <summary>
        /// Gets or sets the width of the dialog window.
        /// </summary>
        public double Width { get => width; set => SetValue(ref width, value); }

        /// <summary>
        /// Gets or sets the title of the dialog window.
        /// </summary>
        public string Title { get => title; set => SetValue(ref title, value); }

        /// <summary>
        /// Gets or sets the message to display in the dialog.
        /// </summary>
        public string Message { get => message; set => SetValue(ref message, value); }

        /// <summary>
        /// Gets or sets the font weight of the message text.
        /// </summary>
        public FontWeight MessageFontWeight { get => fontWeight; set => SetValue(ref fontWeight, value); }

        /// <summary>
        /// Gets or sets the message font weight as a string value.
        /// This allows external consumers to configure the font weight using a string (e.g., "Bold", "Normal").
        /// Invalid values will default to <c>Normal</c>.
        /// </summary>
        public string MessageFontWeightAsString
        {
            get => MessageFontWeight.ToString();
            set
            {
                try
                {
                    var result = fontWeightConverter.ConvertFromString(string.IsNullOrWhiteSpace(value) ? "Normal" : value);
                    if (result is FontWeight fontWeight)
                        MessageFontWeight = fontWeight;
                    else
                        MessageFontWeight = FontWeights.Normal;
                }
                catch
                {
                    MessageFontWeight = FontWeights.Normal;
                }
            }
        }

        /// <summary>
        /// Gets or sets the font size of the message text.
        /// </summary>
        public int MessageFontSize { get => fontSize; set => SetValue(ref fontSize, value); }

        /// <summary>
        /// Gets a value indicating whether the Yes/No buttons are visible.
        /// </summary>
        public bool IsYesNoVisible { get => isYesNoVisible; private set => SetValue(ref isYesNoVisible, value); }

        /// <summary>
        /// Gets a value indicating whether the Cancel button is visible.
        /// </summary>
        public bool IsCancelVisible { get => isCancelVisible; private set => SetValue(ref isCancelVisible, value); }

        /// <summary>
        /// Gets a value indicating whether the OK button is visible.
        /// </summary>
        public bool IsOkVisible { get => isOKVisible; private set => SetValue(ref isOKVisible, value); }

        /// <summary>
        /// Gets a value indicating whether the Close button is visible.
        /// </summary>
        public bool IsCloseVisible { get => isCloseVisible; private set => SetValue(ref isCloseVisible, value); }

        /// <summary>
        /// Gets a value indicating whether the button bar is visible.
        /// </summary>
        public bool IsButtonBarVisible { get => isButtonBarVisible; private set => SetValue(ref isButtonBarVisible, value); }

        /// <summary>
        /// Gets a value indicating whether the message bar is visible.
        /// </summary>
        public bool IsMessageBarVisible { get => isMessageBarVisible; private set => SetValue(ref isMessageBarVisible, value); }

        /// <summary>
        /// Gets a value indicating whether the message log is visible.
        /// </summary>
        public bool IsMessageLogVisible { get => isMessageLogVisible; private set => SetValue(ref isMessageLogVisible, value); }

        /// <summary>
        /// Gets the image source displayed in the dialog.
        /// </summary>
        public Uri ImageSource { get => imageSource; private set => SetValue(ref imageSource, value); }

        /// <summary>
        /// Gets or sets the collection of log messages to display in the dialog.
        /// </summary>
        public IReadOnlyCollection<ILogMessage> Logs { get => logs; set => SetValue(ref logs, value); }

        /// <summary>
        /// Gets or sets the reference to the dialog's owner.
        /// </summary>
        public IDialogWindow DialogWindow { get => dialogWindow; set => SetValue(ref dialogWindow, value); }

        /// <summary>
        /// Gets or sets the result of the dialog interaction.
        /// </summary>
        public TDialogResult DialogResult { get => dialogResult; protected set => SetValue(ref dialogResult, value); }

        /// <summary>
        /// Gets or sets the dialog result as a string value.
        /// This allows external consumers to configure the result using a string (e.g., "Ok", "Cancel").
        /// Invalid values will default to <c>None</c>.
        /// </summary>
        public string DialogResultAsString { get => DialogResult.ToString(); set => DialogResult = value.ToEnum<TDialogResult>(); }

        /// <summary>
        /// Sets the button visibility based on the specified dialog buttons.
        /// </summary>
        public TDialogButtons DialogButtons
        {
            set
            {
                IsCancelVisible = IsCloseVisible = IsOkVisible = IsYesNoVisible = false;
                switch (value)
                {
                    case TDialogButtons.YesNo:
                        IsYesNoVisible = true;
                        break;
                    case TDialogButtons.YesNoCancel:
                        IsYesNoVisible = IsCancelVisible = true;
                        break;
                    case TDialogButtons.OkCancel:
                        IsOkVisible = IsCancelVisible = true;
                        break;
                    case TDialogButtons.OkClose:
                        IsOkVisible = IsCloseVisible = true;
                        break;
                    case TDialogButtons.Ok:
                        IsOkVisible = true;
                        break;
                    case TDialogButtons.Close:
                        IsCloseVisible = true;
                        break;
                }
                IsButtonBarVisible = IsCancelVisible || IsCloseVisible || IsOkVisible || IsYesNoVisible;
            }
        }

        /// <summary>
        /// Gets or sets the dialog buttons as a string value.
        /// This allows external consumers to configure the button options using a string (e.g., "OkCancel", "YesNo").
        /// Invalid values will default to <c>Ok</c>.
        /// </summary>
        public string DialogButtonsAsString { set => DialogButtons = value.ToEnum<TDialogButtons>(); }

        /// <summary>
        /// Sets the image source based on the specified dialog image type.
        /// </summary>
        public TDialogImage DialogImage
        {
            set
            {
                var path = "Images/";
                switch (value)
                {
                    case TDialogImage.Information:
                        path += "Info.png";
                        break;
                    case TDialogImage.Question:
                        path += "Help.png";
                        break;
                    case TDialogImage.Error:
                        path += "Error.png";
                        break;
                    case TDialogImage.Ok:
                        path += "OK.png";
                        break;
                    case TDialogImage.Alert:
                        path += "Alert.png";
                        break;
                    default:
                        path += "Default.png";
                        break;
                }
                ImageSource = new Uri(path, UriKind.Relative);
            }
        }

        /// <summary>
        /// Gets or sets the dialog image type as a string value.
        /// This allows external consumers to configure the image using a string (e.g., "Information", "Error").
        /// Invalid values will default to <c>Default</c>.
        /// </summary>
        public string DialogImageAsString { set => DialogImage = value.ToEnum<TDialogImage>(); }
    }
}
