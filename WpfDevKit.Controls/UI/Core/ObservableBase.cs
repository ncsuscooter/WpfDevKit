using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace WpfDevKit.UI.Core
{
    /// <summary>
    /// Provides an abstract base class for observable objects, implementing IObservable.
    /// </summary>
    [DebuggerStepThrough]
    public abstract class ObservableBase : IObservable
    {
        /// <inheritdoc/>
        public event PropertyChangingEventHandler PropertyChanging;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc/>
        public virtual void OnPropertyChanging([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrWhiteSpace(propertyName))
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }

        /// <inheritdoc/>
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrWhiteSpace(propertyName))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Notifies that all read-only properties of the object have changed.
        /// </summary>
        public virtual void NotifyReadOnlyPropertiesChanged()
        {
            foreach (var item in GetType().GetProperties().Where(x => x.CanRead && !x.CanWrite))
                OnPropertyChanged(item.Name);
        }

        /// <summary>
        /// Sets the value of a field and notifies property change events.
        /// </summary>
        /// <typeparam name="T">The type of the field.</typeparam>
        /// <param name="field">The field to be updated.</param>
        /// <param name="newValue">The new value to be assigned.</param>
        /// <param name="notifyCollection">An optional array of property names to notify.</param>
        /// <param name="propertyName">The name of the property that is changing. Automatically captured by default.</param>
        protected void SetValue<T>(ref T field, T newValue, string[] notifyCollection = null, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return;

            OnPropertyChanging(propertyName);
            field = newValue;
            OnPropertyChanged(propertyName);

            if (notifyCollection != null)
                foreach (var item in notifyCollection)
                    OnPropertyChanged(item);
        }

        /// <summary>
        /// Sets the value of a field or property using an expression and notifies property change events.
        /// </summary>
        /// <typeparam name="TObject">The type of the object containing the property.</typeparam>
        /// <typeparam name="TValue">The type of the value being set.</typeparam>
        /// <param name="obj">The instance of the object whose property is being set.</param>
        /// <param name="value">The new value to be assigned.</param>
        /// <param name="selector">An expression representing the property to set.</param>
        /// <param name="notifyCollection">An optional array of property names to notify.</param>
        /// <param name="propertyName">The name of the property that is changing. Automatically captured by default.</param>
        protected void SetValue<TObject, TValue>(TObject obj, TValue value, Expression<Func<TObject, TValue>> selector, string[] notifyCollection = null, [CallerMemberName] string propertyName = null)
        {
            if (selector.Body is MemberExpression body)
            {
                TValue GetValue()
                {
                    switch (body.Member)
                    {
                        case FieldInfo field:
                            return (TValue)field.GetValue(obj);
                        case PropertyInfo property:
                            return (TValue)property.GetValue(obj);
                        default:
                            return default;
                    }
                }

                void SetValue()
                {
                    switch (body.Member)
                    {
                        case FieldInfo field:
                            field.SetValue(obj, value);
                            break;
                        case PropertyInfo property:
                            property.SetValue(obj, value);
                            break;
                    }
                }

                if (EqualityComparer<TValue>.Default.Equals(GetValue(), value))
                    return;

                OnPropertyChanging(propertyName);
                SetValue();
                OnPropertyChanged(propertyName);

                if (notifyCollection != null)
                    foreach (var item in notifyCollection)
                        OnPropertyChanged(item);
            }
        }
    }
}
