using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfDevKit.UI.Behaviors
{
    /// <summary>
    /// Provides attached behavior properties for <see cref="ItemsControl"/> to enable additional UI functionality.
    /// </summary>
    public static class ItemsControlBehaviors
    {
        #region IsAutoScroll

        /// <summary>
        /// Identifies the IsAutoScroll attached dependency property.
        /// When enabled, automatically scrolls the <see cref="ItemsControl"/> to the last item when new items are added.
        /// </summary>
        public static readonly DependencyProperty IsAutoScrollProperty =
            DependencyProperty.RegisterAttached(
                "IsAutoScroll",
                typeof(bool),
                typeof(ItemsControlBehaviors),
                new PropertyMetadata(IsAutoScrollPropertyChanged)
            );

        /// <summary>
        /// Gets the value of the IsAutoScroll attached property.
        /// </summary>
        /// <param name="d">The target <see cref="ItemsControl"/>.</param>
        /// <returns><c>true</c> if auto-scrolling is enabled; otherwise, <c>false</c>.</returns>
        public static bool GetIsAutoScroll(ItemsControl d) => (bool)d.GetValue(IsAutoScrollProperty);

        /// <summary>
        /// Sets the value of the IsAutoScroll attached property.
        /// </summary>
        /// <param name="d">The target <see cref="ItemsControl"/>.</param>
        /// <param name="value"><c>true</c> to enable auto-scrolling; otherwise, <c>false</c>.</param>
        public static void SetIsAutoScroll(ItemsControl d, bool value) => d.SetValue(IsAutoScrollProperty, value);

        /// <summary>
        /// Handles the property changed event for the IsAutoScroll attached property.
        /// </summary>
        /// <param name="d">The dependency object on which the property changed.</param>
        /// <param name="e">The event data.</param>
        private static void IsAutoScrollPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ItemsControl itemsControl))
                return;

            void OnCollectionChanged(object sender, EventArgs args)
            {
                itemsControl.Dispatcher.Invoke(() =>
                {
                    if (itemsControl.Items.Count == 0)
                        return;
                    if (!(VisualTreeHelper.GetChild(itemsControl, 0) is Decorator border))
                        return;
                    if (!(border.Child is ScrollViewer scroll))
                        return;
                    if (scroll.ComputedVerticalScrollBarVisibility != Visibility.Visible)
                        return;
                    if (scroll.ExtentHeight - (scroll.ViewportHeight + scroll.VerticalOffset) < 10)
                        scroll.ScrollToEnd();
                });
            }

            void OnItemsSourcePropertyChanged(object sender, EventArgs args)
            {
                if (notifyCollection != null)
                    notifyCollection.CollectionChanged -= OnCollectionChanged;
                if (itemsControl.ItemsSource is INotifyCollectionChanged collection)
                {
                    notifyCollection = collection;
                    notifyCollection.CollectionChanged += OnCollectionChanged;
                }
            }

            void OnUnloaded(object sender, EventArgs args)
            {
                itemsControl.Unloaded -= OnUnloaded;
                if (notifyCollection != null)
                    notifyCollection.CollectionChanged -= OnCollectionChanged;
                notifyCollection = null;
                DependencyPropertyDescriptor
                    .FromProperty(ItemsControl.ItemsSourceProperty, typeof(ItemsControl))
                    .RemoveValueChanged(itemsControl, OnItemsSourcePropertyChanged);
            }

            if (d is DataGrid dataGrid &&
                (dataGrid.HeadersVisibility == DataGridHeadersVisibility.None ||
                 dataGrid.HeadersVisibility == DataGridHeadersVisibility.Column))
                dataGrid.RowHeaderWidth = 0;

            itemsControl.Unloaded += OnUnloaded;
            DependencyPropertyDescriptor
                .FromProperty(ItemsControl.ItemsSourceProperty, typeof(ItemsControl))
                .AddValueChanged(itemsControl, OnItemsSourcePropertyChanged);
        }

        private static INotifyCollectionChanged notifyCollection;

        #endregion
    }
}
