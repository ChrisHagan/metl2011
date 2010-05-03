using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.PowerPoint;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers.Structure;
using SandRibbon.Utils.Connection;
using SandRibbonInterop;
using SandRibbonInterop.MeTLStanzas;
using SandRibbonObjects;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

namespace SandRibbon.Utils
{
    public class PowerPointLoaderPrintRange : PrintRange
    {
        //hey Chris, how do I "implement" an interface?
        private int start = 0;
        private int end = 0;
        private object parent;
        private Microsoft.Office.Interop.PowerPoint.Application app;
        int Microsoft.Office.Interop.PowerPoint.PrintRange.Start { get { return start; } }
        int Microsoft.Office.Interop.PowerPoint.PrintRange.End { get { return end; } }
        object Microsoft.Office.Interop.PowerPoint.PrintRange.Parent { get { return parent; } }
        Microsoft.Office.Interop.PowerPoint.Application Microsoft.Office.Interop.PowerPoint.PrintRange.Application { get { return app; } }


        public PowerPointLoaderPrintRange(Presentation ppt)
        {
            app = ppt.Application;
            parent = ppt.Parent;
            end = ppt.Slides.Count - 1;
        }

        public void Delete()
        {
            this.parent = null;
            this.app = null;
        }
    }



    public class PowerPointLoader
    {
        static MsoTriState FALSE = MsoTriState.msoFalse;
        static MsoTriState TRUE = MsoTriState.msoTrue;
        static int resource = 1;
        private static string me;
        public JabberWire wire;
        public DelegateCommand<SandRibbon.Utils.Connection.JabberWire.Credentials> setAuthor;
        private string parsedTitle;
        public PowerPointLoader()
        {
            setAuthor = new DelegateCommand<SandRibbon.Utils.Connection.JabberWire.Credentials>(
                author => me = author.name);
            Commands.SetIdentity.RegisterCommand(setAuthor);
            Commands.PostStartPowerPointLoad.RegisterCommand(new DelegateCommand<object>(
                (conversationDetails) => { startPowerpointImport((ConversationDetails)conversationDetails); }
            ));
        }
        private void startPowerpointImport(ConversationDetails details)
        {
            var fileBrowser = new OpenFileDialog
                                             {
                                                 InitialDirectory = "c:\\",
                                                 Filter = "PowerPoint files (*.ppt, *.pptx)|*.ppt; *.pptx|All files (*.*)|*.*",
                                                 FilterIndex = 0,
                                                 RestoreDirectory = true
                                             };

            var dialogResult = fileBrowser.ShowDialog();
            if (dialogResult == DialogResult.OK)
                foreach (var file in fileBrowser.FileNames)
                    if (file.Contains(".ppt"))
                    {
                        try
                        {
                            LoadPowerpoint(file, details);
                            //LoadPowerpointAsVideoSlides(file, details);
                            //LoadPowerpointAsFlatSlides(file, details);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Sorry, MeTL encountered a problem while trying to import your powerpoint and has to close.");
                            throw e;
                        }
                        finally
                        {
                            Commands.PowerPointLoadFinished.Execute(null);
                        }
                        return;
                    }
                    else
                    {
                        MessageBox.Show("Sorry. I do not know what to do with that file format");
                    }
        }
        public void LoadPowerpointAsFlatSlides(string file, ConversationDetails details)
        {
            var ppt = new ApplicationClass().Presentations.Open(file, TRUE, FALSE, FALSE);
            try
            {
                var currentWorkingDirectory = Directory.GetCurrentDirectory() + "\\tmp";
                if (!Directory.Exists(currentWorkingDirectory))
                    Directory.CreateDirectory(currentWorkingDirectory);
                ppt.SaveAs(currentWorkingDirectory + "\\pptImport", PpSaveAsFileType.ppSaveAsPNG, FALSE);
                var provider = ConversationDetailsProviderFactory.Provider;
                var xml = new XElement("presentation");
                xml.Add(new XAttribute("name", details.Title));
                if (details.Tag == null)
                    details.Tag = "unTagged";
                var conversation = provider.Create(details);
                conversation.Author = me;
                Commands.PowerPointProgress.Execute("Starting to parse powerpoint file");
                foreach (Microsoft.Office.Interop.PowerPoint.Slide slide in ppt.Slides)
                {
                    int slideNumber = slide.SlideIndex;
                    var xSlide = new XElement("slide");
                    xSlide.Add(new XAttribute("index", slide.SlideIndex));
                    var slidePath = currentWorkingDirectory + "\\pptImport\\Slide" + slideNumber.ToString() + ".PNG";
                    xSlide.Add(new XElement("shape",
                    new XAttribute("x", 0),
                    new XAttribute("y", 0),
                    new XAttribute("privacy", "public"),
                    new XAttribute("snapshot", slidePath)));
                    xml.Add(xSlide);
                }
                Commands.PowerPointProgress.Execute("Finished parsing powerpoint, Beginning data upload");
                var startingId = conversation.Slides.First().id;
                var index = 0;
                conversation.Slides = xml.Descendants("slide").Select(d => new SandRibbonObjects.Slide { author = me, id = startingId++, index = index++ }).ToList();
                provider.Update(conversation);
                var xmlSlides = xml.Descendants("slide");
                for (int i = 0; i < xmlSlides.Count(); i++)
                {
                    var slideXml = xmlSlides.ElementAt(i);
                    var slideId = conversation.Slides[i].id;
                    var powerpointThread = new Thread(() =>
                    {
                        uploadXmlUrls(slideId, slideXml);
                        sendSlide(slideId, slideXml);
                    });
                    powerpointThread.SetApartmentState(ApartmentState.STA);
                    powerpointThread.Start();
                }
                Commands.JoinConversation.Execute(conversation.Jid);
            }
            catch (Exception ex)
            {
                MessageBox.Show("LoadPowerpointAsFlatSlides error: " + ex.Message);
            }
            finally
            {
                ppt.Close();
                Commands.PowerPointLoadFinished.Execute(null);
            }
        }
/*
        public void LoadPowerpointAsVideoSlides(string file, ConversationDetails details)
        {
            var ppt = new ApplicationClass().Presentations.Open(file, TRUE, FALSE, FALSE);
            try
            {
                var currentWorkingDirectory = Directory.GetCurrentDirectory() + "\\tmp";
                if (!Directory.Exists(currentWorkingDirectory))
                    Directory.CreateDirectory(currentWorkingDirectory);
                ppt.SaveAs(currentWorkingDirectory + "\\pptImport.wmv", PpSaveAsFileType.ppSaveAsWMV, FALSE);
                
                //I'm not sure how to do this type of callback properly yet in Dot.NET.  I rather suspect that this while loop will never be reached.
                while (ppt.CreateVideoStatus == PpMediaTaskStatus.ppMediaTaskStatusInProgress)
                    if (ppt.CreateVideoStatus == PpMediaTaskStatus.ppMediaTaskStatusDone)
                        MessageBox.Show("ExportAsVideo finished");
                Thread.Sleep(100);
                if (ppt.CreateVideoStatus == PpMediaTaskStatus.ppMediaTaskStatusFailed)
                    MessageBox.Show("ExportAsVideo failed");

                var presentationAdvanceTime = ppt.SlideMaster.SlideShowTransition.AdvanceTime;
                foreach (Microsoft.Office.Interop.PowerPoint.Slide slide in ppt.Slides)
                {
                    foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in slide.Shapes)
                    {
                        var UnsortedTimeLine = new Dictionary<int, float>();
                        var AS = shape.AnimationSettings;
                        if (AS.Animate == MsoTriState.msoTrue)
                        {
                            if (AS.AdvanceMode == PpAdvanceMode.ppAdvanceOnClick)
                                UnsortedTimeLine.Add(AS.AnimationOrder, presentationAdvanceTime);
                            else UnsortedTimeLine.Add(AS.AnimationOrder, AS.AdvanceTime);
                        }
                        var sortedTimeLine = from k in UnsortedTimeLine.Keys orderby UnsortedTimeLine[k] ascending select k;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("LoadPowerpointVideoAsSlides error: " + ex.Message);
            }
        }
*/
        public void LoadPowerpoint(string file, ConversationDetails details)
        {
            var ppt = new ApplicationClass().Presentations.Open(file, TRUE, FALSE, FALSE);
            try
            {
                var provider = ConversationDetailsProviderFactory.Provider;
                var xml = new XElement("presentation");
                xml.Add(new XAttribute("name", details.Title));
                if (details.Tag == null)
                    details.Tag = "unTagged";
                var conversation = provider.Create(details);
                conversation.Author = me;
                Commands.PowerPointProgress.Execute("Starting to parse powerpoint file");
                /*ppt.ExportAsFixedFormat(
                    "testExport.xps", 
                    PpFixedFormatType.ppFixedFormatTypeXPS, 
                    PpFixedFormatIntent.ppFixedFormatIntentPrint, 
                    MsoTriState.msoTrue, 
                    PpPrintHandoutOrder.ppPrintHandoutHorizontalFirst, 
                    PpPrintOutputType.ppPrintOutputSlides, 
                    MsoTriState.msoFalse, 
                    new PowerPointLoaderPrintRange(ppt), 
                    PpPrintRangeType.ppPrintAll, 
                    "whatever", 
                    true, 
                    true, 
                    true, 
                    true, 
                    false, 
                    new object());*/
                foreach (var slide in ppt.Slides)
                {
                    importSlide(xml, (Microsoft.Office.Interop.PowerPoint.Slide)slide);
                }
                Commands.PowerPointProgress.Execute("Finished parsing powerpoint, Beginning data upload");
                var startingId = conversation.Slides.First().id;
                var index = 0;
                conversation.Slides = xml.Descendants("slide").Select(d => new SandRibbonObjects.Slide { author = me, id = startingId++, index = index++ }).ToList();
                provider.Update(conversation);
                var xmlSlides = xml.Descendants("slide");
                for (int i = 0; i < xmlSlides.Count(); i++)
                {
                    var slideXml = xmlSlides.ElementAt(i);
                    var slideId = conversation.Slides[i].id;
                    var powerpointThread = new Thread(() =>
                    {
                        uploadXmlUrls(slideId, slideXml);
                        sendSlide(slideId, slideXml);
                    });
                    powerpointThread.SetApartmentState(ApartmentState.STA);
                    powerpointThread.Start();
                }
                Commands.JoinConversation.Execute(conversation.Jid);
            }
            finally
            {/*How do we know when a parallelized operation has actually finished?
              * We don't.  But the content comes in live.*/
                ppt.Close();
                Commands.PowerPointLoadFinished.Execute(null);
            }
        }
        private void sendSlide(int id, XElement slide)
        {
            sendShapes(id, slide.Descendants("shape"));
            //sendPublicTextBoxes(id, slide.Descendants("publicText"));
            sendTextboxes(id, slide.Descendants("privateText"));
        }
        private void sendPublicTextBoxes(int id, IEnumerable<XElement> shapes)
        {
            wire.SneakInto(id.ToString());
            foreach (var text in shapes)
            {
                var newText = new TextBox();
                newText.Text = text.Attribute("content").Value;
                InkCanvas.SetLeft(newText, Double.Parse(text.Attribute("x").Value));
                InkCanvas.SetTop(newText, Double.Parse(text.Attribute("y").Value));
                newText.Tag = DateTimeFactory.Now() + text.Attribute("x").Value + text.Attribute("x").Value + me;
                var font = text.Descendants("font").ElementAt(0);
                newText.FontFamily = new FontFamily(font.Attribute("family").Value);
                newText.FontSize = Double.Parse(font.Attribute("size").Value);
                newText.Foreground = (Brush)(new BrushConverter().ConvertFromString((font.Attribute("color").Value).ToString()));

                wire.SendTextbox(new TargettedTextBox
                {
                    slide = id,
                    author = me,
                    privacy = "public",
                    target = "presentationSpace",
                    box = newText
                });
            }
            wire.SneakOutOf(id.ToString());
        }
        private void sendShapes(int id, IEnumerable<XElement> shapes)
        {
            wire.SneakInto(id.ToString());
            int shapeCount = 0;
            foreach (var shape in shapes)
            {
                var uri = new Uri(shape.Attribute("uri").Value);
                var hostedImage = new Image();
                var bitmap = new BitmapImage(uri);
                hostedImage.Source = bitmap;
                InkCanvas.SetLeft(hostedImage, Double.Parse(shape.Attribute("x").Value));
                InkCanvas.SetTop(hostedImage, Double.Parse(shape.Attribute("y").Value));
                hostedImage.tag(new ImageTag
                                    {
                                        id = string.Format("{0}:{1}:{2}", me, DateTimeFactory.Now(), shapeCount++),
                                        author = me,
                                        privacy = shape.Attribute("privacy").Value,
                                        //isBackground = shapeCount == 1
                                        isBackground = false
                                    });
                wire.SendImage(new TargettedImage
                {
                    target = "presentationSpace",
                    author = me,
                    image = hostedImage,
                    slide = id,
                    privacy = shape.Attribute("privacy").Value
                });
            }
            wire.SneakOutOf(id.ToString());
        }
        private void sendTextboxes(int id, System.Collections.Generic.IEnumerable<XElement> shapes)
        {
            var privateRoom = id.ToString() + me;
            wire.SneakInto(privateRoom);
            var shapeCount = 0;
            var height = 0;
            foreach (var text in shapes)
            {
                var newText = new TextBox();
                newText.Text = text.Attribute("content").Value;
                InkCanvas.SetLeft(newText, 10);
                InkCanvas.SetTop(newText, 20 + height);
                newText.FontFamily = new FontFamily("Arial");
                newText.FontSize = 12;
                var newLines = newText.Text.Where(l => l.Equals('\r')).Count() + 1;
                height += (int)(newLines * (newText.FontSize));
                shapeCount++;
                newText.tag(new TextTag
                        {
                            author = me,
                            privacy = "private",
                            id = string.Format("{0}:{1}{2}", me, DateTimeFactory.Now(), shapeCount)
                        });
                ;
                wire.SendTextbox(new TargettedTextBox
                {
                    slide = id,
                    author = me,
                    privacy = "private",
                    target = "notepad",
                    box = newText
                });
            }
            wire.SneakOutOf(privateRoom);
        }
        private XElement uploadXmlUrls(int slide, XElement doc)
        {
            var shapeCount = doc.Descendants("shape").Count();
            for (var i = 0; i < shapeCount; i++)
            {
                var shape = doc.Descendants("shape").ElementAt(i);
                var file = shape.Attribute("snapshot").Value;
                var hostedFileName = ResourceUploader.uploadResource(slide.ToString(), file);
                var uri = new Uri(hostedFileName, UriKind.RelativeOrAbsolute);
                if (shape.Attribute("uri") == null)
                    shape.Add(new XAttribute("uri", uri));
                else
                    shape.Attribute("uri").Value = uri.ToString();
            }
            return doc;
        }
        private static bool check(MsoTriState cond)
        {
            return cond == MsoTriState.msoTrue;
        }
        private static void importSlide(XElement xml, Microsoft.Office.Interop.PowerPoint.Slide slide)
        {

            var xSlide = new XElement("slide");
            xSlide.Add(new XAttribute("index", slide.SlideIndex));
            var currentWorkingDirectory = Directory.GetCurrentDirectory() + "\\tmp";
            if (!Directory.Exists(currentWorkingDirectory))
                Directory.CreateDirectory(currentWorkingDirectory);
            var exportFormat = PpShapeFormat.ppShapeFormatPNG;
            var exportMode = PpExportMode.ppRelativeToSlide;
            var backgroundHeight = 540;
            var backgroundWidth = 720;
            try
            {
                if (backgroundHeight != Convert.ToInt32(slide.Master.Height))
                    backgroundHeight = Convert.ToInt32(slide.Master.Height);
                if (backgroundWidth != Convert.ToInt32(slide.Master.Width))
                    backgroundWidth = Convert.ToInt32(slide.Master.Width);
                var Backgroundfile = currentWorkingDirectory + "\\background" + (++resource) + ".jpg";
                foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in slide.Shapes)
                {
                    shape.Visible = MsoTriState.msoFalse;
                }
                //slide.Export(Backgroundfile, "PNG", 724, 543);
                slide.Export(Backgroundfile, "PNG", backgroundWidth, backgroundHeight);
                foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in slide.Shapes)
                {
                    if (shape.Tags.Count > 0 && shape.Tags.Value(shape.Tags.Count) == "Instructor")
                        shape.Visible = MsoTriState.msoFalse;
                    else shape.Visible = MsoTriState.msoTrue;
                }
                xSlide.Add(new XElement("shape",
                    new XAttribute("x", 0),
                    new XAttribute("y", 0),
                    new XAttribute("privacy", "public"),
                    new XAttribute("snapshot", Backgroundfile)));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex);
            }
            var SortedShapes = new List<Microsoft.Office.Interop.PowerPoint.Shape>();
            foreach (var shapeObj in slide.Shapes)
                SortedShapes.Add((Microsoft.Office.Interop.PowerPoint.Shape)shapeObj);
            foreach (var shapeObj in from p in SortedShapes orderby (p.ZOrderPosition) select p)
            {/*This wacky customer has been hand tuned to get the height and width that PPT renders faithfully at.  
                I have no idea why it has to be this way, it just looks right when it is.*/
                var shape = (Microsoft.Office.Interop.PowerPoint.Shape)shapeObj;
                var file = currentWorkingDirectory + "\\background" + (++resource).ToString() + ".jpg";
                var x = shape.Left;
                var y = shape.Top;
                /*if (shape.HasTextFrame == MsoTriState.msoTrue && shape.TextFrame.MarginLeft != 0)
                    x = x + shape.TextFrame.MarginLeft;
                if (shape.HasTextFrame == MsoTriState.msoTrue && shape.TextFrame2.MarginLeft != shape.TextFrame.MarginLeft)
                    x = x - shape.TextFrame.MarginLeft + shape.TextFrame2.MarginLeft;
                if (shape.HasTextFrame == MsoTriState.msoTrue && shape.TextFrame.MarginLeft != 0)
                    y = y + shape.TextFrame.MarginTop;
                if (shape.HasTextFrame == MsoTriState.msoTrue && shape.TextFrame2.MarginTop != shape.TextFrame.MarginTop)
                    y = y - shape.TextFrame.MarginTop + shape.TextFrame2.MarginTop;
                */
                string tags;
                if (shape.Type == MsoShapeType.msoInkComment)
                    tags = shape.Tags.ToString();
                //the ink doesn't appear to have vertices - I can't find the actual ink data
                if (shape.Type == MsoShapeType.msoPlaceholder)
                    //there're two of these on my sample slide.  They become the textboxes that have text in them, if you use the template's textbox placeholders.  Otherwise they'd be textboxes instead.
                    tags = shape.Tags.ToString();
                /*if (shape.HasDiagram == MsoTriState.msoTrue)
                    MessageBox.Show("has diagram");*/
                /*if (shape.HasChart == MsoTriState.msoTrue)
                    MessageBox.Show("has chart");
                if (shape.HasDiagram == MsoTriState.msoTrue && shape.HasDiagramNode == MsoTriState.msoTrue)
                    MessageBox.Show("has diagramNode");
                if (shape.HasTable == MsoTriState.msoTrue)
                    MessageBox.Show("has Table");
                */
                if (shape.Tags.Count > 0 && shape.Tags.Value(shape.Tags.Count) == "Instructor")
                {
                    try
                    {
                        shape.Visible = MsoTriState.msoTrue;
                        // Importing Powerpoint textboxes AS textboxes instead of pictures currently disabled.  
                        //Uncomment the following lines to re-enable.
                        /*if (shape.HasTextFrame == MsoTriState.msoTrue &&
                            shape.TextFrame.HasText == MsoTriState.msoTrue &&
                            !String.IsNullOrEmpty(shape.TextFrame.TextRange.Text))
                            addPublicText(xSlide, shape, "private");
                        else
                        */
                        {
                            try
                            {
                                shape.Export(file, PpShapeFormat.ppShapeFormatPNG, backgroundWidth, backgroundHeight, exportMode);
                                xSlide.Add(new XElement("shape",
                                    new XAttribute("x", x),
                                    new XAttribute("y", y),
                                    new XAttribute("privacy", "private"),
                                    new XAttribute("snapshot", file)));
                            }
                            catch (COMException)
                            {
                                //This shape doesn't export gracefully.  Continue looping through the others.
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception parsing private content: " + ex);
                    }
                }
                else if (shape.Visible == MsoTriState.msoTrue)
                    if (shape.Tags.Count > 0 && shape.Tags.Value(shape.Tags.Count) == "_")
                    {
                        // Importing Powerpoint textboxes AS textboxes instead of pictures currently disabled.  
                        //Uncomment the following lines to re-enable.
                        /*if (shape.HasTextFrame == MsoTriState.msoTrue &&
                            shape.TextFrame.HasText == MsoTriState.msoTrue &&
                            !String.IsNullOrEmpty(shape.TextFrame.TextRange.Text))
                            addPublicText(xSlide, shape, "public");
                        else
                        */
                        {
                            shape.Export(file, PpShapeFormat.ppShapeFormatPNG, backgroundWidth, backgroundHeight, exportMode);
                            xSlide.Add(new XElement("shape",
                                new XAttribute("x", x),
                                new XAttribute("y", y),
                                new XAttribute("privacy", "public"),
                                new XAttribute("snapshot", file)));
                        }
                    }
                    else
                    {
                        shape.Export(file, exportFormat, backgroundWidth, backgroundHeight, exportMode);
                        xSlide.Add(new XElement("shape",
                            new XAttribute("x", x),
                            new XAttribute("y", y),
                            new XAttribute("privacy", "public"),
                            new XAttribute("snapshot", file)));
                    }
            }
            foreach (var notes in slide.NotesPage.Shapes)
            {
                var shape = (Microsoft.Office.Interop.PowerPoint.Shape)notes;
                addText(xSlide, shape);
            }
            xml.Add(xSlide);
        }
        private static void addPublicText(XElement xSlide, Microsoft.Office.Interop.PowerPoint.Shape shape, string privacy)
        {
            //This should be used to create a RichTextbox, not a textbox, so that it can correctly represent PPT textboxes. 
            var textFrame = (Microsoft.Office.Interop.PowerPoint.TextFrame)shape.TextFrame;
            if (check(textFrame.HasText))
            {
                var pptcolour = textFrame.TextRange.Font.Color.RGB;
                var SystemDrawingColor = System.Drawing.ColorTranslator.FromOle(Int32.Parse((pptcolour.ToString())));
                var safeColour = (new Color { A = SystemDrawingColor.A, R = SystemDrawingColor.R, G = SystemDrawingColor.G, B = SystemDrawingColor.B }).ToString();
                xSlide.Add(new XElement("publicText",
                        new XAttribute("privacy", privacy),
                        new XAttribute("content", textFrame.TextRange.Text.Replace('\v', '\n')),
                        new XAttribute("x", shape.Left),
                        new XAttribute("y", shape.Top),
                        new XElement("font",
                            new XAttribute("family", textFrame.TextRange.Font.Name),
                            new XAttribute("size", textFrame.TextRange.Font.Size),
                            new XAttribute("color", safeColour))));
            }
        }
        private static void addText(XElement xSlide, Microsoft.Office.Interop.PowerPoint.Shape shape)
        {
            var textFrame = (Microsoft.Office.Interop.PowerPoint.TextFrame)shape.TextFrame;
            if (check(textFrame.HasText))
            {
                xSlide.Add(new XElement("privateText",
                        new XAttribute("content", textFrame.TextRange.Text.Replace('\v', '\n')),
                        new XAttribute("x", shape.Left),
                        new XAttribute("y", shape.Top),
                        new XElement("font",
                            new XAttribute("family", "Arial"),
                            new XAttribute("size", "12"),
                            new XAttribute("color", "Black"))));
            }
        }
    }
}