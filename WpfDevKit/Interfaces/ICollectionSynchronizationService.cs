using System.Collections;

namespace WpfDevKit.Interfaces
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
        /// <param name="objectLock">The lock object used for synchronization.</param>
        void EnableCollectionSynchronization(IEnumerable collection, object objectLock);

        /// <summary>
        /// Disables synchronization for the specified collection.
        /// After calling this method, the collection will no longer be synchronized and access will not be thread-safe.
        /// </summary>
        /// <param name="collection">The collection to disable synchronization for.</param>
        void DisableCollectionSynchronization(IEnumerable collection);
    }
}
