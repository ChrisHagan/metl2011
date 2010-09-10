using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Interop.PowerPoint;
using Microsoft.Office.Core;
using System.Windows;

namespace PowerpointJabber
{
    public partial class ThisAddIn
    {
        public static ThisAddIn instance;
        public bool customPresenterIsEnabled
        {
            get
            {
                Properties.Settings.Default.Reload();
                return Properties.Settings.Default.SimplePensEnabled;
            }
            set
            {
                Properties.Settings.Default.SimplePensEnabled = value;
                Properties.Settings.Default.Save();
            }
        }
        public string username
        {
            get
            {
                Properties.Settings.Default.Reload();
                return Properties.Settings.Default.username;
            }
            set
            {
                Properties.Settings.Default.username = value;
                Properties.Settings.Default.Save();
            }
        }
        public string password
        {
            get
            {
                Properties.Settings.Default.Reload();
                return Properties.Settings.Default.password;
            }
            set
            {
                Properties.Settings.Default.password = value;
                Properties.Settings.Default.Save();
            }
        }
        public string location
        {
            get
            {
                Properties.Settings.Default.Reload();
                return Properties.Settings.Default.location;
            }
            set
            {
                Properties.Settings.Default.location = value;
                Properties.Settings.Default.Save();
            }
        }

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            instance = this;
            this.Application.SlideShowBegin += onSlideShowBegin;
            this.Application.SlideShowEnd += onSlideShowEnd;
        }
        private void onSlideShowBegin(object sender)
        {
            if (customPresenterIsEnabled)
            {
                MessageBoxResult response = System.Windows.MessageBox.Show("Do you want to upload this to the VLE?","Upload presentation", MessageBoxButton.YesNo);
                string output = "";
                if (response == MessageBoxResult.Yes)
                    output = VLE_integration.UploadPresentationToVLE();
                if (!String.IsNullOrEmpty(output)) MessageBox.Show(output);
            }
        }
        private void onSlideShowEnd(object sender)
        {
        }
        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            ThisAddIn.instance = null;
        }
        protected override Microsoft.Office.Core.IRibbonExtensibility CreateRibbonExtensibilityObject()
        {
            return new Ribbon1();
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        #endregion
    }
}
