using System;
using System.Windows;
using System.Windows.Data;
using WpfDevKit.UI.CollectionView;

namespace WpfDevKit.UI.Behaviors
{
    public static class CollectionViewSourceBehaviors
    {
        #region RegisterService

        public static readonly DependencyProperty RegisterServiceProperty =
            DependencyProperty.RegisterAttached("RegisterService", typeof(ICollectionViewService), typeof(CollectionViewSourceBehaviors), new PropertyMetadata(OnRegisterServiceChanged));
        public static ICollectionViewService GetRegisterService(CollectionViewSource source) => (ICollectionViewService)source.GetValue(RegisterServiceProperty);
        public static void SetRegisterService(CollectionViewSource source, ICollectionViewService value) => source.SetValue(RegisterServiceProperty, value);
        private static void OnRegisterServiceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is CollectionViewSource cvs && e.NewValue is ICollectionViewService service))
                return;
            service.Bind(CollectionViewSource.GetDefaultView(cvs.Source) ?? throw new InvalidOperationException("Couldn’t get default view"));
        }

        #endregion
    }
}
