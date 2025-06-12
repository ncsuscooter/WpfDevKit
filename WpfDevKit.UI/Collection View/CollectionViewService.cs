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
        public void Bind(ICollectionView collectionView) => this.collectionView = collectionView ?? throw new ArgumentNullException(nameof(collectionView));

        /// <inheritdoc/>
        public ICollectionView View => collectionView;
    }
}
   