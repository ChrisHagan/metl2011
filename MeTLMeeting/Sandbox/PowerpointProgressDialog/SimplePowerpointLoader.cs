using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.PowerPoint;
using System.IO;
using System.Xml.Linq;
using Microsoft.Office.Core;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace PowerpointProgressDialog
{
    public class SimplePowerpointLoader
    {
        static MsoTriState FALSE = MsoTriState.msoFalse;
        static MsoTriState TRUE = MsoTriState.msoTrue;
        static int MagnificationRating = 4;
        public delegate void PowerpointFinishedHandler(PowerpointCompleteEventArgs e);
        public event PowerpointFinishedHandler workComplete;
        public delegate void PowerpointProgressHandler(PowerpointProgressEventArgs e);
        public event PowerpointProgressHandler workingOn;
        public delegate void PowerpointIntentionToWorkHandler(PowerpointIntentionEventArgs e);
        public event PowerpointIntentionToWorkHandler goingToWorkOn;
        public void Load()
        {
            var pptThread = new Thread(new ParameterizedThreadStart(delegate{
                var pptApp = new ApplicationClass();
                
                var ppt = pptApp.Presentations.Open(new FileInfo("sample.pptx").FullName, TRUE, FALSE, FALSE);
                try
                {
                    var currentWorkingDirectory = Directory.GetCurrentDirectory() + "\\tmp";
                    if (!Directory.Exists(currentWorkingDirectory))
                        Directory.CreateDirectory(currentWorkingDirectory);
                    var xml = new XElement("presentation");
                    var backgroundWidth = ppt.SlideMaster.Width * MagnificationRating;
                    var backgroundHeight = ppt.SlideMaster.Height * MagnificationRating;
                    var index = 0;
                    goingToWorkOn(new PowerpointIntentionEventArgs{count=ppt.Slides.Count});
                    foreach (Microsoft.Office.Interop.PowerPoint.Slide slide in ppt.Slides)
                    {
                        var slidePath = Directory.GetCurrentDirectory();
                        foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in slide.Shapes)
                            shape.Visible = MsoTriState.msoFalse;
                        var privateShapes = new List<Microsoft.Office.Interop.PowerPoint.Shape>();
                        foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in slide.Shapes)
                        {
                            if (shape.Tags.Count > 0 && shape.Tags.Value(shape.Tags.Count) == "Instructor")
                            {
                                shape.Visible = MsoTriState.msoFalse;
                                privateShapes.Add(shape);
                            }
                            else shape.Visible = MsoTriState.msoTrue;
                        }
                        var slideFilename = string.Format("{0}/{1}.png", slidePath, index);
                        slide.Export(slideFilename, "PNG", (int)backgroundWidth, (int)backgroundHeight);
                        if(workingOn != null)
                            workingOn(new PowerpointProgressEventArgs{
                                index=index,
                                filename=slideFilename});
                        var xSlide = new XElement("slide");
                        xml.Add(xSlide);
                        index++;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("LoadPowerpointAsFlatSlides error: " + ex.Message);
                }
                finally
                {
                    workComplete(new PowerpointCompleteEventArgs());
                    ppt.Close();
                }
            }));
            pptThread.Start();
        }
    }
    public class PowerpointCompleteEventArgs : EventArgs{
    }
    public class PowerpointIntentionEventArgs : EventArgs{
        public int count { get; set; }
    }
    public class PowerpointProgressEventArgs : EventArgs{
        public int index { get; set; }
        public string filename { get; set; }
    }
}