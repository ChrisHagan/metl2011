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
        public bool customPresenterIsEnabled = true;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            wire = new Wire();
            instance = this;
            this.Application.SlideShowBegin += onSlideShowBegin;
            this.Application.SlideShowEnd += onSlideShowEnd;
            this.Application.SlideSelectionChanged += onSlideChanged;
            this.Application.SlideShowOnNext += onSlideShowOnNext;
            this.Application.SlideShowOnPrevious += onSlideShowOnPrevious;
            this.Application.SlideShowNextBuild += onSlideShowNextBuild;
            this.Application.SlideShowNextClick += onSlideShowNextClick;
            this.Application.SlideShowNextSlide += onSlideShowNextSlide;
        }
        private void onSlideChanged(object sender)
        {
            if (SSSW != null)
                SSSW.OnSlideChanged();
        }
        private void onSlideShowOnNext(object sender)
        {
            if (SSSW != null)
                SSSW.OnSlideChanged();
        }
        private void onSlideShowOnPrevious(object sender)
        {
            if (SSSW != null)
                SSSW.OnSlideChanged();
        }
        private void onSlideShowNextBuild(object sender)
        {
            if (SSSW != null)
                SSSW.OnSlideChanged();
        }
        private void onSlideShowNextClick(object sender, Effect e)
        {
            if (SSSW != null)
                SSSW.OnSlideChanged();
        }
        private void onSlideShowNextSlide(object sender)
        {
            if (SSSW != null)
                SSSW.OnSlideChanged();
        }
        private void onSlideShowBegin(object sender)
        {
            if (customPresenterIsEnabled)
            {
                SSSW = new SimpleSlideShowWindow();
                SSSW.Show();
                SSSW.isExtendedDesktopMode = true;
            }
        }
        private void onSlideShowEnd(object sender)
        {
            if (SSSW != null)
            {
                SSSW.slideshowMembrane.Close();
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
