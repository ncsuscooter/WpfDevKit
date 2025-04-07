namespace WpfDevKit.UI.Core
{
    public interface IOpenDialogHandler
    {
        /// <summary>
        /// Invoked by a double-click behavior to request opening a dialog for the selected item.
        /// </summary>
        /// <param name="item">The selected item in the DataGrid.</param>
        void OnOpenDialogRequested(object item);
    }
}
