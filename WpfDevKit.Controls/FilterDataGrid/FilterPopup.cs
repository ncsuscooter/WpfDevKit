using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace WpfDevKit.Controls.FilterDataGrid
{
    /// <summary>
    /// Represents a filtering popup used within a DataGrid column to apply column-level filtering.
    /// Supports text and hierarchical filtering, including date hierarchy filtering.
    /// </summary>
    [DebuggerStepThrough]
    public partial class FilterPopup
    {
        /// <summary>
        /// Identifies the <see cref="FieldName"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FieldNameProperty = DependencyProperty.Register(nameof(FieldName), typeof(string), typeof(FilterPopup));

        /// <summary>
        /// Identifies the <see cref="FieldType"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FieldTypeProperty = DependencyProperty.Register(nameof(FieldType), typeof(Type), typeof(FilterPopup));

        /// <summary>
        /// Gets or sets the name of the field being filtered.
        /// </summary>
        public string FieldName
        {
            get => (string)GetValue(FieldNameProperty);
            set => SetValue(FieldNameProperty, value);
        }

        /// <summary>
        /// Gets or sets the type of the field being filtered.
        /// </summary>
        public Type FieldType
        {
            get => (Type)GetValue(FieldTypeProperty);
            set => SetValue(FieldTypeProperty, value);
        }

        /// <summary>
        /// Gets or sets the binding path used to extract the field value from data items.
        /// </summary>
        private string BindingPath { get; set; }

        /// <summary>
        /// Gets or sets the collection of filters applied to the column. 
        /// The key is the item's full path, and the value is the filtered content.
        /// </summary>
        private Dictionary<string, object> Filters { get; set; } = new Dictionary<string, object>();

        private DataGrid dataGrid;
        private Button button;
        private Popup popup;
        private TextBox textBox;
        private TreeView treeView;
        private FilterItem root;
        private FilterItem blank;

        /// <summary>
        /// Gets the owning <see cref="DataGrid"/> for this filter popup.
        /// </summary>
        private DataGrid DataGridOwner
        {
            get
            {
                if (dataGrid == null)
                    dataGrid = this.TryFindParent<DataGrid>();
                return dataGrid;
            }
        }

        /// <summary>
        /// Gets the header button associated with this filter popup.
        /// </summary>
        private Button HeaderButton
        {
            get
            {
                if (button == null)
                    button = this.TryFindParent<DataGridColumnHeader>()?.FindVisualChild<Button>(nameof(HeaderButton));
                return button;
            }
        }

        /// <summary>
        /// Gets the popup element associated with this filter popup.
        /// </summary>
        private Popup PopupOwner
        {
            get
            {
                if (popup == null)
                    popup = this.TryFindParent<Popup>();
                return popup;
            }
        }

        /// <summary>
        /// Gets the search text box used within the filter popup.
        /// </summary>
        private TextBox SearchTextBox
        {
            get
            {
                if (textBox == null)
                    textBox = this.FindVisualChild<TextBox>(nameof(SearchTextBox));
                return textBox;
            }
        }

        /// <summary>
        /// Gets the tree view displaying filter items.
        /// </summary>
        private TreeView Tree
        {
            get
            {
                if (treeView == null)
                    treeView = this.FindVisualChild<TreeView>();
                return treeView;
            }
        }

        /// <summary>
        /// Gets the root filter item for hierarchical filtering.
        /// </summary>
        private FilterItem Root
        {
            get
            {
                if (root == null && Tree != null && Tree.Items.Count > 0)
                    root = Tree.Items[0] as FilterItem;
                return root;
            }
        }

        /// <summary>
        /// Gets the blank filter item representing null or empty values.
        /// </summary>
        private FilterItem Blank
        {
            get
            {
                if (blank == null && Tree != null && Tree.Items.Count > 1)
                    blank = Tree.Items[1] as FilterItem;
                return blank;
            }
        }

        /// <summary>
        /// Gets the collection view of the DataGrid's items source.
        /// </summary>
        private ICollectionView SourceItemsCollectionView => DataGridOwner?.ItemsSource as ICollectionView ?? CollectionViewSource.GetDefaultView(DataGridOwner?.ItemsSource);

        /// <summary>
        /// Initializes and configures filtering behavior when the popup and related components are loaded.
        /// </summary>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataGridOwner != null && !FilterBehavior.GetIsIntialized(DataGridOwner))
            {
                FilterBehavior.SetFilterPopups(DataGridOwner, new Dictionary<string, FilterPopup>());
                DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(DataGrid)).AddValueChanged(DataGridOwner, OnDataGridItemsSourceChanged);
                WeakEventManager<DataGrid, RoutedEventArgs>.AddHandler(DataGridOwner, nameof(Unloaded), (s, args) =>
                {
                    DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(DataGrid)).RemoveValueChanged(DataGridOwner, OnDataGridItemsSourceChanged);
                    FilterBehavior.GetFilterPopups(DataGridOwner).Clear();
                });
                OnDataGridItemsSourceChanged(sender, e);
                FilterBehavior.SetIsIntialized(DataGridOwner, true);
            }
            if (PopupOwner != null && !FilterBehavior.GetIsIntialized(PopupOwner))
            {
                WeakEventManager<Popup, EventArgs>.AddHandler(PopupOwner, nameof(Popup.Opened), OnPopupOpened);
                WeakEventManager<Popup, EventArgs>.AddHandler(PopupOwner, nameof(Popup.Closed), OnPopupClosed);
                FilterBehavior.SetIsIntialized(PopupOwner, true);
            }
            if (HeaderButton != null && !FilterBehavior.GetIsIntialized(HeaderButton))
            {
                WeakEventManager<Button, RoutedEventArgs>.AddHandler(HeaderButton, nameof(ButtonBase.Click), OnHeaderButtonClick);
                FilterBehavior.SetIsIntialized(HeaderButton, true);
            }
            try
            {
                if (Blank == null || Root == null || DataGridOwner == null || DataGridOwner.Items.Count == 0)
                    return;
                BindingPath = ((this.TryFindParent<DataGridColumnHeader>()?.Column as DataGridBoundColumn)?.Binding as Binding)?.Path?.Path;
                if (string.IsNullOrWhiteSpace(BindingPath))
                    return;
                var parts = BindingPath.Split('.');
                if (parts == null || parts.Length == 0)
                    return;
                var fieldProperty = SourceItemsCollectionView?.SourceCollection?.GetType().GenericTypeArguments.FirstOrDefault()?.GetProperty(parts[0]);
                foreach (var item in parts.Skip(1))
                    fieldProperty = fieldProperty?.PropertyType.GetProperty(item);
                if (fieldProperty == null)
                    return;
                FieldType = Nullable.GetUnderlyingType(fieldProperty.PropertyType) ?? fieldProperty.PropertyType;
                if (FieldType == null)
                    return;
                var filterPopups = FilterBehavior.GetFilterPopups(DataGridOwner);
                if (filterPopups != null && !filterPopups.ContainsKey(FieldName))
                    filterPopups[FieldName] = this;

                Mouse.OverrideCursor = Cursors.Wait;

                IEnumerable<object> collection = default;
                if (FilterBehavior.GetIsFiltered(HeaderButton))
                    collection = SourceItemsCollectionView.SourceCollection.Cast<object>().Select(x => BindingEvaluator.GetValue(x, BindingPath)).ToList();
                else
                    collection = SourceItemsCollectionView.Cast<object>().Select(x => BindingEvaluator.GetValue(x, BindingPath)).ToList();

                VirtualizingPanel.SetIsVirtualizing(Tree, collection.Count() > 1000);
                VirtualizingPanel.SetVirtualizationMode(Tree, collection.Count() > 1000 ? VirtualizationMode.Recycling : VirtualizationMode.Standard);

                Blank.IsVisible = collection.Any(x => x == null || string.IsNullOrWhiteSpace(Convert.ToString(x)));
                Blank.IsChecked = Blank.IsVisible;

                Root.Children.Clear();
                Root.IsChecked = true;

                if (FieldType == typeof(DateTime))
                {
                    var dates = collection.OfType<DateTime>().OrderBy(x => x).
                        Select(x => x.Date).Distinct().GroupBy(x => x.Date.Year).
                        ToDictionary(y => y.Key, y => y.GroupBy(m => m.Date.Month).
                        ToDictionary(m => m.Key, m => m.GroupBy(d => d.Date.Day).ToList()));

                    foreach (var y in dates)
                    {
                        var year = new FilterItem()
                        {
                            Level = 1,
                            Content = y.Key,
                            Parent = Root
                        };
                        Root.Children.Add(year);
                        foreach (var m in y.Value)
                        {
                            var month = new FilterItem()
                            {
                                Level = 2,
                                Content = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m.Key),
                                Parent = year
                            };
                            year.Children.Add(month);
                            foreach (var d in m.Value)
                            {
                                var day = new FilterItem()
                                {
                                    Level = 3,
                                    Content = d.Key,
                                    Parent = month
                                };
                                month.Children.Add(day);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var item in collection.Distinct().OrderBy(x => x))
                    {
                        Root.Children.Add(new FilterItem()
                        {
                            Level = 1,
                            Content = item,
                            Parent = Root
                        });
                    }
                }

                void SetIsChecked(IEnumerable<FilterItem> source)
                {
                    foreach (var item in source)
                    {
                        item.IsChecked = !Filters.ContainsKey(item.FullPath);
                        if (item.Children.Count > 0)
                            SetIsChecked(item.Children);
                    }
                }
                SetIsChecked(Root.Children);
                Root.IsChecked = Root.Children.All(x => x.IsChecked == true) ? true : (bool?)null;
                SearchTextBox.Focus();
            }
            finally
            {
                Tree?.FindVisualChild<ScrollViewer>()?.ScrollToTop();
                Mouse.OverrideCursor = default;
            }
        }

        /// <summary>
        /// Handles the <c>Opened</c> event of the popup, disabling column headers.
        /// </summary>
        private void OnPopupOpened(object sender, EventArgs e)
        {
            var presenter = this.TryFindParent<DataGridColumnHeadersPresenter>();
            if (presenter != null)
                presenter.IsEnabled = false;
            Mouse.OverrideCursor = default;
        }

        /// <summary>
        /// Handles the <c>Closed</c> event of the popup, re-enabling column headers.
        /// </summary>
        private void OnPopupClosed(object sender, EventArgs e)
        {
            var presenter = this.TryFindParent<DataGridColumnHeadersPresenter>();
            if (presenter != null)
                presenter.IsEnabled = true;
        }

        /// <summary>
        /// Handles the click event of the column header button to open the popup.
        /// </summary>
        private void OnHeaderButtonClick(object sender, RoutedEventArgs e) => PopupOwner.IsOpen = (SourceItemsCollectionView?.SourceCollection?.Cast<object>().Any()).GetValueOrDefault();

        /// <summary>
        /// Handles the click event of the cancel button to close the popup without applying filters.
        /// </summary>
        private void OnCancelButtonClick(object sender, RoutedEventArgs e) => PopupOwner.IsOpen = false;

        /// <summary>
        /// Handles the click event of the apply button to apply the selected filters.
        /// </summary>
        private void OnApplyButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Root?.IsChecked == true && Blank?.IsChecked == true && SearchTextBox.Text.Length == 0)
                {
                    Filters.Clear();
                    return;
                }
                void UpdateFilters(IEnumerable<FilterItem> source)
                {
                    foreach (var item in source)
                    {
                        object content = item.Content;
                        if (item.Level == 3 && item.Content != null && item.Parent != null && item.Parent.Content != null && item.Parent.Parent != null && item.Parent.Parent.Content != null)
                        {
                            int d = (int)item.Content;
                            int m = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames.ToList().IndexOf((string)item.Parent.Content) + 1;
                            int y = (int)item.Parent.Parent.Content;
                            content = new DateTime(y, m, d);
                        }
                        if (!item.IsVisible || (item.IsVisible && item.IsChecked != true))
                            Filters[item.FullPath] = content;
                        if (item.IsVisible && item.IsChecked == true && Filters.ContainsKey(item.FullPath))
                            Filters.Remove(item.FullPath);
                        if (item.Children.Count > 0)
                            UpdateFilters(item.Children);
                    }
                }
                UpdateFilters(Root.Children);
                if (Blank.IsVisible && Blank.IsChecked == false)
                    Filters[Blank.FullPath] = null;
                else
                    Filters.Remove(Blank.FullPath);
            }
            finally
            {
                FilterBehavior.SetIsFiltered(HeaderButton, Filters.Count > 0);
                SourceItemsCollectionView?.Refresh();
                PopupOwner.IsOpen = false;
                SearchTextBox.Clear();
                SearchTextBox.Focus();
            }
        }

        /// <summary>
        /// Handles changes to the DataGrid's item source and reapplies filters accordingly.
        /// </summary>
        private void OnDataGridItemsSourceChanged(object sender, EventArgs e)
        {
            bool Filter(FilterPopup popup, object x)
            {
                var value = BindingEvaluator.GetValue(x, popup.BindingPath);
                if (popup.FieldType == typeof(DateTime))
                    return !popup.Filters.ContainsValue(((DateTime?)value)?.Date);
                else
                    return !popup.Filters.ContainsValue(string.IsNullOrWhiteSpace(Convert.ToString(value)) ? null : value);
            }
            var filterPopups = FilterBehavior.GetFilterPopups(DataGridOwner);
            if (filterPopups == null)
                return;
            if (SourceItemsCollectionView?.CanFilter == true && SourceItemsCollectionView?.Filter == null)
                SourceItemsCollectionView.Filter = x => filterPopups.Values.Cast<FilterPopup>().Aggregate(true, (p, f) => p && Filter(f, x));
            foreach (var item in filterPopups.Values)
                item.OnClearFilterButtonClick(new object(), new RoutedEventArgs());
        }

        /// <summary>
        /// Handles the clear filter button click event and resets the filter.
        /// </summary>
        private void OnClearFilterButtonClick(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Clear();
            SearchTextBox.Focus();
            Root.IsChecked = true;
            Blank.IsChecked = true;
            OnApplyButtonClick(sender, e);
        }

        /// <summary>
        /// Handles the clear search button click event and clears the search text box.
        /// </summary>
        private void OnClearSearchButtonClick(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Clear();
            SearchTextBox.Focus();
        }

        /// <summary>
        /// Handles keyboard input in the search text box for Enter and Escape keys.
        /// </summary>
        private void OnSearchPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                OnApplyButtonClick(sender, e);
            else if (e.Key == Key.Escape)
                OnCancelButtonClick(sender, e);
        }

        /// <summary>
        /// Handles changes to the search text box content and updates visible items accordingly.
        /// </summary>
        private void OnSearchTextChanged(object sender, RoutedEventArgs e)
        {
            var searchToggleButton = this.FindVisualChild<ToggleButton>("SearchToggleButton");
            foreach (var item in Root.Children)
            {
                var contentText = Convert.ToString(item.Content);
                var searchLength = (searchToggleButton.IsChecked == true) && (SearchTextBox.Text.Length <= contentText.Length) ? SearchTextBox.Text.Length : contentText.Length;
                item.IsVisible = string.IsNullOrWhiteSpace(SearchTextBox.Text) || contentText.IndexOf(SearchTextBox.Text, 0, searchLength, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            Tree?.FindVisualChild<ScrollViewer>()?.ScrollToTop();
        }

        /// <summary>
        /// Handles dragging of the resize thumb to adjust popup size.
        /// </summary>
        private void OnResizeThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            Width = ActualWidth + e.HorizontalChange > MinWidth ? ActualWidth + e.HorizontalChange : MinWidth;
            Height = ActualHeight + e.VerticalChange > MinHeight ? ActualHeight + e.VerticalChange : MinHeight;
        }

        /// <summary>
        /// Handles the start of the resize thumb drag operation and updates the cursor.
        /// </summary>
        private void OnResizeThumbDragStarted(object sender, DragStartedEventArgs e) => Mouse.OverrideCursor = Cursors.SizeNWSE;

        /// <summary>
        /// Handles the completion of the resize thumb drag operation and resets the cursor.
        /// </summary>
        private void OnResizeThumbDragCompleted(object sender, DragCompletedEventArgs e) => Mouse.OverrideCursor = default;
    }
}
