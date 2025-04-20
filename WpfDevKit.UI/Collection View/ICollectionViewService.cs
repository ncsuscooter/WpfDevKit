using System;
using System.ComponentModel;

namespace WpfDevKit.UI.CollectionView
{
    public interface ICollectionViewService
    {
        Predicate<object> Filter { get; set; }
        IDisposable DeferRefresh();
        void Refresh();
    }

    public interface ICollectionViewService2
    {
        /// <summary>
        /// Binds the given <see cref="ICollectionView"/> to the service.
        /// </summary>
        /// <param name="view">The collection view to control.</param>
        void Bind(ICollectionView view);

        /// <summary>
        /// Refreshes the currently bound view.
        /// </summary>
        void Refresh();
    }
}
   