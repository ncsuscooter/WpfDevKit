using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace WpfDevKit.Controls.FilterDataGrid
{
    /// <summary>
    /// Provides attached properties and behavior for enabling filtering functionality in a <see cref="DataGrid"/>.
    /// </summary>
    [DebuggerStepThrough]
    public static class FilterBehavior
    {
        #region FilterPopups

        /// <summary>
        /// Identifies the <c>FilterPopups</c> attached dependency property.
        /// Used to store filter popup instances associated with a DataGrid.
        /// </summary>
        public static readonly DependencyProperty FilterPopupsProperty =
            DependencyProperty.RegisterAttached("FilterPopups", typeof(Dictionary<string, FilterPopup>), typeof(FilterBehavior));

        /// <summary>
        /// Gets the dictionary of filter popups associated with the specified <see cref="DataGrid"/>.
        /// </summary>
        /// <param name="d">The DataGrid instance.</param>
        /// <returns>The dictionary of filter popups.</returns>
        public static Dictionary<string, FilterPopup> GetFilterPopups(DataGrid d) => d.GetValue(FilterPopupsProperty) as Dictionary<string, FilterPopup>;

        /// <summary>
        /// Sets the dictionary of filter popups associated with the specified <see cref="DataGrid"/>.
        /// </summary>
        /// <param name="d">The DataGrid instance.</param>
        /// <param name="value">The dictionary of filter popups to associate.</param>
        public static void SetFilterPopups(DataGrid d, Dictionary<string, FilterPopup> value) => d.SetValue(FilterPopupsProperty, value);

        #endregion

        #region IsIntialized

        /// <summary>
        /// Identifies the <c>IsIntialized</c> attached dependency property.
        /// Indicates whether filtering behavior has been initialized on a DataGrid.
        /// </summary>
        public static readonly DependencyProperty IsIntializedProperty =
            DependencyProperty.RegisterAttached("IsIntialized", typeof(bool), typeof(FilterBehavior), new PropertyMetadata(false));

        /// <summary>
        /// Gets a value indicating whether the filtering behavior is initialized on the specified <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="d">The target dependency object.</param>
        /// <returns><c>true</c> if initialized; otherwise, <c>false</c>.</returns>
        public static bool GetIsIntialized(DependencyObject d) => (bool)d.GetValue(IsIntializedProperty);

        /// <summary>
        /// Sets a value indicating whether the filtering behavior is initialized on the specified <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="d">The target dependency object.</param>
        /// <param name="value"><c>true</c> to mark as initialized; otherwise, <c>false</c>.</param>
        public static void SetIsIntialized(DependencyObject d, bool value) => d.SetValue(IsIntializedProperty, value);

        #endregion

        #region IsFiltered

        /// <summary>
        /// Identifies the <c>IsFiltered</c> attached dependency property.
        /// Indicates whether filtering is currently applied on a DataGrid.
        /// </summary>
        public static readonly DependencyProperty IsFilteredProperty =
            DependencyProperty.RegisterAttached("IsFiltered", typeof(bool), typeof(FilterBehavior), new PropertyMetadata(false));

        /// <summary>
        /// Gets a value indicating whether filtering is currently applied on the specified <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="d">The target dependency object.</param>
        /// <returns><c>true</c> if filtering is applied; otherwise, <c>false</c>.</returns>
        public static bool GetIsFiltered(DependencyObject d) => (bool)d.GetValue(IsFilteredProperty);

        /// <summary>
        /// Sets a value indicating whether filtering is currently applied on the specified <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="d">The target dependency object.</param>
        /// <param name="value"><c>true</c> to mark as filtered; otherwise, <c>false</c>.</param>
        public static void SetIsFiltered(DependencyObject d, bool value) => d.SetValue(IsFilteredProperty, value);

        #endregion

        #region IsEnabled

        /// <summary>
        /// Identifies the <c>IsEnabled</c> attached dependency property.
        /// Indicates whether filtering is enabled for a specific <see cref="DataGridTextColumn"/>.
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(FilterBehavior), new PropertyMetadata(false, IsEnabledPropertyChanged));

        /// <summary>
        /// Gets a value indicating whether filtering is enabled for the specified <see cref="DataGridTextColumn"/>.
        /// </summary>
        /// <param name="column">The target DataGridTextColumn.</param>
        /// <returns><c>true</c> if filtering is enabled; otherwise, <c>false</c>.</returns>
        public static bool GetIsEnabled(DataGridTextColumn column) => (bool)column.GetValue(IsEnabledProperty);

        /// <summary>
        /// Sets a value indicating whether filtering is enabled for the specified <see cref="DataGridTextColumn"/>.
        /// </summary>
        /// <param name="column">The target DataGridTextColumn.</param>
        /// <param name="value"><c>true</c> to enable filtering; otherwise, <c>false</c>.</param>
        public static void SetIsEnabled(DataGridTextColumn column, bool value) => column.SetValue(IsEnabledProperty, value);

        /// <summary>
        /// Handles changes to the <c>IsEnabled</c> attached property.
        /// Applies a filter header style if filtering is enabled.
        /// </summary>
        /// <param name="d">The target dependency object.</param>
        /// <param name="e">The event data.</param>
        private static void IsEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGridTextColumn column && GetIsEnabled(column) && column.HeaderTemplate == null)
            {
                var resourceUri = new Uri("pack://application:,,,/WpfDevKit.Controls;component/FilterDataGrid/FilterTheme.xaml");
                var resourceDict = new ResourceDictionary { Source = resourceUri };
                column.HeaderStyle = resourceDict["FilterDataGridColumnHeader"] as Style;
            }
        }

        #endregion
    }
}
