using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using WpfDevKit.Controls.Dialogs.Enums;
using WpfDevKit.Controls.Dialogs.Interfaces;
using WpfDevKit.Extensions;
using WpfDevKit.Interfaces;
using WpfDevKit.Logging.Extensions;
using WpfDevKit.Logging.Interfaces;
using WpfDevKit.Mvvm;

namespace WpfDevKit.Controls.Dialogs
{
    /// <summary>
    /// Represents a base dialog window implementation with support for various configurations,
    /// button visibility, and logging.
    /// </summary>
    [DebuggerStepThrough]
    public class DialogBase : CommandBase, IDialogContext
    {
        private static readonly FontWeightConverter fontWeightConverter = new FontWeightConverter();

        private readonly ILogService logService;
        private readonly IBusyService busyService;

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
        private Uri imageSource = new Uri("/Images/Default.png", UriKind.Relative);

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogBase"/> class.
        /// </summary>
        /// <param name="logService">The logging service used for dialog events.</param>
        /// <param name="busyService">The busy service used to indicate background activity.</param>
        public DialogBase(ICommandFactory commandFactory, ILogService logService, IBusyService busyService) : base(commandFactory) =>
            (this.logService, this.busyService) = (logService, busyService);

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
                var path = "UI/Images/";
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

        /// <summary>
        /// Handles property change events and updates visibility of message bar, log display, or dialog result.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        public override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            logService.LogDebug(null, $"{nameof(propertyName)}='{propertyName}'");
            if (propertyName == nameof(Message))
                IsMessageBarVisible = !string.IsNullOrWhiteSpace(Message);
            else if (propertyName == nameof(Logs))
                IsMessageLogVisible = Logs.Count > 0;
            else if (propertyName == nameof(DialogResult) && DialogWindow != null)
                DialogWindow.DialogResult = true;
            base.OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Executes dialog commands asynchronously and sets the dialog result.
        /// </summary>
        /// <param name="commandName">The name of the command to execute.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override async Task DoPerformCommandAsync(string commandName)
        {
            logService.LogDebug(null, $"{nameof(commandName)}='{commandName}'", GetType());
            using (busyService.Busy())
            {
                await Task.Delay(50);
                switch (commandName)
                {
                    case nameof(TDialogResult.Cancel): DialogResult = TDialogResult.Cancel; break;
                    case nameof(TDialogResult.Close): DialogResult = TDialogResult.Close; break;
                    case nameof(TDialogResult.No): DialogResult = TDialogResult.No; break;
                    case nameof(TDialogResult.Ok): DialogResult = TDialogResult.Ok; break;
                    case nameof(TDialogResult.Yes): DialogResult = TDialogResult.Yes; break;
                    default: logService.LogWarning(NO_ACTION_MESSAGE); break;
                }
            }
        }
    }
}
