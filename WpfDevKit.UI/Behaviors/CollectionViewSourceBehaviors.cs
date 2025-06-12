using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
            if (!(d is CollectionViewSource cvs))
                return;
            Attachment.RemoveHandler(cvs);
            if (!(e.NewValue is ICollectionViewService service))
                return;
            void Handler(object sender, EventArgs args) => service.Bind(cvs.View);
            if (cvs.View == null)
                Attachment.AddHandler(cvs, DependencyPropertyDescriptor.FromProperty(CollectionViewSource.ViewProperty, typeof(CollectionViewSource)), Handler);
            else
                Handler(cvs, EventArgs.Empty);
        }
        private class Attachment
        {
            private EventHandler handler;
            private DependencyPropertyDescriptor descriptor;
            private static readonly ConditionalWeakTable<CollectionViewSource, Attachment> attachments =
                new ConditionalWeakTable<CollectionViewSource, Attachment>();
            public static void AddHandler(CollectionViewSource component, DependencyPropertyDescriptor descriptor, EventHandler handler)
            {
                descriptor.AddValueChanged(component, handler);
                attachments.Add(component, new Attachment
                {
                    handler = handler,
                    descriptor = descriptor
                });
            }
            public static void RemoveHandler(CollectionViewSource component)
            {
                if (attachments.TryGetValue(component, out var attachment))
                {
                    attachment.descriptor.RemoveValueChanged(component, attachment.handler);
                    attachments.Remove(component);
                }
            }
        }

        #endregion
    }
}
