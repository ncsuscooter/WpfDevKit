using System.Windows;
using System.Windows.Media;

public static class DependencyObjectExtensions
{
    public static T GetParent<T>(this DependencyObject d) where T : DependencyObject
    {
        while (d != null && !(d is T))
            d = VisualTreeHelper.GetParent(d);
        return d as T;
    }
    public static T FindVisualChild<T>(this DependencyObject d, string name = "") where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
        {
            var child = VisualTreeHelper.GetChild(d, i);
            if (child is T t && (name == string.Empty || name == (string)child.GetValue(FrameworkElement.NameProperty)))
                return t;
            var grandchild = child.FindVisualChild<T>(name);
            if (grandchild != null)
                return grandchild;
        }
        return default;
    }
}
