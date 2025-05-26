using Avalonia.Controls;
using Avalonia;

namespace ORControlPanelNew.ViewModels.Intercom
{
    public class ViewModelProxy : AvaloniaObject
    {
        public static readonly AttachedProperty<object> ProxyProperty =
            AvaloniaProperty.RegisterAttached<ViewModelProxy, Control, object>("Proxy");

        public static void SetProxy(Control element, object value) => element.SetValue(ProxyProperty, value);
        public static object GetProxy(Control element) => element.GetValue(ProxyProperty);
    }
} 