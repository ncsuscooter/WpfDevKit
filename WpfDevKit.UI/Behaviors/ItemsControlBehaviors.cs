using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
        #region GroupStyle

        public static readonly DependencyProperty GroupStyleProperty =
            DependencyProperty.RegisterAttached("GroupStyle", typeof(GroupStyle), typeof(ItemsControlBehaviors), new PropertyMetadata(GroupStylePropertyChanged));
        public static GroupStyle GetGroupStyle(ItemsControl d) => (GroupStyle)d.GetValue(GroupStyleProperty);
        public static void SetGroupStyle(ItemsControl d, GroupStyle value) => d.SetValue(GroupStyleProperty, value);
        private static void GroupStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ItemsControl itemsControl))
                return;
            if (e.OldValue is GroupStyle o)
                itemsControl.GroupStyle.Remove(o);
            if (e.NewValue is GroupStyle n)
                itemsControl.GroupStyle.Add(n);
        }

        #endregion

        #region IsAutoScroll

        private class AutoScrollBehaviorState
        {
            public INotifyCollectionChanged Collection { get; set; }
            public NotifyCollectionChangedEventHandler CollectionHandler { get; set; }
            public EventHandler ItemsSourceChangedHandler { get; set; }
            public RoutedEventHandler UnloadedHandler { get; set; }
        }

        private static readonly ConditionalWeakTable<ItemsControl, AutoScrollBehaviorState> _states = new ConditionalWeakTable<ItemsControl, AutoScrollBehaviorState>();

        public static readonly DependencyProperty IsAutoScrollProperty =
            DependencyProperty.RegisterAttached("IsAutoScroll", typeof(bool), typeof(ItemsControlBehaviors), new PropertyMetadata(false, OnIsAutoScrollChanged));

        public static bool GetIsAutoScroll(ItemsControl d) => (bool)d.GetValue(IsAutoScrollProperty);
        public static void SetIsAutoScroll(ItemsControl d, bool value) => d.SetValue(IsAutoScrollProperty, value);

        private static void OnIsAutoScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ItemsControl itemsControl))
                return;

            if ((bool)e.NewValue)
                AttachAutoScroll(itemsControl);
            else
                DetachAutoScroll(itemsControl);
        }

        private static void AttachAutoScroll(ItemsControl itemsControl)
        {
            var state = new AutoScrollBehaviorState
            {
                CollectionHandler = (_, __) => ScrollToBottomIfNearEnd(itemsControl)
            };

            state.ItemsSourceChangedHandler = (_, __) =>
            {
                if (state.Collection != null)
                    state.Collection.CollectionChanged -= state.CollectionHandler;

                if (itemsControl.ItemsSource is INotifyCollectionChanged collection)
                {
                    state.Collection = collection;
                    collection.CollectionChanged += state.CollectionHandler;
                }
            };

            state.UnloadedHandler = (_, __) => DetachAutoScroll(itemsControl);

            DependencyPropertyDescriptor
                .FromProperty(ItemsControl.ItemsSourceProperty, typeof(ItemsControl))
                .AddValueChanged(itemsControl, state.ItemsSourceChangedHandler);

            itemsControl.Unloaded += state.UnloadedHandler;

            // Trigger immediately in case ItemsSource is already set
            state.ItemsSourceChangedHandler(itemsControl, EventArgs.Empty);

            _states.Add(itemsControl, state);
        }

        private static void DetachAutoScroll(ItemsControl itemsControl)
        {
            if (!_states.TryGetValue(itemsControl, out var state))
                return;

            if (state.Collection != null)
                state.Collection.CollectionChanged -= state.CollectionHandler;

            DependencyPropertyDescriptor
                .FromProperty(ItemsControl.ItemsSourceProperty, typeof(ItemsControl))
                .RemoveValueChanged(itemsControl, state.ItemsSourceChangedHandler);

            itemsControl.Unloaded -= state.UnloadedHandler;

            _states.Remove(itemsControl);
        }

        private static void ScrollToBottomIfNearEnd(ItemsControl itemsControl)
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

                // Only scroll if user is near bottom already
                var distanceFromBottom = scroll.ExtentHeight - (scroll.VerticalOffset + scroll.ViewportHeight);
                if (distanceFromBottom < 10)
                    scroll.ScrollToEnd();
            });
        }

        #endregion
    }
}
