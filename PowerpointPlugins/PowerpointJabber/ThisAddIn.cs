using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Office = Microsoft.Office.Core;
using System.Windows.Forms;
using Microsoft.Office.Interop.PowerPoint;
using Microsoft.Office.Core;

namespace PowerpointJabber
{
    public partial class ThisAddIn
    {
        public Wire wire;
        public static ThisAddIn instance;
        public SimpleSlideShowWindow SSSW;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            wire = new Wire();
            instance = this;
            this.Application.SlideShowBegin += onSlideShowBegin;
            this.Application.SlideShowEnd += onSlideShowEnd;
        }
        private void onSlideShowBegin(object sender)
        {
            SSSW = new SimpleSlideShowWindow();
            SSSW.Show();
            if (System.Windows.Forms.Screen.AllScreens.Length > 1)
            {
                SSSW.isExtendedDesktopMode = true;
            }
            //FireUpMultipleSlideShows(100, 100, 400, 250);
            //FireUpMultipleSlideShows(500, 400, 400, 250);
        }
        private void FireUpMultipleSlideShows(int left, int top, int width, int height)
        {
            var newWindow = ThisAddIn.instance.Application.ActivePresentation;
            //var saSlides = new int[newWindow.Slides.Count];
            //for (int i = 0; i < newWindow.Slides.Count; i++)
            //    saSlides[i] = newWindow.Slides[i + 1].SlideID;
            //newWindow.SlideShowSettings.NamedSlideShows.Add(name, saSlides);
            //ewWindow.SlideShowSettings.SlideShowName = name;
            //var oldSlideShowWindow = newWindow.SlideShowWindow;
            PowerPoint.SlideShowSettings newSettings = newWindow.SlideShowSettings;
            newSettings.StartingSlide = 1;
            newSettings.EndingSlide = newWindow.Slides.Count;
            var newSlideShowWindow = newSettings.Run();
            newSlideShowWindow.Left = left;
            newSlideShowWindow.Top = top;
            newSlideShowWindow.Height = height;
            newSlideShowWindow.Width = width;
            newSlideShowWindow.Activate();
        }
        private void onSlideShowEnd(object sender)
        {
            if (SSSW != null)
            {
                SSSW.Close();
                SSSW = null;
            }
        }
        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            if (SSSW != null)
            {
                SSSW.Close();
                SSSW = null;
            }
            ThisAddIn.instance = null;
            this.wire.Disconnect();
            this.wire = null;
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
