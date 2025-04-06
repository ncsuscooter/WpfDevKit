using System.Diagnostics;
using System.Windows;
using System.Windows.Data;

namespace WpfDevKit.UI.FilterDataGrid
{
    /// <summary>
    /// Provides functionality to evaluate a binding expression and retrieve the value of a property path from a source object.
    /// </summary>
    [DebuggerStepThrough]
    public static class BindingEvaluator
    {
        /// <summary>
        /// Retrieves the value of the specified property path from the given source object.
        /// </summary>
        /// <param name="obj">The source object from which to retrieve the value.</param>
        /// <param name="path">The property path to evaluate.</param>
        /// <returns>The value of the specified property path, or <c>null</c> if not found.</returns>
        public static object GetValue(object obj, string path)
        {
            BindingOperations.SetBinding(dummy, Dummy.ValueProperty, new Binding(path)
            {
                Mode = BindingMode.OneTime,
                Source = obj
            });
            return dummy.GetValue(Dummy.ValueProperty);
        }
        private static readonly Dummy dummy = new Dummy();
        /// <summary>
        /// Represents a dummy dependency object used internally for binding evaluation.
        /// </summary>
        private class Dummy : DependencyObject
        {
            /// <summary>
            /// Identifies the <see cref="Value"/> dependency property.
            /// </summary>
            public static readonly DependencyProperty ValueProperty =
                DependencyProperty.Register("Value", typeof(object), typeof(Dummy));
        }
    }
}
