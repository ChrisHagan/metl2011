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
    class CommandHelper
    {
        public CommandHelper()
        {
            //var ac = System.Windows.Input.ApplicationCommands;
            //var nc = System.Windows.Input.NavigationCommands.PreviousPage;
            //System.Windows.Input.Key.
        }
    }
}
