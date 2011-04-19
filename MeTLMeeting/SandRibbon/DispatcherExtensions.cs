using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Threading;
using System.Windows;

namespace SandRibbon
{
    public static class DispatcherExtensions
    {
        public static void adopt(this Dispatcher dispatcher, Action del) {
            if(dispatcher.CheckAccess())
                del();
            else
                dispatcher.Invoke(del);
        }
        public static void adoptAsync(this Dispatcher dispatcher, Action del)
        {
            if(dispatcher.CheckAccess())
                del();
            else
                dispatcher.BeginInvoke(del);
        }
        public static void queueFocus(this Dispatcher dispatcher, FrameworkElement el) {
            dispatcher.Invoke(DispatcherPriority.Background, (Action)delegate { el.Focus(); });
        }
    }
}
