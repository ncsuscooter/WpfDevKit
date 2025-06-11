using System;
using System.ComponentModel;

namespace WpfDevKit.UI.CollectionView
{
    public interface ICollectionViewService
    {
        /// <summary>
        /// Gets or sets a callback used to determine if an item is suitable for inclusion in the view.
        /// </summary>
        Predicate<object> Filter { get; set; }

        /// <summary>
        /// Enters a defer cycle that you can use to merge changes to the view and delay automatic refresh.
        /// </summary>
        /// <returns>An <see cref="IDisposable"/> object that you can use to dispose of the calling object.</returns>
        IDisposable DeferRefresh();

        /// <summary>
        /// Refreshes the currently bound view.
        /// </summary>
        void Refresh();

        /// <summary>
        /// Binds the given <see cref="ICollectionView"/> to the service.
        /// </summary>
        /// <param name="view">The collection view to control.</param>
        void Bind(ICollectionView view);
    }
}
   