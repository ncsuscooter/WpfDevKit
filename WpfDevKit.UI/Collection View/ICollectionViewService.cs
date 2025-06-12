using System.ComponentModel;

namespace WpfDevKit.UI.CollectionView
{
    public interface ICollectionViewService
    {
        /// <summary>
        /// Gets the instance of the ICollectionView associated with this instance of the service.
        /// </summary>
        ICollectionView View { get; }

        /// <summary>
        /// Binds the given <see cref="ICollectionView"/> to the service.
        /// </summary>
        /// <param name="view">The collection view to control.</param>
        void Bind(ICollectionView view);
    }
}
   