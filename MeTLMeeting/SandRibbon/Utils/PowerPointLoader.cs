using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Media;
using System.Xml.Linq;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.PowerPoint;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbonObjects;
using System.Windows;
using SandRibbon.Components;
using MeTLLib.DataTypes;
using SandRibbon.Providers;
using MeTLLib;
using System.Diagnostics;
using Newtonsoft.Json;
using SandRibbon.Components.Utility;

namespace SandRibbon.Utils
{
    public class WebThreadPool {
        private static Amib.Threading.SmartThreadPool pool = new Amib.Threading.SmartThreadPool { 
            MaxThreads=4
        };
        public static void QueueUserWorkItem(Amib.Threading.Action action){
            pool.QueueWorkItem(action);
        }
    }
    public class SneakyImage : MeTLStanzas.Image{
        public SneakyImage(String target,ImageTag tag,Uri src,double x, double y, int slide) : base(){ 
            SetTag(MeTLStanzas.tagTag, JsonConvert.SerializeObject(tag));
            SetTag(sourceTag, src.LocalPath);
            SetTag(MeTLStanzas.xTag, x);
            SetTag(MeTLStanzas.yTag, y);
            SetTag(MeTLStanzas.authorTag, tag.author);
            SetTag(MeTLStanzas.targetTag, target);
            SetTag(MeTLStanzas.privacyTag, tag.privacy.ToString());
            SetTag(MeTLStanzas.slideTag, slide);
            SetTag(MeTLStanzas.identityTag, tag.id);
        }
        public void SetWidth(double width) {
            SetTag(widthTag, width);
        }
        public void SetHeight(double height) {
            SetTag(heightTag, height);
        }
    }
    public class SneakyText : MeTLStanzas.TextBox {
        public SneakyText(String target, String text, String family, double size,
            String color, TextTag tag, double x,double y,int slide) { 
            SetTag(MeTLStanzas.xTag, x);
            SetTag(MeTLStanzas.yTag, y);
            this.SetTag(MeTLStanzas.TextBox.textTag, text);
            this.SetTag(MeTLStanzas.TextBox.familyTag, family);
            this.SetTag(MeTLStanzas.TextBox.sizeTag, size);
            this.SetTag(MeTLStanzas.TextBox.colorTag, color);
            this.SetTag(MeTLStanzas.tagTag, JsonConvert.SerializeObject(tag));
            this.SetTag(MeTLStanzas.authorTag, tag.author);
            this.SetTag(MeTLStanzas.identityTag, tag.id);
            this.SetTag(MeTLStanzas.targetTag, target);
            this.SetTag(MeTLStanzas.privacyTag, tag.privacy.ToString());
            this.SetTag(MeTLStanzas.slideTag, slide);
            this.SetTag(MeTLStanzas.TextBox.decorationTag, "None");
            this.SetTag(MeTLStanzas.TextBox.weightTag, "Normal");
            this.SetTag(MeTLStanzas.TextBox.styleTag, "Normal");
        }
        public void SetWidth(double width) {
            SetTag(widthTag, width);
        }
        public void SetHeight(double height) {
            SetTag(heightTag, height);
        }
    }
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
        public PowerpointImportProgress(IMPORT_STAGE Stage, int CurrentSlide, int TotalSlides,string thumbnail)
            :this(Stage,CurrentSlide)
        {
            totalSlides = TotalSlides;
            slideThumbnailSource = thumbnail;
        }
        public int slideId;
        public int totalSlides;
        public enum IMPORT_STAGE { DESCRIBED, ANALYSED, EXTRACTED_IMAGES, UPLOADED_XML, UPLOADED_RESOURCES, FINISHED, PRINTING };
        public IMPORT_STAGE stage;
        public string slideThumbnailSource{get;set;}
    }
    public class PowerPointLoader
    {
        private const MsoTriState FALSE = MsoTriState.msoFalse;
        private const MsoTriState TRUE = MsoTriState.msoTrue;
        private static int resource = 1;
        private MeTLLib.ClientConnection clientConnection;
        private string currentConversation = null;
        public enum PowerpointImportType
        {
            HighDefImage,
            Image,
            Shapes
        }
        public PowerPointLoader()
        {
            Commands.UploadPowerpoint.RegisterCommandToDispatcher(new DelegateCommand<PowerpointSpec>(UploadPowerpoint));
            clientConnection = MeTLLib.ClientFactory.Connection();
        }
        private class PowerpointLoadTracker {
            private int slidesUploaded = 0;
            private readonly int target;
            private String conversationJid;
            private DelegateCommand<object> cancel = new DelegateCommand<object>(delegate { }, _unused => false);
            public PowerpointLoadTracker(int _target, String _conversation) {
                target = _target;
                conversationJid = _conversation;
                Commands.ReceiveImage.RegisterCommand(cancel);
                Commands.ReceiveTextBox.RegisterCommand(cancel);
            }
            public void increment() {
                Interlocked.Increment(ref slidesUploaded);
                if (slidesUploaded == target)
                {
                    // completed the import
                    Commands.ReceiveImage.UnregisterCommand(cancel);
                    Commands.ReceiveTextBox.UnregisterCommand(cancel);

                    // join successfully created conversation 
                    Commands.JoinConversation.Execute(conversationJid);
                }
            }
        }
        private void UploadPowerpoint(PowerpointSpec spec)
        {
            if (IsPowerPointRunning())
            {
                MeTLMessage.Information("PowerPoint seems to be running, please close the program before importing a presentation");
                return;
            }

            var app = GetPowerPointApplication();
            if (app == null)
                return;
            
            var conversation = ClientFactory.Connection().CreateConversation(spec.Details);
            var worker = new Thread(new ParameterizedThreadStart(
                delegate
                {
                    try
                    {
                        progress(PowerpointImportProgress.IMPORT_STAGE.DESCRIBED, 0, 0);
                        switch (spec.Type)
                        {
                            case PowerpointImportType.HighDefImage:
                                Trace.TraceInformation("ImportingPowerpoint HighDef {0}", spec.File);
                                Logger.Log(string.Format("ImportingPowerpoint HighDef {0}", spec.File));
                                LoadPowerpointAsFlatSlides(app, spec.File, conversation, spec.Magnification);
                                break;
                            case PowerpointImportType.Image:
                                Trace.TraceInformation("ImportingPowerpoint NormalDef {0}", spec.File);
                                Logger.Log(string.Format("ImportingPowerpoint NormalDef {0}", spec.File));
                                LoadPowerpointAsFlatSlides(app, spec.File, conversation, spec.Magnification);
                                break;
                            case PowerpointImportType.Shapes:
                                Trace.TraceInformation("ImportingPowerpoint Flexible {0}", spec.File);
                                Logger.Log(string.Format("ImportingPowerpoint Flexible {0}", spec.File));
                                LoadPowerpoint(app, spec.File, conversation);
                                break;
                        }

                    }
                    catch (Exception)
                    {
                        ClientFactory.Connection().DeleteConversation(conversation);
                    }
                }));
            worker.SetApartmentState(ApartmentState.STA);
            worker.Start();
        }
        public void CreateBlankConversation()
        {
            var details = new ConversationDetails(ConversationDetails.DefaultName(Globals.me), "", Globals.me, new List<MeTLLib.DataTypes.Slide>(), Permissions.LECTURE_PERMISSIONS, "Unrestricted");
            Commands.HideConversationSearchBox.Execute(null);
            Commands.CreateConversation.ExecuteAsync(details);
        }
        public void ImportPowerpoint(Window owner)
        {
            var configDialog = new ConversationConfigurationDialog(ConversationConfigurationDialog.ConversationConfigurationMode.IMPORT);
            configDialog.Owner = owner;
            configDialog.ChooseFileForImport();

            var spec = configDialog.Import();
            if (spec == null) return;
            UploadPowerpoint(spec);
        }

        private PowerPoint.Application GetPowerPointApplication()
        {
            try
            {
                return new PowerPoint.Application();
            }
            catch (Exception)
            {
                MeTLMessage.Error("MeTL requires Microsoft PowerPoint to be installed to import a presentation");
                Commands.HideProgressBlocker.ExecuteAsync(null);
            }

            return null;
        }

        private bool IsPowerPointRunning()
        {
            var procList = Process.GetProcessesByName("POWERPNT");
            return procList.Count() != 0;
        }

        public void LoadPowerpointAsFlatSlides(PowerPoint.Application app, string file, ConversationDetails conversation, int MagnificationRating)
        {
            var ppt = app.Presentations.Open(file, TRUE, FALSE, FALSE);
            var currentWorkingDirectory = LocalFileProvider.getUserFolder("tmp");
            if (!Directory.Exists(currentWorkingDirectory))
                Directory.CreateDirectory(currentWorkingDirectory);

            var convDescriptor = new ConversationDescriptor(conversation, new XElement("presentation"));

            convDescriptor.Xml.Add(new XAttribute("name", conversation.Title));
            if (conversation.Tag == null)
                conversation.Tag = "unTagged";
            currentConversation = conversation.Jid;
            conversation.Author = Globals.me;
            var backgroundWidth = ppt.SlideMaster.Width * MagnificationRating;
            var backgroundHeight = ppt.SlideMaster.Height * MagnificationRating;
            var thumbnailStartId = conversation.Slides.First().id;
            try
            {
                foreach (Microsoft.Office.Interop.PowerPoint.Slide slide in ppt.Slides)
                {
                    var slidePath = getThumbnailPath(conversation.Jid, thumbnailStartId++);
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
                    new XAttribute("background", true),
                    new XAttribute("snapshot", slidePath)));
                    convDescriptor.Xml.Add(xSlide);
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

                        // add the speaker notes
                        var speakerNotes = slide.NotesPage.Shapes.Placeholders[2];
                        if (speakerNotes != null)
                        {
                            speakerNotes.Tags.Add("speakerNotes", "true");
                            privateShapes.Add(speakerNotes);
                        }
                    }
                    catch (Exception)
                    {
                    }
                    foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in (from p in privateShapes orderby (p.ZOrderPosition) select p))
                    {
                        ExportShape(convDescriptor, shape, xSlide, currentWorkingDirectory, exportFormat, exportMode, actualBackgroundWidth, actualBackgroundHeight, Magnification);
                    }
                    progress(PowerpointImportProgress.IMPORT_STAGE.EXTRACTED_IMAGES, 0, ppt.Slides.Count);
                }
            }
            catch (COMException e)
            {
                Logger.Crash(e);
            }
            finally
            {
                ppt.Close();
            }
            var startingId = conversation.Slides.First().id;
            var index = 0;
            conversation.Slides = convDescriptor.Xml.Descendants("slide").Select(d => new MeTLLib.DataTypes.Slide(startingId++,Globals.me,MeTLLib.DataTypes.Slide.TYPE.SLIDE,index++,float.Parse(d.Attribute("defaultWidth").Value),float.Parse(d.Attribute("defaultHeight").Value))).ToList();
            var updatedConversation = ClientFactory.Connection().UpdateConversationDetails(conversation);
            if (!updatedConversation.ValueEquals(conversation))
            {
                Trace.TraceInformation("PowerpointImport: Failed to update conversation");
            }
            UploadFromXml(convDescriptor);
        }

        private void UploadFromXml(ConversationDescriptor conversationDescriptor)
        {
            var xmlSlides = conversationDescriptor.Xml.Descendants("slide");
            var slideCount = xmlSlides.Count();
            var tracker = new PowerpointLoadTracker(slideCount, conversationDescriptor.Details.Jid);
            for (var i = 0; i < slideCount; i++)
            {
                var slideXml = xmlSlides.ElementAt(i);
                var slideId = conversationDescriptor.Details.Slides[i].id;
                var slideIndex = i;
                WebThreadPool.QueueUserWorkItem(delegate
                {
                    uploadXmlUrls(slideId, slideXml);
                    sendSlide(slideId, slideXml, conversationDescriptor);
                    progress(PowerpointImportProgress.IMPORT_STAGE.ANALYSED, slideIndex, slideCount);
                    tracker.increment();
                });
            }
        }
        public static string getThumbnailPath(string jid, int id)
        {
            string fullPath = createThumbnailFileStructure(jid);
            var path = string.Format("{0}{1}.png", fullPath, id);
            return path;
        }
        private static string createThumbnailFileStructure(string jid)
        {
            var thumbsDirectory = LocalFileProvider.getUserFolder("thumbs");
            var myThumbsDirectory = LocalFileProvider.getUserFolder(new string[] { "thumbs", Globals.me });
            var myConversationThumbsDirectory = LocalFileProvider.getUserFolder(new string[] { "thumbs", Globals.me, jid });
            if (!Directory.Exists(thumbsDirectory))//string.Format("{0}\\thumbs\\", Directory.GetCurrentDirectory())))
                Directory.CreateDirectory(thumbsDirectory);//string.Format("{0}\\thumbs\\", Directory.GetCurrentDirectory()));
            if(!Directory.Exists(myThumbsDirectory))//string.Format("{0}\\thumbs\\{1}\\", Directory.GetCurrentDirectory(), Globals.me)))
                Directory.CreateDirectory(myThumbsDirectory);//string.Format("{0}\\thumbs\\{1}\\", Directory.GetCurrentDirectory(), Globals.me));
            var fullPath = string.Format(myConversationThumbsDirectory);//"{0}\\thumbs\\{1}\\{2}\\", Directory.GetCurrentDirectory(), Globals.me, jid);
            if(!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);
            return fullPath;
        }
        private static void progress(PowerpointImportProgress.IMPORT_STAGE action, int currentSlideId)
        {
            Commands.UpdatePowerpointProgress.Execute(new PowerpointImportProgress(action, currentSlideId));
        }
        private static void progress(PowerpointImportProgress.IMPORT_STAGE action, int currentSlideId, int totalNumberOfSlides) 
        {
            Commands.UpdatePowerpointProgress.Execute(new PowerpointImportProgress(action, currentSlideId, totalNumberOfSlides));
        }
        private static void progress(PowerpointImportProgress.IMPORT_STAGE action, int currentSlideId, int totalSlides, string imageSource)
        {
            Commands.UpdatePowerpointProgress.Execute(new PowerpointImportProgress(action, currentSlideId, totalSlides, imageSource));
        }
        public void LoadPowerpoint(PowerPoint.Application app, string file, ConversationDetails conversation)
        {
            try
            {
                var ppt = app.Presentations.Open(file, TRUE, FALSE, FALSE);
                var provider = ClientFactory.Connection();
                var convDescriptor = new ConversationDescriptor(conversation, new XElement("presentation"));
                convDescriptor.Xml.Add(new XAttribute("name", conversation.Title));
                if (conversation.Tag == null)
                    conversation.Tag = "unTagged";
                currentConversation = conversation.Jid;
                conversation.Author = Globals.me;
                try
                {
                    foreach (var slide in ppt.Slides)
                    {
                        importSlide(convDescriptor, (Microsoft.Office.Interop.PowerPoint.Slide)slide);
                        progress(PowerpointImportProgress.IMPORT_STAGE.EXTRACTED_IMAGES, -1, ppt.Slides.Count);//All the consumers count for themselves
                    }
                    var startingId = conversation.Slides.First().id;
                    var index = 0;
                    conversation.Slides = convDescriptor.Xml.Descendants("slide").Select(d => new MeTLLib.DataTypes.Slide
                    (startingId++, Globals.me, MeTLLib.DataTypes.Slide.TYPE.SLIDE, index++, float.Parse(d.Attribute("defaultWidth").Value), float.Parse(d.Attribute("defaultHeight").Value))).ToList();
                    provider.UpdateConversationDetails(conversation);
                    UploadFromXml(convDescriptor);
                }
                catch (COMException e)
                {
                    Logger.Crash(e);
                }
                finally
                {
                    ppt.Close();
                }
            }
            catch (Exception e) {
                Logger.Crash(e);
            }
        }
        
        private void sendSlide(int id, XElement slide, ConversationDescriptor conversationDescriptor)
        {
            bool hasPrivate = conversationDescriptor.HasPrivateContent; 
            var privateRoom = string.Format("{0}{1}", id, Globals.me);
            if(hasPrivate) 
                clientConnection.SneakInto(privateRoom);
            clientConnection.SneakInto(id.ToString());
            sneakilySendShapes(id, slide.Descendants("shape"));
            sneakilySendPublicTextBoxes(id, slide.Descendants("publicText"));
            if (hasPrivate)
                clientConnection.SneakOutOf(privateRoom);
            clientConnection.SneakOutOf(id.ToString());
        }
        private void sneakilySendPublicTextBoxes(int id, IEnumerable<XElement> shapes)
        {
            int shapeCount = 0;
            foreach (var text in shapes)
            {
                var target = text.Attribute("target").Value;
                var content = text.Attribute("content").Value;
                var x = Double.Parse(text.Attribute("x").Value);
                var y = Double.Parse(text.Attribute("y").Value);
                var textBoxIdentity = DateTimeFactory.Now() + text.Attribute("x").Value + text.Attribute("x").Value + Globals.me + shapeCount++;
                var font = text.Descendants("font").ElementAt(0);
                var privacy = (Privacy)Enum.Parse(typeof(Privacy), text.Attribute("privacy").Value, true);
                var family = font.Attribute("family").Value;
                var size = Double.Parse(font.Attribute("size").Value);
                var color = (font.Attribute("color").Value).ToString();
                var tag = new TextTag
                    {
                        author = Globals.me,
                        id = textBoxIdentity,
                        privacy = privacy
                    };
                var stanza = new SneakyText(target,content,family,size,color,tag,x,y,id);
                if (text.Attributes("height").Count() > 0)
                    stanza.SetHeight(Double.Parse(text.Attribute("height").Value));
                if (text.Attributes("width").Count() > 0)
                    stanza.SetWidth(Double.Parse(text.Attribute("width").Value));
                clientConnection.SendStanza(id.ToString(),stanza);
            }
        }
        private void sneakilySendShapes(int id, IEnumerable<XElement> shapes) { 
            int shapeCount = 0;
            var me = Globals.me;
            var target = "presentationSpace";
            foreach (var shape in shapes)
            {
                bool isBackgroundImage = false;
                if (shape.Attribute("background") != null && shape.Attribute("background").Value.ToLower() == "true")
                    isBackgroundImage = true;
                var tag = new ImageTag {
                    id = string.Format("{0}:{1}:{2}", me, DateTimeFactory.Now().Ticks, shapeCount++),
                    author = me,
                    privacy = (Privacy)Enum.Parse(typeof(Privacy), shape.Attribute("privacy").Value, true),
                    isBackground = isBackgroundImage
                };
                var uri = new Uri(shape.Attribute("uri").Value);
                var x = Double.Parse(shape.Attribute("x").Value);
                var y = Double.Parse(shape.Attribute("y").Value);
                var stanza = new SneakyImage(target,tag,uri,x,y,id);
                if (shape.Attributes("width").Count() > 0)
                    stanza.SetWidth( Double.Parse(shape.Attribute("width").Value));
                if (shape.Attributes("height").Count() > 0)
                    stanza.SetHeight( Double.Parse(shape.Attribute("height").Value));
                clientConnection.SendStanza(id.ToString(), stanza);
            }
        }
        private XElement uploadXmlUrls(int slide, XElement doc)
        {
            var conn = MeTLLib.ClientFactory.Connection();
            var shapeCount = doc.Descendants("shape").Count();
            for (var i = 0; i < shapeCount; i++)
            {
                var shape = doc.Descendants("shape").ElementAt(i);
                var file = shape.Attribute("snapshot").Value;
                var hostedFileUri = conn.UploadResource(new Uri(file, UriKind.RelativeOrAbsolute), slide.ToString());
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
        private static void importSlide(ConversationDescriptor conversationDescriptor, Microsoft.Office.Interop.PowerPoint.Slide slide)
        {
            var xSlide = new XElement("slide");
            xSlide.Add(new XAttribute("index", slide.SlideIndex));
            var currentWorkingDirectory = LocalFileProvider.getUserFolder("tmp");//getAppropriateFolder("tmp");
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
                var Backgroundfile = currentWorkingDirectory + "background" + (++resource) + ".jpg";
                //var Backgroundfile = currentWorkingDirectory + "\\background" + (++resource) + ".jpg";
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
                    new XAttribute("height", backgroundHeight * Magnification),
                    new XAttribute("width", backgroundWidth * Magnification),
                    new XAttribute("privacy", "public"),
                    new XAttribute("snapshot", Backgroundfile),
                    new XAttribute("background", true)));
            }
            catch (Exception ex)
            {
                Trace.TraceError("SandRibbon::Utils::PowerpointLoader:Exception: " + ex);
            }
            var SortedShapes = new List<Microsoft.Office.Interop.PowerPoint.Shape>();
            foreach (var shapeObj in slide.Shapes)
                SortedShapes.Add((Microsoft.Office.Interop.PowerPoint.Shape)shapeObj);
            foreach (var shapeObj in from p in SortedShapes orderby (p.ZOrderPosition) select p)
            {/*This wacky customer has been hand tuned to get the height and width that PPT renders faithfully at.  
                I have no idea why it has to be this way, it just looks right when it is.*/
                ExportShape(conversationDescriptor, shapeObj, xSlide, currentWorkingDirectory, exportFormat, exportMode, backgroundWidth, backgroundHeight, Magnification);
            }
            // Placeholders[2] is always the speaker notes
            var speakerNotes = slide.NotesPage.Shapes.Placeholders[2];
            if (speakerNotes != null)
            {
                addSpeakerNotes(conversationDescriptor, xSlide, speakerNotes, Magnification);
            }
            conversationDescriptor.Xml.Add(xSlide);
        }

        private static bool HasExportableText(PowerPoint.Shape shape)
        {
            return shape.HasTextFrame == TRUE && shape.TextFrame.HasText == TRUE && !String.IsNullOrEmpty(shape.TextFrame.TextRange.Text);
        }

        private static void ExportShape(ConversationDescriptor conversationDescriptor, Microsoft.Office.Interop.PowerPoint.Shape shapeObj, XElement xSlide, string currentWorkingDirectory, PpShapeFormat exportFormat,
            PpExportMode exportMode,
            int backgroundWidth,
            int backgroundHeight,
            double Magnification)
        {
            var shape = (Microsoft.Office.Interop.PowerPoint.Shape)shapeObj;
            var file = currentWorkingDirectory + "background" + (++resource).ToString() + ".jpg";
            //var file = currentWorkingDirectory + "\\background" + (++resource).ToString() + ".jpg";
            string tags;
            if (shape.Type == MsoShapeType.msoInkComment)
                tags = shape.Tags.ToString();
            //the ink doesn't appear to have vertices - I can't find the actual ink data
            if (shape.Type == MsoShapeType.msoPlaceholder)
                //there're two of these on my sample slide.  They become the textboxes that have text in them, if you use the template's textbox placeholders.  Otherwise they'd be textboxes instead.
                tags = shape.Tags.ToString();

            var speakerNotes = shape.Tags["speakerNotes"];
            if (!string.IsNullOrEmpty(speakerNotes) && speakerNotes == "true")
            {
                addSpeakerNotes(conversationDescriptor, xSlide, shape, Magnification);
            }
            else if ((shape.Tags.Count > 0 && shape.Tags.Value(shape.Tags.Count) == "Instructor") || shape.Visible == FALSE)
            {
                try
                {
                    shape.Visible = MsoTriState.msoTrue;
                    if (HasExportableText(shape))
                    {
                        conversationDescriptor.HasPrivateContent = true;
                        addPublicText(xSlide, shape, "private", "presentationSpace", Magnification);
                    }
                    else
                    {
                        conversationDescriptor.HasPrivateContent = true;
                        addShape(xSlide, shape, Privacy.Private, file, exportMode, backgroundWidth, backgroundHeight, Magnification);
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceInformation("PowerpointLoader::LoadShape: Exception parsing private content: " + ex);
                }
            }
            else if (shape.Visible == MsoTriState.msoTrue)
            //if (shape.Tags.Count > 0 && shape.Tags.Value(shape.Tags.Count) == "_")
            {
                if (HasExportableText(shape))
                    addPublicText(xSlide, shape, "public", "presentationSpace", Magnification);
                else
                {
                    addShape(xSlide, shape, Privacy.Public, file, exportMode, backgroundWidth, backgroundHeight, Magnification);
                }
            }
        }

        private static void addShape(XElement xSlide, PowerPoint.Shape shape, Privacy privacy, string file, PpExportMode exportMode, int backgroundWidth, int backgroundHeight, double Magnification)
        {
            try
            {
                var x = shape.Left;
                var y = shape.Top;
                shape.Export(file, PpShapeFormat.ppShapeFormatPNG, backgroundWidth, backgroundHeight, exportMode);
                xSlide.Add(new XElement("shape",
                    new XAttribute("x", x * Magnification),
                    new XAttribute("y", y * Magnification),
                    new XAttribute("height", shape.Height * Magnification),
                    new XAttribute("width", shape.Width * Magnification),
                    new XAttribute("privacy", privacy.ToString().ToLower()),
                    new XAttribute("snapshot", file),
                    new XAttribute("background", privacy == Privacy.Private ? false : true)));
            }
            catch (Exception e)
            {
                Trace.TraceError("Failed to export shape. Reason: " + e.Message);
            }
        }

        private static void addPublicText(XElement xSlide, Microsoft.Office.Interop.PowerPoint.Shape shape, string privacy, string target, double Magnification)
        {
            //This should be used to create a RichTextbox, not a textbox, so that it can correctly represent PPT textboxes. 
            var textFrame = (Microsoft.Office.Interop.PowerPoint.TextFrame)shape.TextFrame;
            if (check(textFrame.HasText))
            {
                int pptcolour;
                if (textFrame.TextRange.Text.Length > 0)
                    pptcolour = textFrame.TextRange.Runs(0, 1).Font.Color.RGB;
                else
                     pptcolour = textFrame.TextRange.Font.Color.RGB;
                var SystemDrawingColor = System.Drawing.ColorTranslator.FromOle(Int32.Parse((pptcolour.ToString())));
                var safeColour = (new Color { A = SystemDrawingColor.A, R = SystemDrawingColor.R, G = SystemDrawingColor.G, B = SystemDrawingColor.B }).ToString();
                string safeFont = "arial";
                if (textFrame.TextRange.Font.Name != null)
                    safeFont = textFrame.TextRange.Font.Name;

                var shapeX = shape.Left * Magnification;
                var shapeY = shape.Top * Magnification;
                var fontSizeFactor = 1.0;
                // overrides for the notepad target
                if (target == "notepad")
                {
                    shapeX = 5;
                    shapeY = 5;
                    fontSizeFactor = 2.0;
                }

                xSlide.Add(new XElement("publicText",
                        new XAttribute("privacy", privacy),
                        new XAttribute("target", target),
                        new XAttribute("content", textFrame.TextRange.Text.Replace('\v', '\n')),
                        new XAttribute("x", shapeX),
                        new XAttribute("y", shapeY),
                        new XAttribute("width", shape.Width * Magnification),
                        new XAttribute("height", shape.Height * Magnification),
                        new XElement("font",
                            new XAttribute("family", safeFont),
                            new XAttribute("size", textFrame.TextRange.Font.Size * fontSizeFactor * Magnification),
                            new XAttribute("color", safeColour))));
            }
        }

        private static void addSpeakerNotes(ConversationDescriptor conversationDescriptor, XElement xSlide, Microsoft.Office.Interop.PowerPoint.Shape placeholder, double Magnification)
        {
            conversationDescriptor.HasPrivateContent = true;
            addPublicText(xSlide, placeholder, "private", "notepad", Magnification);
        }
    }
}