using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using WpfDevKit.Logging;

namespace WpfDevKit.UI.Core
{
    /// <summary>
    /// Provides an abstract base class for observable objects, implementing IObservable.
    /// </summary>
    [DebuggerStepThrough]
    public abstract class ObservableBase : IObservable
    {
        private readonly Dictionary<string, List<Action>> propertyChangingActions = new Dictionary<string, List<Action>>();
        private readonly Dictionary<string, List<Action>> propertyChangedActions = new Dictionary<string, List<Action>>();
        private readonly ILogService logService;

        /// <inheritdoc/>
        public event PropertyChangingEventHandler PropertyChanging;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        protected ObservableBase(ILogService logService) => this.logService = logService ?? throw new ArgumentNullException(nameof(logService));

        /// <inheritdoc/>
        public void OnPropertyChanging([CallerMemberName] string propertyName = null)
        {
            if (propertyChangingActions.TryGetValue(propertyName, out var collection) && collection != null && collection.Count > 0)
            {
                logService.LogTrace(null, $"{nameof(propertyName)}='{propertyName}'", GetType());
                foreach (var action in collection)
                    action();
            }
            if (!string.IsNullOrWhiteSpace(propertyName))
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }

        /// <inheritdoc/>
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyChangedActions.TryGetValue(propertyName, out var collection) && collection != null && collection.Count > 0)
            {
                logService.LogTrace(null, $"{nameof(propertyName)}='{propertyName}'", GetType());
                foreach (var action in collection)
                    action();
            }
            if (!string.IsNullOrWhiteSpace(propertyName))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Notifies that all read-only properties of the object have changed.
        /// </summary>
        public void Notify()
        {
            foreach (var item in GetType().GetProperties().Where(x => x.CanRead && !x.CanWrite))
                OnPropertyChanged(item.Name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="action"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected void RegisterPropertyChangingAction(string propertyName, Action action)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentNullException(nameof(propertyName));
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (!propertyChangingActions.TryGetValue(propertyName, out var collection))
                collection = new List<Action>();
            collection.Add(action);
            logService.LogTrace("Property changing registered", $"{nameof(propertyName)}='{propertyName}'", GetType());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="action"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected void RegisterPropertyChangedAction(string propertyName, Action action)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentNullException(nameof(propertyName));
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (!propertyChangedActions.TryGetValue(propertyName, out var collection))
                collection = new List<Action>();
            collection.Add(action);
            logService.LogTrace(" Property changed  registered", $"{nameof(propertyName)}='{propertyName}'", GetType());
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
