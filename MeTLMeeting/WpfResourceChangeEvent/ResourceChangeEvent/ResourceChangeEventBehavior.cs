using System;
using System.Windows;
using System.Windows.Interactivity;

namespace Itschwabing.Libraries.ResourceChangeEvent
{
    public class ResourceChangeEventBehavior : Behavior<FrameworkElement>
    {
        public FrameworkElement GetAssociatedObject() {
            return AssociatedObject;
        }

        public static readonly DependencyProperty ResourceProperty = DependencyProperty.Register(
            "Resource", typeof(object), typeof(ResourceChangeEventBehavior), new PropertyMetadata(default(object), ResourceChangedCallback));

        public event EventHandler<ResourceChangeEventArgs> ResourceChanged;

        public object Resource
        {
            get { return GetValue(ResourceProperty); }
            set { SetValue(ResourceProperty, value); }
        }

        private static void ResourceChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var resourceChangeNotifier = dependencyObject as ResourceChangeEventBehavior;
            if (resourceChangeNotifier == null)
                return;

            resourceChangeNotifier.OnResourceChanged(new ResourceChangeEventArgs(args.OldValue, args.NewValue));
        }

        private void OnResourceChanged(ResourceChangeEventArgs args)
        {
            EventHandler<ResourceChangeEventArgs> handler = ResourceChanged;
            if (handler != null) handler(this, args);
        }
    }
}
