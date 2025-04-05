namespace WpfDevKit.UI.Dialogs
{
    /// <summary>
    /// Represents a dialog window that can be displayed as a modal dialog.
    /// </summary>
    public interface IDialogWindow
    {
        /// <summary>
        /// Gets or sets the data context for the dialog window.
        /// </summary>
        object DataContext { get; set; }

        /// <summary>
        /// Gets or sets the result of the dialog after it is closed.
        /// </summary>
        bool? DialogResult { get; set; }

        /// <summary>
        /// Displays the dialog window as a modal dialog and returns the result.
        /// </summary>
        /// <returns>
        /// A nullable boolean value indicating the result of the dialog:
        /// <list type="bullet">
        /// <item><description><c>true</c> if the dialog was accepted (e.g., OK or Yes button was clicked).</description></item>
        /// <item><description><c>false</c> if the dialog was canceled or closed.</description></item>
        /// <item><description><c>null</c> if no result is set.</description></item>
        /// </list>
        /// </returns>
        bool? ShowDialog();
    }
}
