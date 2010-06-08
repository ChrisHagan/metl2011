using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Threading;

namespace SandRibbon
{
    public static class DispatcherExtensions
    {
        public static void adopt(this Dispatcher dispatcher, Action del) {
            if (Thread.CurrentThread == dispatcher.Thread)
                del();
            else
                dispatcher.Invoke(del);
        }
        public static void adoptAsync(this Dispatcher dispatcher, Action del)
        {
            if (Thread.CurrentThread == dispatcher.Thread)
                del();
            else
                dispatcher.BeginInvoke(del);
        }
    }
}
