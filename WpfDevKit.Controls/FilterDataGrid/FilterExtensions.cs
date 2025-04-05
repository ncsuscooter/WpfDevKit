using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace WpfDevKit.Controls.FilterDataGrid
{
    /// <summary>
    /// Provides extension methods for traversing the visual and logical tree in WPF, 
    /// commonly used for filtering and locating elements in a DataGrid.
    /// </summary>
    [DebuggerStepThrough]
    public static class FilterExtensions
    {
        /// <summary>
        /// Attempts to find a parent of the specified type in the logical or visual tree hierarchy.
        /// </summary>
        /// <typeparam name="T">The type of parent to search for.</typeparam>
        /// <param name="d">The starting dependency object.</param>
        /// <returns>The first parent of the specified type, or <c>null</c> if none found.</returns>
        public static T TryFindParent<T>(this DependencyObject d) where T : DependencyObject
        {
            var p = default(DependencyObject);
            if (d is ContentElement contentElement)
            {
                p = ContentOperations.GetParent(contentElement);
                if (p is null)
                    if (contentElement is FrameworkContentElement frameworkContentElement)
                        p = frameworkContentElement.Parent;
            }
            if (d is FrameworkElement frameworkElement)
                p = frameworkElement.Parent;
            if (p is null)
                p = VisualTreeHelper.GetParent(d);
            if (p is null)
                return null;
            if (p is T parent)
                return parent;
            return TryFindParent<T>(p);
        }

        /// <summary>
        /// Searches for a visual child of the specified type and optional name in the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of visual child to search for.</typeparam>
        /// <param name="d">The starting dependency object.</param>
        /// <param name="name">Optional name of the child element to match. If empty, the first matching type is returned.</param>
        /// <returns>The first matching visual child of the specified type and name, or <c>null</c> if none found.</returns>
        public static T FindVisualChild<T>(this DependencyObject d, string name = "") where T : DependencyObject
        {
            var count = VisualTreeHelper.GetChildrenCount(d);
            if (count == 0 && d is Popup popup)
                return popup.Child?.FindVisualChild<T>(name);
            var result = default(T);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(d, i);
                if (child is T t && (name == string.Empty || name == (string)child.GetValue(FrameworkElement.NameProperty)))
                    return t;
                var grandchild = child.FindVisualChild<T>(name);
                if (!(grandchild is null))
                    return grandchild;
            }
            return result;
        }
    }
}
