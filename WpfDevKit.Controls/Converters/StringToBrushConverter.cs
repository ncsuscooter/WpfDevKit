using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace WpfDevKit.Controls.Converters
{
    /// <summary>
    /// Converts a string to a corresponding <see cref="Brush"/> for UI display.
    /// Useful for color-coding in WPF controls based on input.
    /// </summary>
    [DebuggerStepThrough]
    [ValueConversion(typeof(string), typeof(Brush))]
    public class StringToBrushConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets the dictionary that contains mappings from string keys to brushes.
        /// </summary>
        public ResourceDictionary BrushMap { get; set; }

        /// <summary>
        /// Gets or sets the fallback brush used when a key is not found. Defaults to <see cref="Brushes.Black"/>.
        /// </summary>
        public Brush DefaultBrush { get; set; } = Brushes.Black;

        /// <summary>
        /// Converts a string to a <see cref="Brush"/> based on a map of string keys to brushes.
        /// </summary>
        /// <param name="value">The log category string.</param>
        /// <param name="targetType">The type of the binding target property (expected to be <see cref="Brush"/>).</param>
        /// <param name="parameter">An optional converter parameter (not used).</param>
        /// <param name="culture">The culture to use in the converter (not used).</param>
        /// <returns>A <see cref="Brush"/> corresponding to the string key, or the <see cref="DefaultBrush"/> as a fallback.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => 
            value is string category && BrushMap.Contains(category) ? BrushMap[category] : DefaultBrush;

        /// <summary>
        /// Not supported. This converter does not support converting back from <see cref="Brush"/> to a string.
        /// </summary>
        /// <param name="value">The value that is passed to the target (not used).</param>
        /// <param name="targetType">The type to convert to (not used).</param>
        /// <param name="parameter">An optional parameter (not used).</param>
        /// <param name="culture">The culture to use (not used).</param>
        /// <returns>This method always throws <see cref="NotSupportedException"/>.</returns>
        /// <exception cref="NotSupportedException">Thrown unconditionally.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
