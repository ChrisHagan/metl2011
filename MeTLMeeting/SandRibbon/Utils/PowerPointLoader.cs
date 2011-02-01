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
using SandRibbonObjects;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;
using System.Windows;
using SandRibbon.Components;
using System.Collections.ObjectModel;
using MeTLLib.DataTypes;
using SandRibbon.Providers;
using MeTLLib;
using System.ComponentModel;
using System.Diagnostics;
using MeTLLib.Providers.Connection;

namespace SandRibbon.Utils
{
    public class PowerpointSpec
    {
        public string File;
        public ConversationDetails Details;
        public PowerPointLoader.PowerpointImportType Type;
        public int Magnification;
    }
    public class PowerpointImportProgress
    {
        public PowerpointImportProgress(IMPORT_STAGE Stage, int CurrentSlide)
        {
            stage = Stage;
            slideId = CurrentSlide;
        }
        public PowerpointImportProgress(IMPORT_STAGE Stage, int CurrentSlide, int TotalSlides)
            :this(Stage,CurrentSlide)
        {
            totalSlides = TotalSlides;
        }
        public PowerpointImportProgress(IMPORT_STAGE Stage, int CurrentSlide, string thumbnail)
            :this(Stage,CurrentSlide)
        {
            slideThumbnailSource = thumbnail;
        }
        public int slideId;
        public int totalSlides;
        public enum IMPORT_STAGE { DESCRIBED, ANALYSED, EXTRACTED_IMAGES, UPLOADED_XML, UPLOADED_RESOURCES, FINISHED };
        public IMPORT_STAGE stage;
        public string slideThumbnailSource{get;set;}
    }
    public class PowerPointLoader
    {
        private static MsoTriState FALSE = MsoTriState.msoFalse;
        private static MsoTriState TRUE = MsoTriState.msoTrue;
        private static int resource = 1;
        private MeTLLib.ClientConnection wire;
        private MeTLLib.MeTLLibEventHandlers.ImageAvailableEventHandler waitHandler;
        public enum PowerpointImportType
        {
            HighDefImage,
            Image,
            Shapes
        }
        public PowerPointLoader()
        {
            Commands.EditConversation.RegisterCommandToDispatcher(new DelegateCommand<string>(EditConversation));
            Commands.UploadPowerpoint.RegisterCommandToDispatcher(new DelegateCommand<PowerpointSpec>(UploadPowerpoint));
            wire = MeTLLib.ClientFactory.Connection();
        }
        private void UploadPowerpoint(PowerpointSpec spec)
        {
            var worker = new Thread(new ParameterizedThreadStart(
                delegate
                {
                    switch (spec.Type)
                    {
                        case PowerpointImportType.HighDefImage:
                            LoadPowerpointAsFlatSlides(spec.File, spec.Details, spec.Magnification);
                            break;
                        case PowerpointImportType.Image:
                            LoadPowerpointAsFlatSlides(spec.File, spec.Details, spec.Magnification);
                            break;
                        case PowerpointImportType.Shapes:
                            LoadPowerpoint(spec.File, spec.Details);
                            break;
                    }
                }));
            worker.SetApartmentState(ApartmentState.STA);
            worker.Start();
        }
        private void EditConversation(string conversation)
        {
            new ConversationConfigurationDialog(ConversationConfigurationDialog.ConversationConfigurationMode.EDIT, conversation).ShowDialog();
        }
        public void CreateBlankConversation()
        {
            var details = new ConversationDetails(ConversationDetails.DefaultName(Globals.me), "", Globals.me, new List<MeTLLib.DataTypes.Slide>(), Permissions.LECTURE_PERMISSIONS, "Unrestricted");
            Commands.HideConversationSearchBox.Execute(null);
            Commands.CreateConversation.ExecuteAsync(details);
        }
        public void ImportPowerpoint()
        {
            var spec = new ConversationConfigurationDialog(ConversationConfigurationDialog.ConversationConfigurationMode.IMPORT).Import();
            if (spec == null) return;
            UploadPowerpoint(spec);
        }
        public void LoadPowerpointAsFlatSlides(string file, ConversationDetails details, int MagnificationRating)
        {
            progress(PowerpointImportProgress.IMPORT_STAGE.DESCRIBED, 0, 0);
            var ppt = new ApplicationClass().Presentations.Open(file, TRUE, FALSE, FALSE);
            var currentWorkingDirectory = Directory.GetCurrentDirectory() + "\\tmp";
            if (!Directory.Exists(currentWorkingDirectory))
                Directory.CreateDirectory(currentWorkingDirectory);
            var provider = ClientFactory.Connection();
            var xml = new XElement("presentation");
            xml.Add(new XAttribute("name", details.Title));
            if (details.Tag == null)
                details.Tag = "unTagged";
            var conversation = provider.CreateConversation(details);
            conversation.Author = Globals.me;
            var backgroundWidth = ppt.SlideMaster.Width * MagnificationRating;
            var backgroundHeight = ppt.SlideMaster.Height * MagnificationRating;
            var thumbnailStartId = conversation.Slides.First().id;
            foreach (Microsoft.Office.Interop.PowerPoint.Slide slide in ppt.Slides)
            {
                int conversationSlideNumber = (Int32.Parse(details.Jid) + slide.SlideNumber);
                var slidePath = getThumbnailPath( conversation.Jid, thumbnailStartId++);
                foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in slide.Shapes)
                {
                    shape.Visible = MsoTriState.msoFalse;
                }
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
                slide.Export(slidePath, "PNG", (int)backgroundWidth, (int)backgroundHeight);
                progress(PowerpointImportProgress.IMPORT_STAGE.ANALYSED, conversationSlideNumber, slidePath);
                var xSlide = new XElement("slide");
                xSlide.Add(new XAttribute("index", slide.SlideIndex));
                xSlide.Add(new XAttribute("defaultHeight", backgroundHeight));
                xSlide.Add(new XAttribute("defaultWidth", backgroundWidth));
                xSlide.Add(new XElement("shape",
                new XAttribute("x", 0),
                new XAttribute("y", 0),
                new XAttribute("height", backgroundHeight),
                new XAttribute("width", backgroundWidth),
                new XAttribute("privacy", "public"),
                new XAttribute("snapshot", slidePath)));
                xml.Add(xSlide);
                var exportFormat = PpShapeFormat.ppShapeFormatPNG;
                var exportMode = PpExportMode.ppRelativeToSlide;
                var actualBackgroundHeight = 540;
                var actualBackgroundWidth = 720;
                double Magnification = (double)MagnificationRating;
                try
                {
                    if (actualBackgroundHeight != Convert.ToInt32(slide.Master.Height))
                        actualBackgroundHeight = Convert.ToInt32(slide.Master.Height);
                    if (actualBackgroundWidth != Convert.ToInt32(slide.Master.Width))
                        actualBackgroundWidth = Convert.ToInt32(slide.Master.Width);
                }
                catch (Exception)
                {
                }
                foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in (from p in privateShapes orderby (p.ZOrderPosition) select p))
                {
                    ExportShape(shape, xSlide, currentWorkingDirectory, exportFormat, exportMode, actualBackgroundWidth, actualBackgroundHeight, Magnification);
                }
                progress(PowerpointImportProgress.IMPORT_STAGE.EXTRACTED_IMAGES, conversationSlideNumber);
            }
            ppt.Close();
            var startingId = conversation.Slides.First().id;
            var index = 0;
            conversation.Slides = xml.Descendants("slide").Select(d => new MeTLLib.DataTypes.Slide(startingId++,Globals.me,MeTLLib.DataTypes.Slide.TYPE.SLIDE,index++,float.Parse(d.Attribute("defaultWidth").Value),float.Parse(d.Attribute("defaultHeight").Value))).ToList();
            provider.UpdateConversationDetails(conversation);
            var xmlSlides = xml.Descendants("slide");
            int xmlSlidesCount = xmlSlides.Count();
            waitForSlideToHaveContentThenJoin(conversation.Slides.Last().id, conversation.Jid);
            for (int i = 0; i < xmlSlidesCount; i++)
            {
                var slideXml = xmlSlides.ElementAt(i);
                var slideId = conversation.Slides[i].id;
                uploadXmlUrls(slideId, slideXml);
                progress(PowerpointImportProgress.IMPORT_STAGE.UPLOADED_XML, slideId, xmlSlidesCount);
                sendSlide(slideId, slideXml);
                progress(PowerpointImportProgress.IMPORT_STAGE.UPLOADED_RESOURCES, slideId, xmlSlidesCount);
            }
        }

        public static string getThumbnailPath(string jid, int id)
        {
            string fullPath = createThumbnailFileStructure(jid);
            var path = string.Format("{0}\\{1}.png", fullPath, id);
            return path;
        }
        private static string createThumbnailFileStructure(string jid)
        {
            if (!Directory.Exists(string.Format("{0}\\thumbs\\", Directory.GetCurrentDirectory())))
                Directory.CreateDirectory(string.Format("{0}\\thumbs\\", Directory.GetCurrentDirectory()));
            if(!Directory.Exists(string.Format("{0}\\thumbs\\{1}\\", Directory.GetCurrentDirectory(), Globals.me)))
                Directory.CreateDirectory(string.Format("{0}\\thumbs\\{1}\\", Directory.GetCurrentDirectory(), Globals.me));
            var fullPath = string.Format("{0}\\thumbs\\{1}\\{2}\\", Directory.GetCurrentDirectory(), Globals.me, jid);
            if(!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);
            return fullPath;
        }
        private void waitForSlideToHaveContentThenJoin(int slideToWaitOn, string conversationToJoin) {
            waitHandler = (sender, args) =>
            {
                if (args.image.slide == slideToWaitOn)
                {
                    progress(PowerpointImportProgress.IMPORT_STAGE.FINISHED, 0);
                    Commands.JoinConversation.Execute(conversationToJoin);
                    wire.events.ImageAvailable -= waitHandler;
                }
            };
            wire.events.ImageAvailable += waitHandler;
        }
        private static void progress(PowerpointImportProgress.IMPORT_STAGE action, int currentSlideId)
        {
            Commands.UpdatePowerpointProgress.Execute(new PowerpointImportProgress(action, currentSlideId));
        }
        private static void progress(PowerpointImportProgress.IMPORT_STAGE action, int currentSlideId, string imageSource)
        {
            Commands.UpdatePowerpointProgress.Execute(new PowerpointImportProgress(action, currentSlideId, imageSource));
        }
        private static void progress(PowerpointImportProgress.IMPORT_STAGE action, int currentSlideId, int totalNumberOfSlides) 
        {
            Commands.UpdatePowerpointProgress.Execute(new PowerpointImportProgress(action, currentSlideId, totalNumberOfSlides));
        }
        public void LoadPowerpoint(string file, ConversationDetails details)
        {
            var ppt = new ApplicationClass().Presentations.Open(file, TRUE, FALSE, FALSE);
            try
            {
                progress(PowerpointImportProgress.IMPORT_STAGE.DESCRIBED, 0, ppt.Slides.Count);
                var provider = ClientFactory.Connection();
                var xml = new XElement("presentation");
                xml.Add(new XAttribute("name", details.Title));
                if (details.Tag == null)
                    details.Tag = "unTagged";
                var conversation = provider.CreateConversation(details);
                conversation.Author = Globals.me;
                progress(PowerpointImportProgress.IMPORT_STAGE.DESCRIBED, 0, ppt.Slides.Count);
                foreach (var slide in ppt.Slides)
                {
                    importSlide(details, xml, (Microsoft.Office.Interop.PowerPoint.Slide)slide);
                }
                var startingId = conversation.Slides.First().id;
                var index = 0;
                conversation.Slides = xml.Descendants("slide").Select(d => new MeTLLib.DataTypes.Slide 
                (startingId++,Globals.me,MeTLLib.DataTypes.Slide.TYPE.SLIDE,index++,float.Parse(d.Attribute("defaultWidth").Value),float.Parse(d.Attribute("defaultHeight").Value))).ToList();
                provider.UpdateConversationDetails(conversation);
                var xmlSlides = xml.Descendants("slide");
                for (var i = 0; i < xmlSlides.Count(); i++)
                {
                    var slideXml = xmlSlides.ElementAt(i);
                    var slideId = conversation.Slides[i].id;
                    var powerpointThread = new Thread(() =>
                    {
                        uploadXmlUrls(slideId, slideXml);
                        progress(PowerpointImportProgress.IMPORT_STAGE.UPLOADED_XML, slideId);
                        sendSlide(slideId, slideXml);
                        progress(PowerpointImportProgress.IMPORT_STAGE.UPLOADED_RESOURCES, slideId);
                    });
                    powerpointThread.SetApartmentState(ApartmentState.STA);
                    powerpointThread.Start();
                }
                Commands.JoinConversation.ExecuteAsync(conversation.Jid);
            }
            finally
            {
                ppt.Close();
            }
        }
        private void sendSlide(int id, XElement slide)
        {
            sendShapes(id, slide.Descendants("shape"));
            sendPublicTextBoxes(id, slide.Descendants("publicText"));
            //sendTextboxes(id, slide.Descendants("privateText"));
        }
        private void sendPublicTextBoxes(int id, IEnumerable<XElement> shapes)
        {
            wire.SneakInto(id.ToString());
            int shapeCount = 0;
            foreach (var text in shapes)
            {
                var newText = new TextBox();
                newText.Text = text.Attribute("content").Value;
                InkCanvas.SetLeft(newText, Double.Parse(text.Attribute("x").Value));
                InkCanvas.SetTop(newText, Double.Parse(text.Attribute("y").Value));
                if (text.Attributes("height").Count() > 0)
                    newText.Height = Double.Parse(text.Attribute("height").Value);
                if (text.Attributes("width").Count() > 0)
                    newText.Width = Double.Parse(text.Attribute("width").Value);
                var textBoxIdentity = DateTimeFactory.Now() + text.Attribute("x").Value + text.Attribute("x").Value + Globals.me + shapeCount++;
                var font = text.Descendants("font").ElementAt(0);
                var privacy = text.Attribute("privacy").Value.ToString();
                newText.FontFamily = new FontFamily(font.Attribute("family").Value);
                newText.FontSize = Double.Parse(font.Attribute("size").Value);
                newText.Foreground = (Brush)(new BrushConverter().ConvertFromString((font.Attribute("color").Value).ToString()));
                newText.tag(
                    new TextTag
                    {
                        author = Globals.me,
                        id = textBoxIdentity,
                        privacy = privacy
                    });
                wire.SendTextBox(new TargettedTextBox(id,Globals.me,"presentationSpace",privacy,newText));
            }
        }
        private void sendShapes(int id, IEnumerable<XElement> shapes)
        {
            wire.SneakInto(id.ToString());
            int shapeCount = 0;
            var me = Globals.me;
            foreach (var shape in shapes)
            {
                var uri = new Uri(shape.Attribute("uri").Value);
                var hostedImage = new Image();
                var bitmap = new BitmapImage(uri);
                hostedImage.Source = bitmap;
                InkCanvas.SetLeft(hostedImage, Double.Parse(shape.Attribute("x").Value));
                InkCanvas.SetTop(hostedImage, Double.Parse(shape.Attribute("y").Value));
                if (shape.Attributes("height").Count() > 0)
                    hostedImage.Height = Double.Parse(shape.Attribute("height").Value);
                if (shape.Attributes("width").Count() > 0)
                    hostedImage.Width = Double.Parse(shape.Attribute("width").Value);
                hostedImage.tag(new ImageTag
                                    {
                                        id = string.Format("{0}:{1}:{2}", me, DateTimeFactory.Now(), shapeCount++),
                                        author = me,
                                        privacy = shape.Attribute("privacy").Value,
                                        isBackground = false
                                    });
                wire.SendImage(new TargettedImage(id,me,"presentationSpace",shape.Attribute("privacy").Value,hostedImage));
            }
        }
        private void sendTextboxes(int id, IEnumerable<XElement> shapes)
        {
            var me = Globals.me;
            var privateRoom = id + me;
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
                            id = string.Format("{0}:{1}{2}", me, DateTimeFactory.Now(), shapeCount++)
                        });
                ;
                wire.SendTextBox(new TargettedTextBox (id,me,"presentationSpace","private",newText));
            }
        }
        private XElement uploadXmlUrls(int slide, XElement doc)
        {
            var shapeCount = doc.Descendants("shape").Count();
            for (var i = 0; i < shapeCount; i++)
            {
                var shape = doc.Descendants("shape").ElementAt(i);
                var file = shape.Attribute("snapshot").Value;
                var hostedFileUri = MeTLLib.ClientFactory.Connection().UploadResource(new Uri(file, UriKind.RelativeOrAbsolute), slide.ToString());
                var uri = hostedFileUri;
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
        private static void importSlide(ConversationDetails details, XElement xml, Microsoft.Office.Interop.PowerPoint.Slide slide)
        {
            int conversationSlideNumber = (Int32.Parse(details.Jid) + slide.SlideNumber);
            var xSlide = new XElement("slide");
            xSlide.Add(new XAttribute("index", slide.SlideIndex));
            var currentWorkingDirectory = Directory.GetCurrentDirectory() + "\\tmp";
            if (!Directory.Exists(currentWorkingDirectory))
                Directory.CreateDirectory(currentWorkingDirectory);
            var exportFormat = PpShapeFormat.ppShapeFormatPNG;
            var exportMode = PpExportMode.ppRelativeToSlide;
            var backgroundHeight = 540;
            var backgroundWidth = 720;
            xSlide.Add(new XAttribute("defaultHeight", backgroundHeight));
            xSlide.Add(new XAttribute("defaultWidth", backgroundWidth));
            double Magnification = 1;
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
                Trace.TraceError("Exception: " + ex);
            }
            var SortedShapes = new List<Microsoft.Office.Interop.PowerPoint.Shape>();
            foreach (var shapeObj in slide.Shapes)
                SortedShapes.Add((Microsoft.Office.Interop.PowerPoint.Shape)shapeObj);
            foreach (var shapeObj in from p in SortedShapes orderby (p.ZOrderPosition) select p)
            {/*This wacky customer has been hand tuned to get the height and width that PPT renders faithfully at.  
                I have no idea why it has to be this way, it just looks right when it is.*/
                ExportShape(shapeObj, xSlide, currentWorkingDirectory, exportFormat, exportMode, backgroundWidth, backgroundHeight, Magnification);
            }
            progress(PowerpointImportProgress.IMPORT_STAGE.EXTRACTED_IMAGES, conversationSlideNumber);
            foreach (var notes in slide.NotesPage.Shapes)
            {
                var shape = (Microsoft.Office.Interop.PowerPoint.Shape)notes;
                addText(xSlide, shape, Magnification);
            }
            xml.Add(xSlide);
        }
        private static void ExportShape(
            Microsoft.Office.Interop.PowerPoint.Shape shapeObj,
            XElement xSlide,
            string currentWorkingDirectory,
            PpShapeFormat exportFormat,
            PpExportMode exportMode,
            int backgroundWidth,
            int backgroundHeight,
            double Magnification)
        {
            var shape = (Microsoft.Office.Interop.PowerPoint.Shape)shapeObj;
            var file = currentWorkingDirectory + "\\background" + (++resource).ToString() + ".jpg";
            var x = shape.Left;
            var y = shape.Top;
            string tags;
            if (shape.Type == MsoShapeType.msoInkComment)
                tags = shape.Tags.ToString();
            //the ink doesn't appear to have vertices - I can't find the actual ink data
            if (shape.Type == MsoShapeType.msoPlaceholder)
                //there're two of these on my sample slide.  They become the textboxes that have text in them, if you use the template's textbox placeholders.  Otherwise they'd be textboxes instead.
                tags = shape.Tags.ToString();
            if ((shape.Tags.Count > 0 && shape.Tags.Value(shape.Tags.Count) == "Instructor") || shape.Visible == FALSE)
            {
                try
                {
                    shape.Visible = MsoTriState.msoTrue;
                    if (shape.HasTextFrame == MsoTriState.msoTrue &&
                        shape.TextFrame.HasText == MsoTriState.msoTrue &&
                        !String.IsNullOrEmpty(shape.TextFrame.TextRange.Text))
                        addPublicText(xSlide, shape, "private", Magnification);
                    else
                    {
                        try
                        {
                            shape.Export(file, PpShapeFormat.ppShapeFormatPNG, backgroundWidth, backgroundHeight, exportMode);
                            xSlide.Add(new XElement("shape",
                                new XAttribute("x", x * Magnification),
                                new XAttribute("y", y * Magnification),
                                new XAttribute("height", shape.Height * Magnification),
                                new XAttribute("width", shape.Width * Magnification),
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
                    if (shape.HasTextFrame == MsoTriState.msoTrue &&
                        shape.TextFrame.HasText == MsoTriState.msoTrue &&
                        !String.IsNullOrEmpty(shape.TextFrame.TextRange.Text))
                        addPublicText(xSlide, shape, "public", Magnification);
                    else
                    {
                        shape.Export(file, PpShapeFormat.ppShapeFormatPNG, backgroundWidth, backgroundHeight, exportMode);
                        xSlide.Add(new XElement("shape",
                            new XAttribute("x", x * Magnification),
                            new XAttribute("y", y * Magnification),
                            new XAttribute("height", shape.Height * Magnification),
                            new XAttribute("width", shape.Width * Magnification),
                            new XAttribute("privacy", "public"),
                            new XAttribute("snapshot", file)));
                    }
                }
                else
                {
                    if (shape.HasTextFrame == MsoTriState.msoTrue &&
                        shape.TextFrame.HasText == MsoTriState.msoTrue &&
                        !String.IsNullOrEmpty(shape.TextFrame.TextRange.Text))
                        addPublicText(xSlide, shape, "public", Magnification);
                    else
                    {
                        shape.Export(file, exportFormat, backgroundWidth, backgroundHeight, exportMode);
                        xSlide.Add(new XElement("shape",
                            new XAttribute("x", x * Magnification),
                            new XAttribute("y", y * Magnification),
                            new XAttribute("height", shape.Height * Magnification),
                            new XAttribute("width", shape.Width * Magnification),
                            new XAttribute("privacy", "public"),
                            new XAttribute("snapshot", file)));
                    }
                }
        }

        private static void addPublicText(XElement xSlide, Microsoft.Office.Interop.PowerPoint.Shape shape, string privacy, double Magnification)
        {
            //This should be used to create a RichTextbox, not a textbox, so that it can correctly represent PPT textboxes. 
            var textFrame = (Microsoft.Office.Interop.PowerPoint.TextFrame)shape.TextFrame;
            if (check(textFrame.HasText))
            {
                var pptcolour = textFrame.TextRange.Font.Color.RGB;
                var SystemDrawingColor = System.Drawing.ColorTranslator.FromOle(Int32.Parse((pptcolour.ToString())));
                var safeColour = (new Color { A = SystemDrawingColor.A, R = SystemDrawingColor.R, G = SystemDrawingColor.G, B = SystemDrawingColor.B }).ToString();
                string safeFont = "arial";
                if (textFrame.TextRange.Font.Name != null)
                    safeFont = textFrame.TextRange.Font.Name;
                xSlide.Add(new XElement("publicText",
                        new XAttribute("privacy", privacy),
                        new XAttribute("content", textFrame.TextRange.Text.Replace('\v', '\n')),
                        new XAttribute("x", shape.Left * Magnification),
                        new XAttribute("y", shape.Top * Magnification),
                        new XAttribute("width", shape.Width * Magnification),
                        new XAttribute("height", shape.Height * Magnification),
                        new XElement("font",
                            new XAttribute("family", safeFont),
                            new XAttribute("size", textFrame.TextRange.Font.Size * Magnification),
                            new XAttribute("color", safeColour))));
            }
        }
        private static void addText(XElement xSlide, Microsoft.Office.Interop.PowerPoint.Shape shape, double Magnification)
        {
            var textFrame = (Microsoft.Office.Interop.PowerPoint.TextFrame)shape.TextFrame;
            if (check(textFrame.HasText))
            {
                xSlide.Add(new XElement("privateText",
                        new XAttribute("content", textFrame.TextRange.Text.Replace('\v', '\n')),
                        new XAttribute("x", shape.Left * Magnification),
                        new XAttribute("y", shape.Top * Magnification),
                        new XElement("font",
                            new XAttribute("family", "Arial"),
                            new XAttribute("size", "12"),
                            new XAttribute("color", "Black"))));
            }
        }
    }
}