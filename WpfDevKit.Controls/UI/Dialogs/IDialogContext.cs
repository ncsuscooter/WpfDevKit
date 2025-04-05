namespace WpfDevKit.UI.Dialogs
{
    /// <summary>
    /// Represents the context for a dialog window, providing properties for title, message, buttons, image, result, and window.
    /// </summary>
    public interface IDialogContext
    {
        /// <summary>
        /// Gets or sets the title of the dialog.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Gets or sets the message displayed in the dialog.
        /// </summary>
        string Message { get; set; }

        /// <summary>
        /// Sets the buttons available in the dialog.
        /// </summary>
        TDialogButtons DialogButtons { set; }

        /// <summary>
        /// Sets the image/icon displayed in the dialog.
        /// </summary>
        TDialogImage DialogImage { set; }

        /// <summary>
        /// Gets the result of the dialog after it is closed.
        /// </summary>
        TDialogResult DialogResult { get; }
        
        /// <summary>
        /// Gets the Window responsible for displaying the dialog
        /// </summary>
        IDialogWindow DialogWindow { get; set; }
    }
}
