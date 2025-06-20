using System.Collections;
using System.Diagnostics;
using System.Windows.Data;

namespace WpfDevKit.UI.Synchronization.Collections
{
    /// <summary>
    /// Provides services to enable or disable collection synchronization for collections bound to the UI in WPF.
    /// This is useful for ensuring that collections are thread-safe when being modified from multiple threads in WPF applications.
    /// </summary>
    [DebuggerStepThrough]
    internal class CollectionSynchronizationService : ICollectionSynchronizationService
    {
        /// <inheritdoc/>
        /// <remarks>
        /// This method calls <see cref="BindingOperations.EnableCollectionSynchronization(IEnumerable, object)"/>
        /// to enable collection synchronization for the provided collection, ensuring that WPF's data binding engine
        /// can safely interact with the collection when accessed from multiple threads.
        /// </remarks>
        public void EnableCollectionSynchronization(IEnumerable collection, object lockObject) => BindingOperations.EnableCollectionSynchronization(collection, lockObject);

        /// <inheritdoc/>
        /// <remarks>
        /// This method calls <see cref="BindingOperations.DisableCollectionSynchronization(IEnumerable)"/>
        /// to disable collection synchronization, allowing the collection to be accessed freely without thread synchronization.
        /// </remarks>
        public void DisableCollectionSynchronization(IEnumerable collection) => BindingOperations.DisableCollectionSynchronization(collection);
    }
}
