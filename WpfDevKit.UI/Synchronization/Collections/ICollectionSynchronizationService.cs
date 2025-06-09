using System.Collections;

namespace WpfDevKit.UI.Synchronization.Collections
{
    /// <summary>
    /// Provides methods for synchronizing access to collections in a thread-safe manner.
    /// </summary>
    public interface ICollectionSynchronizationService
    {
        /// <summary>
        /// Enables synchronization for the specified collection using the provided lock object.
        /// This ensures that all access to the collection is thread-safe.
        /// </summary>
        /// <param name="collection">The collection to synchronize.</param>
        /// <param name="lockObject">The lock object used for synchronization.</param>
        /// <remarks>
        /// Enables WPF data binding synchronization for the specified collection using the given lock object.
        /// </remarks>
        void EnableCollectionSynchronization(IEnumerable collection, object lockObject);

        /// <summary>
        /// Disables synchronization for the specified collection.
        /// After calling this method, the collection will no longer be synchronized and access will not be thread-safe.
        /// </summary>
        /// <param name="collection">The collection to disable synchronization for.</param>
        void DisableCollectionSynchronization(IEnumerable collection);
    }
}
