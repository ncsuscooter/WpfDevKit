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

    internal class CollectionViewService2 : ICollectionViewService2
    {
        private ICollectionView collectionView;
        public void Bind(ICollectionView collectionView) => this.collectionView = collectionView ?? throw new ArgumentNullException(nameof(collectionView));
        public void Refresh() => collectionView?.Refresh();
    }
}
   