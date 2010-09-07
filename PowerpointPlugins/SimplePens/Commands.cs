using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace PowerpointJabber
{
    static class Commands
    {
        public static RoutedCommand CloseApplication = new RoutedCommand();
        public static RoutedCommand PageUp = new RoutedCommand();
        public static RoutedCommand PageDown = new RoutedCommand();
    }
}
