using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfDevKit.UI.Core;

namespace WpfDevKit.UI.Behaviors
{
    public static class DataGridBehaviors
    {
        #region IsShowRowNumber

        public static readonly DependencyProperty IsShowRowNumberProperty =
            DependencyProperty.RegisterAttached("IsShowRowNumber", typeof(bool), typeof(DataGridBehaviors), new PropertyMetadata(IsShowRowNumberPropertyChanged));
        public static bool GetIsShowRowNumber(DataGrid d) => (bool)d.GetValue(IsShowRowNumberProperty);
        public static void SetIsShowRowNumber(DataGrid d, bool value) => d.SetValue(IsShowRowNumberProperty, value);
        private static void IsShowRowNumberPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is DataGrid dg))
                return;
            dg.HeadersVisibility = (bool)e.NewValue ? DataGridHeadersVisibility.All : DataGridHeadersVisibility.Column;
            void OnLoadingRow(object sender, DataGridRowEventArgs args) => args.Row.Header = args.Row.GetIndex() + 1;
            if ((bool)e.NewValue)
                WeakEventManager<DataGrid, DataGridRowEventArgs>.AddHandler(dg, nameof(DataGrid.LoadingRow), OnLoadingRow);
            else
                WeakEventManager<DataGrid, DataGridRowEventArgs>.RemoveHandler(dg, nameof(DataGrid.LoadingRow), OnLoadingRow);
        }

        #endregion

        #region IsMultiSelect

        public static readonly DependencyProperty IsMultiSelectProperty =
            DependencyProperty.RegisterAttached("IsMultiSelect", typeof(bool), typeof(DataGridBehaviors), new PropertyMetadata(IsMultiSelectPropertyChanged));
        public static bool GetIsMultiSelect(DataGrid d) => (bool)d.GetValue(IsMultiSelectProperty);
        public static void SetIsMultiSelect(DataGrid d, bool value) => d.SetValue(IsMultiSelectProperty, value);
        private static void IsMultiSelectPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is DataGrid dg))
                return;
            void OnMouseEnter(object sender, MouseEventArgs args)
            {
                if (args.LeftButton == MouseButtonState.Pressed && args.OriginalSource is DataGridRow row)
                {
                    row.IsSelected = !row.IsSelected;
                    args.Handled = true;
                }
            }
            void OnPreviewMouseDown(object sender, MouseButtonEventArgs args)
            {
                if (args.LeftButton == MouseButtonState.Pressed && (args.OriginalSource is FrameworkElement element) && (element.GetParent<DataGridRow>() is DataGridRow row))
                {
                    row.IsSelected = !row.IsSelected;
                    args.Handled = true;
                }
            }
            if ((bool)e.NewValue)
            {
                WeakEventManager<DataGrid, MouseEventArgs>.AddHandler(dg, nameof(DataGrid.MouseEnter), OnMouseEnter);
                WeakEventManager<DataGrid, MouseButtonEventArgs>.AddHandler(dg, nameof(DataGrid.PreviewMouseDown), OnPreviewMouseDown);
            }
            else
            {
                WeakEventManager<DataGrid, MouseEventArgs>.RemoveHandler(dg, nameof(DataGrid.MouseEnter), OnMouseEnter);
                WeakEventManager<DataGrid, MouseButtonEventArgs>.RemoveHandler(dg, nameof(DataGrid.PreviewMouseDown), OnPreviewMouseDown);
            }
        }

        #endregion

        #region IsBlockNavigationWhenDirty

        public static readonly DependencyProperty IsBlockNavigationWhenDirtyProperty =
            DependencyProperty.RegisterAttached("IsBlockNavigationWhenDirty", typeof(bool), typeof(DataGridBehaviors), new PropertyMetadata(IsBlockNavigationWhenDirtyPropertyChanged));
        public static bool GetIsBlockNavigationWhenDirty(DataGrid d) => (bool)d.GetValue(IsBlockNavigationWhenDirtyProperty);
        public static void SetIsBlockNavigationWhenDirty(DataGrid d, bool value) => d.SetValue(IsBlockNavigationWhenDirtyProperty, value);
        private static void IsBlockNavigationWhenDirtyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is DataGrid dg))
                return;
            void OnPreviewKeyDown(object sender, KeyEventArgs args) => _ = BlockNavigationWhenDirtyAsync(sender, args);
            async Task BlockNavigationWhenDirtyAsync(object sender, KeyEventArgs args)
            {
                // Handle inline editing keys that shouldn't leave current row
                if (args.Key == Key.Tab || args.Key == Key.Return || args.Key == Key.Enter || args.Key == Key.Left || args.Key == Key.Right)
                {
                    args.Handled = true;
                    return;
                }

                // Handle navigation keys that could leave the row
                if (args.Key == Key.Up || args.Key == Key.Down || args.Key == Key.PageUp || args.Key == Key.PageDown ||
                    ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                     (args.Key == Key.Home || args.Key == Key.End)))
                {
                    if (dg.DataContext is IConfirmNavigation nav)
                    {
                        var canLeave = await nav.CanNavigateAwayAsync(); // async call to dialog, etc.
                        if (!canLeave)
                            args.Handled = true; // prevent movement
                    }
                }
            }
            if ((bool)e.NewValue)
                WeakEventManager<DataGrid, KeyEventArgs>.AddHandler(dg, nameof(DataGrid.PreviewKeyDown), OnPreviewKeyDown);
            else
                WeakEventManager<DataGrid, KeyEventArgs>.RemoveHandler(dg, nameof(DataGrid.PreviewKeyDown), OnPreviewKeyDown);
        }

        #endregion

        #region IsOpenDialog

        public static readonly DependencyProperty IsOpenDialogProperty =
            DependencyProperty.RegisterAttached("IsOpenDialog", typeof(bool), typeof(DataGridBehaviors), new PropertyMetadata(IsOpenDialogPropertyChanged));
        public static bool GetIsOpenDialog(DataGrid d) => (bool)d.GetValue(IsOpenDialogProperty);
        public static void SetIsOpenDialog(DataGrid d, bool value) => d.SetValue(IsOpenDialogProperty, value);
        private static void IsOpenDialogPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is DataGrid dg))
                return;
            void OnMouseDoubleClick(object sender, MouseButtonEventArgs args)
            {
                if (dg.SelectedItem is null)
                    return;
                if (dg.DataContext is IOpenDialogHandler handler)
                    handler.OnOpenDialogRequested(dg.SelectedItem);
            }
            if ((bool)e.NewValue)
                WeakEventManager<DataGrid, MouseButtonEventArgs>.AddHandler(dg, nameof(DataGrid.MouseDoubleClick), OnMouseDoubleClick);
            else
                WeakEventManager<DataGrid, MouseButtonEventArgs>.RemoveHandler(dg, nameof(DataGrid.MouseDoubleClick), OnMouseDoubleClick);
        }

        #endregion
    }
}
