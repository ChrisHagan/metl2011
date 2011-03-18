using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbonObjects;
using SandRibbon.Providers;
using SandRibbon.Providers.Structure;
using SandRibbon.Components;
using System.Diagnostics;
using Divelements.SandRibbon;

namespace SandRibbon.Chrome
{
    public partial class ApplicationPopup : Divelements.SandRibbon.ApplicationPopup
    {
        public ApplicationPopup()
        {
            InitializeComponent();
            Opened += ApplicationButtonPopup_Opened;
            Closed += ApplicationButtonPopup_Closed;
        }
        private void ApplicationButtonPopup_Closed(object sender, EventArgs e)
        {
            Trace.TraceInformation("ApplicationButtonPopup_Closed");
            Commands.SetTutorialVisibility.ExecuteAsync(Visibility.Collapsed);
        }
        private void ApplicationButtonPopup_Opened(object sender, EventArgs e)
        {
            Trace.TraceInformation("ApplicationButtonPopup_Opened");
            Commands.SetTutorialVisibility.ExecuteAsync(Visibility.Visible);
        }
        #region helpLinks
        private void OpenEULABrowser(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://metl.adm.monash.edu.au/MeTL/docs/tabletSupport/MLS_UserAgreement.html");
        }
        private void OpenTutorialBrowser(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://metl.adm.monash.edu.au/MeTL/docs/tabletSupport/MLS_Tutorials.html");
        }
        private void OpenReportBugBrowser(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://metl.adm.monash.edu.au/MeTL/docs/report_a_bug.html");
        }
        private void OpenAboutMeTLBrowser(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.monash.edu.au/eeducation/myls2010/students/resources/software/metl/");
        }
        #endregion
        private void ApplicationPopup_ShowOptions(object sender, EventArgs e)
        {
            Trace.TraceInformation("UserOptionsDialog_Show");
            new UserOptionsDialog().Show();
        }
    }
}