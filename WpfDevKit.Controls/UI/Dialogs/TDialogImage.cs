namespace WpfDevKit.UI.Dialogs
{
    /// <summary>
    /// Specifies the type of image to be displayed in a dialog.
    /// </summary>
    public enum TDialogImage
    {
        /// <summary>
        /// No image is displayed in the dialog.
        /// </summary>
        None,

        /// <summary>
        /// Displays an informational image (typically an "i" icon).
        /// </summary>
        Information,

        /// <summary>
        /// Displays a question mark image, indicating a prompt for user decision.
        /// </summary>
        Question,

        /// <summary>
        /// Displays an error image (typically a red "X" icon).
        /// </summary>
        Error,

        /// <summary>
        /// Displays an "OK" image, indicating a successful operation.
        /// </summary>
        Ok,

        /// <summary>
        /// Displays an alert or warning image (typically a yellow exclamation mark).
        /// </summary>
        Alert,

        /// <summary>
        /// Displays the default image, which may be determined by the system or application settings.
        /// </summary>
        Default
    }
}
