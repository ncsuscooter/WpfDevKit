using System.ComponentModel;

namespace WpfDevKit.UI.Core
{
    /// <summary>
    /// Defines an interface for objects that can notify others about property changes.
    /// It extends <see cref="INotifyPropertyChanging"/> and <see cref="INotifyPropertyChanged"/>.
    /// </summary>
    public interface IObservable : INotifyPropertyChanging, INotifyPropertyChanged
    {
        /// <summary>
        /// Notifies that a property is about to change.
        /// </summary>
        /// <param name="propertyName">The name of the property that is changing.</param>
        void OnPropertyChanging(string propertyName);

        /// <summary>
        /// Notifies that a property has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        void OnPropertyChanged(string propertyName);
    }
}
