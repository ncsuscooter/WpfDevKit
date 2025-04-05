using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace WpfDevKit.Controls.FilterDataGrid
{
    /// <summary>
    /// Represents an item used in filtering operations within a DataGrid, supporting hierarchical structure and checked state.
    /// </summary>
    [DebuggerStepThrough]
    public class FilterItem : INotifyPropertyChanged
    {
        private bool? isChecked = true;
        private bool isVisible = true;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the child filter items of this item.
        /// </summary>
        public ObservableCollection<FilterItem> Children { get; } = new ObservableCollection<FilterItem>();

        /// <summary>
        /// Gets or sets the parent filter item of this item.
        /// </summary>
        public FilterItem Parent { get; set; }

        /// <summary>
        /// Gets or sets the content associated with this filter item.
        /// </summary>
        public object Content { get; set; }

        /// <summary>
        /// Gets or sets the hierarchical level of this filter item.
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Gets the full hierarchical path of this filter item.
        /// </summary>
        public string FullPath => $"{Parent?.FullPath}\\{Content}";

        /// <summary>
        /// Gets or sets a value indicating whether the filter item is checked.
        /// Setting this value also updates child and parent items accordingly.
        /// </summary>
        public bool? IsChecked
        {
            get => isChecked;
            set => SetIsChecked(value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the filter item is visible in the UI.
        /// </summary>
        public bool IsVisible
        {
            get => isVisible;
            set
            {
                if (isVisible == value)
                    return;
                isVisible = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVisible)));
            }
        }

        /// <summary>
        /// Sets the checked state of the filter item and optionally updates parent and child items.
        /// </summary>
        /// <param name="value">The new checked state.</param>
        /// <param name="isUpdateParent">Indicates whether the parent item's checked state should be updated.</param>
        private void SetIsChecked(bool? value, bool isUpdateParent = true)
        {
            if (IsChecked == value)
                return;
            isChecked = value;
            if (IsChecked != null)
                foreach (var item in Children)
                    item.SetIsChecked(value, false);
            if (isUpdateParent && Parent != null)
                Parent.SetIsChecked(Parent.Children.All(x => x.IsChecked == IsChecked) ? IsChecked : null);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked)));
        }
    }
}
