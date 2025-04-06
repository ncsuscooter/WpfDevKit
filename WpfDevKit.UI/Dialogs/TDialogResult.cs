namespace WpfDevKit.UI.Dialogs
{
    /// <summary>
    /// Represents the possible results of a dialog interaction.
    /// </summary>
    public enum TDialogResult
    {
        /// <summary>
        /// No result was selected.
        /// </summary>
        None,

        /// <summary>
        /// The user selected "Yes".
        /// </summary>
        Yes,

        /// <summary>
        /// The user selected "No".
        /// </summary>
        No,

        /// <summary>
        /// The user selected "OK".
        /// </summary>
        Ok,

        /// <summary>
        /// The user selected "Cancel".
        /// </summary>
        Cancel,

        /// <summary>
        /// The user selected "Close".
        /// </summary>
        Close
    }
}
