using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace WpfDevKit.UI.Converters
{
    /// <summary>
    /// A markup extension that provides a <see cref="Brush"/> by resolving a string key or binding
    /// against a resource dictionary. Falls back to a default brush if no match is found.
    /// </summary>
    [DebuggerStepThrough]
    [MarkupExtensionReturnType(typeof(Brush))]
    public class BrushForExtension : MarkupExtension
    {
        /// <summary>
        /// Gets or sets the key or binding used to look up the brush.
        /// This can be a string representing a resource key, or a <see cref="Binding"/> for dynamic resolution.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets the fallback brush to use when no brush is found in the provided or application resources.
        /// </summary>
        public Brush Fallback { get; set; } = Brushes.Black;

        /// <summary>
        /// Gets or sets an optional <see cref="ResourceDictionary"/> to search for brushes
        /// before falling back to application-level resources.
        /// </summary>
        public ResourceDictionary BrushMap { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrushForExtension"/> class.
        /// </summary>
        public BrushForExtension() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrushForExtension"/> class
        /// with the specified key or binding.
        /// </summary>
        /// <param name="value">The resource key or <see cref="Binding"/> used to resolve the brush.</param>
        public BrushForExtension(object value) => Value = value;

        /// <summary>
        /// Returns the appropriate brush based on the specified key or binding.
        /// If a <see cref="Binding"/> is provided, wraps it with a converter to resolve the brush at runtime.
        /// </summary>
        /// <param name="serviceProvider">The service provider used during XAML processing.</param>
        /// <returns>A resolved <see cref="Brush"/> from resources or the <see cref="Fallback"/> value.</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // Handle bindings
            if (Value is Binding binding)
            {
                return new Binding
                {
                    Path = binding.Path,
                    Source = binding.Source,
                    Converter = new InlineStringToBrushConverter
                    {
                        Fallback = Fallback,
                        BrushMap = BrushMap
                    }
                };
            }

            // Handle static string keys
            if (Value is string key)
            {
                if (BrushMap?.Contains(key) == true && BrushMap[key] is Brush localBrush)
                    return localBrush;

                if (Application.Current.Resources.Contains(key) && Application.Current.Resources[key] is Brush globalBrush)
                    return globalBrush;
            }

            return Fallback;
        }

        /// <summary>
        /// A private value converter used when the extension is initialized with a <see cref="Binding"/>.
        /// Converts string keys to brushes using the same logic as <see cref="ProvideValue"/>.
        /// </summary>
        private class InlineStringToBrushConverter : IValueConverter
        {
            /// <summary>
            /// Gets or sets the fallback brush to use when resolution fails.
            /// </summary>
            public Brush Fallback { get; set; }

            /// <summary>
            /// Gets or sets an optional dictionary for key-to-brush mapping.
            /// </summary>
            public ResourceDictionary BrushMap { get; set; }

            /// <summary>
            /// Attempts to convert a string key into a <see cref="Brush"/> using the provided resources.
            /// </summary>
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is string key)
                {
                    if (BrushMap?.Contains(key) == true && BrushMap[key] is Brush localBrush)
                        return localBrush;
                    if (Application.Current.Resources.Contains(key) && Application.Current.Resources[key] is Brush globalBrush)
                        return globalBrush;
                }
                return Fallback;
            }

            /// <summary>
            /// Not supported. This converter is one-way only.
            /// </summary>
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
                throw new NotSupportedException();
        }
    }
}
