using System;

namespace WpfDevKit.UI.CollectionView
{
    public interface ICollectionViewService
    {
        Predicate<object> Filter { get; set; }
        IDisposable DeferRefresh();
        void Refresh();
    }
}
   