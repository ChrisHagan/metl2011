using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace ContentAndStrokesExperimentMeTL
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        App()
        {
            this.Shutdown();
            System.Diagnostics.Process.Start("metl.exe");
        }
    }
}
