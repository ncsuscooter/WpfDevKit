using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace WpfDevKit.Controls.Converters
{
    /// <summary>
    /// A markup extension that converts a string (or binding) to a <see cref="Brush"/> using the application's resource dictionary.
    /// </summary>
    [MarkupExtensionReturnType(typeof(Brush))]
    public class BrushForExtension : MarkupExtension
    {
        /// <summary>
        /// The key or binding used to look up the brush.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// A fallback brush if no resource is found.
        /// </summary>
        public Brush Fallback { get; set; } = Brushes.Black;

        /// <summary>
        /// Optional dictionary to search before falling back to application resources.
        /// </summary>
        public ResourceDictionary BrushMap { get; set; }

        public BrushForExtension() { }

        public BrushForExtension(object value) => Value = value;

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

        private class InlineStringToBrushConverter : IValueConverter
        {
            public Brush Fallback { get; set; }
            public ResourceDictionary BrushMap { get; set; }
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
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
                throw new NotSupportedException();
        }
    }
}
