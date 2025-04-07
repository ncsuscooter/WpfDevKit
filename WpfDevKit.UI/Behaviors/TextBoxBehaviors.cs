using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfDevKit.UI.Behaviors
{
    /// <summary>
    /// Provides attached behavior properties for <see cref="TextBox"/> to configure label-like behavior.
    /// </summary>
    public static class TextBoxBehaviors
    {
        #region IsLabel

        /// <summary>
        /// Identifies the IsLabel attached dependency property.
        /// When enabled, the <see cref="TextBox"/> behaves as a label and disables text selection.
        /// </summary>
        public static readonly DependencyProperty IsLabelProperty =
            DependencyProperty.RegisterAttached(
                "IsLabel",
                typeof(bool),
                typeof(TextBoxBehaviors),
                new PropertyMetadata(IsLabelPropertyChanged)
            );

        /// <summary>
        /// Gets the value of the IsLabel attached property.
        /// </summary>
        /// <param name="d">The target <see cref="TextBox"/>.</param>
        /// <returns><c>true</c> if label behavior is enabled; otherwise, <c>false</c>.</returns>
        public static bool GetIsLabel(TextBox d) => (bool)d.GetValue(IsLabelProperty);

        /// <summary>
        /// Sets the value of the IsLabel attached property.
        /// </summary>
        /// <param name="d">The target <see cref="TextBox"/>.</param>
        /// <param name="value"><c>true</c> to enable label behavior; otherwise, <c>false</c>.</param>
        public static void SetIsLabel(TextBox d, bool value) => d.SetValue(IsLabelProperty, value);

        /// <summary>
        /// Handles the property changed event for the IsLabel attached property.
        /// Applies a label style and prevents text selection.
        /// </summary>
        /// <param name="d">The dependency object on which the property changed.</param>
        /// <param name="e">The event data.</param>
        private static void IsLabelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TextBox textBox))
                return;

            if (!(textBox.TryFindResource("LabelTextBox") is Style style))
                return;

            textBox.Style = style;

            WeakEventManager<TextBox, RoutedEventArgs>.AddHandler(
                textBox,
                nameof(TextBox.SelectionChanged),
                (sender, args) =>
                {
                    if (textBox.SelectionLength > 0)
                        textBox.SelectionLength = 0;
                });
        }

        #endregion

        #region Mask

        public static readonly DependencyProperty MaskProperty =
            DependencyProperty.RegisterAttached("Mask", typeof(string), typeof(TextBoxBehaviors), new PropertyMetadata(MaskPropertyChanged));
        public static string GetMask(TextBox d) => (string)d.GetValue(MaskProperty);
        public static void SetMask(TextBox d, string value) => d.SetValue(MaskProperty, value);
        private static void MaskPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TextBox textBox))
                return;
            // ^-?[0-9]*\.?[0-9]*$
            // ^(2[0-3]|[0-1]?[0-9]):([0-5]?[0-9]):([0-5]?[0-9])$
            var regex = new Regex(e.NewValue as string, RegexOptions.IgnorePatternWhitespace);
            bool IsMatch(string input) => regex.IsMatch(textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength).Insert(textBox.CaretIndex, input));
            void PreviewTextInput(object sender, TextCompositionEventArgs args) => args.Handled = !IsMatch(args.Text);
            void PreviewKeyDown(object sender, KeyEventArgs args)
            {
                if (args.Key == Key.Space)
                    args.Handled = !IsMatch(" ");
            }
            ;
            void Pasting(object sender, DataObjectPastingEventArgs args)
            {
                if (args.DataObject.GetDataPresent(typeof(string)))
                {
                    if (!IsMatch((string)args.DataObject.GetData(typeof(string))))
                        args.CancelCommand();
                }
                else
                {
                    args.CancelCommand();
                }
            }
            ;
            textBox.PreviewTextInput -= PreviewTextInput;
            textBox.PreviewKeyDown -= PreviewKeyDown;
            DataObject.RemovePastingHandler(textBox, Pasting);
            if (e.NewValue == null)
            {
                textBox.ClearValue(MaskProperty);
            }
            else
            {
                textBox.SetValue(MaskProperty, e.NewValue);
                textBox.PreviewTextInput += PreviewTextInput;
                textBox.PreviewKeyDown += PreviewKeyDown;
                DataObject.AddPastingHandler(textBox, Pasting);
            }
        }

        #endregion
    }
}
