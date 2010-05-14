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
    public class SimpleSlideShowViewer : PowerPoint.SlideShowWindow
    {
        public SimpleSlideShowWindow SSSW; 
        public SimpleSlideShowViewer()
        {
            SSSW = new SimpleSlideShowWindow();
            SSSW.Show();
        }
        public void Activate()
        {
        }
        public MsoTriState Active
        {
            get { return MsoTriState.msoTrue; }
        }
        public Microsoft.Office.Interop.PowerPoint.Application Application
        {
            get { return ThisAddIn.instance.Application; }
        }
        public float Height
        {
            get { return ThisAddIn.instance.Application.Height; }
            set { }
        }
        public int HWND
        {
            get { return ThisAddIn.instance.Application.HWND; }
        }
        public MsoTriState IsFullScreen
        {
            get { return MsoTriState.msoTrue; }
        }
        public float Left
        {
            get { return 0; }
            set { }
        }
        public object Parent
        {
            get { return null; }
        }
        public Presentation Presentation
        {
            get { return ThisAddIn.instance.Application.ActivePresentation; }
        }
        public float Top
        {
            get { return 0; }
            set { }
        }
        public SlideShowView View
        {
            get {return ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View;}
        }
        public float Width
        {
            get { return ThisAddIn.instance.Application.Width; }
            set { }
        }
    }
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
            try
            {
                //This is where I'm going to try and set it to Presenter View and set the correct windows.
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Exception: "+Ex.Message.ToString());
            }
            SSSW = new SimpleSlideShowWindow();
            //SSSW.Owner = (this.Application.HWND);
            SSSW.Show();
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
