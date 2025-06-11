using System;
using System.ComponentModel;
using System.Diagnostics;

namespace WpfDevKit.UI.CollectionView
{
    [DebuggerStepThrough]
    internal class CollectionViewService : ICollectionViewService
    {
        private ICollectionView collectionView;
        
        /// <inheritdoc/>
        public Predicate<object> Filter
        {
            get => collectionView?.Filter;
            set
            {
                if (collectionView != null)
                    collectionView.Filter = value;
            }
        }

        /// <inheritdoc/>
        public IDisposable DeferRefresh() => collectionView?.DeferRefresh();

        /// <inheritdoc/>
        public void Refresh() => collectionView?.Refresh();

        /// <inheritdoc/>
        public void Bind(ICollectionView collectionView) => this.collectionView = collectionView ?? throw new ArgumentNullException(nameof(collectionView));
    }
}
   