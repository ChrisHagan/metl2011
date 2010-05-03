using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SandRibbon.Providers
{
    public class ProviderMonitor
    {
        public static void CouchDown()
        {
            MessageBox.Show("No connection could be made to the structure server.  This means that no conversations can be loaded.  MeTL is not useful in this state.");
        }
    }
}
