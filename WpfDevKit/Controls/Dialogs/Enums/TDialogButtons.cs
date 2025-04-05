namespace WpfDevKit.Controls.Dialogs.Enums
{
    /// <summary>
    /// Specifies the types of button configurations available for a dialog.
    /// </summary>
    public enum TDialogButtons
    {
        /// <summary>
        /// No buttons are displayed in the dialog.
        /// </summary>
        None,

        /// <summary>
        /// The dialog displays "Yes" and "No" buttons.
        /// </summary>
        YesNo,

        /// <summary>
        /// The dialog displays "Yes," "No," and "Cancel" buttons.
        /// </summary>
        YesNoCancel,

        /// <summary>
        /// The dialog displays "OK" and "Cancel" buttons.
        /// </summary>
        OkCancel,

        /// <summary>
        /// The dialog displays "OK" and "Close" buttons.
        /// </summary>
        OkClose,

        /// <summary>
        /// The dialog displays only an "OK" button.
        /// </summary>
        Ok,

        /// <summary>
        /// The dialog displays only a "Close" button.
        /// </summary>
        Close
    }
}
