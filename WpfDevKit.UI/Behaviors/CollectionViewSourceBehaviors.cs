using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using WpfDevKit.UI.CollectionView;

namespace WpfDevKit.UI.Behaviors
{
    public static class CollectionViewSourceBehaviors
    {
        #region IsRegisterService

        public static readonly DependencyProperty RegisterWithProperty =
            DependencyProperty.RegisterAttached("RegisterWith", typeof(ICollectionViewService), typeof(CollectionViewSourceBehaviors), new PropertyMetadata(OnRegisterWithChanged));
        public static ICollectionViewService GetRegisterWith(CollectionViewSource source) => (ICollectionViewService)source.GetValue(RegisterWithProperty);
        public static void SetRegisterWith(CollectionViewSource source, ICollectionViewService value) => source.SetValue(RegisterWithProperty, value);
        private static void OnRegisterWithChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CollectionViewSource cvs && e.NewValue is ICollectionViewService2 service)
            {
                // CollectionViewSource.View is populated only after it's loaded
                if (cvs.View != null)
                    service.Bind(cvs.View);
                else
                    DependencyPropertyDescriptor.FromProperty(CollectionViewSource.ViewProperty, typeof(CollectionViewSource)).AddValueChanged(cvs, (_, __) => service.Bind(cvs.View));
            }
        }

        #endregion
    }
}
