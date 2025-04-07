using System;
using System.ComponentModel;
using System.Diagnostics;

namespace WpfDevKit.UI.CollectionView
{
    [DebuggerStepThrough]
    internal class CollectionViewService : ICollectionViewService
    {
        private readonly ICollectionView collectionView;
        public CollectionViewService(ICollectionView collectionView) => this.collectionView = collectionView ?? throw new ArgumentNullException(nameof(collectionView));
        public Predicate<object> Filter
        {
            get => collectionView.Filter;
            set => collectionView.Filter = value;
        }
        public IDisposable DeferRefresh() => collectionView.DeferRefresh();
        public void Refresh() => collectionView.Refresh();
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

    public class CollectionViewService2 : ICollectionViewService2
    {
        private ICollectionView view;

        public void Bind(ICollectionView view)
        {
            this.view = view;
        }

        public void Refresh()
        {
            view?.Refresh();
        }
    }
}
   