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
    public class WebThreadPool
    {
        private static Amib.Threading.SmartThreadPool pool = new Amib.Threading.SmartThreadPool
        {
            MaxThreads = 4
        };
        public static void QueueUserWorkItem(Amib.Threading.Action action)
        {
            pool.QueueWorkItem(action);
        }
    }
    public class PowerpointSpec
    {
        public string File;
        public ConversationDetails Details;
        public PowerpointImportType Type;
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
            : this(Stage, CurrentSlide)
        {
            totalSlides = TotalSlides;
        }
        public PowerpointImportProgress(IMPORT_STAGE Stage, int CurrentSlide, string thumbnail)
            : this(Stage, CurrentSlide)
        {
            slideThumbnailSource = thumbnail;
        }
        public PowerpointImportProgress(IMPORT_STAGE Stage, int CurrentSlide, int TotalSlides, string thumbnail)
            : this(Stage, CurrentSlide)
        {
            totalSlides = TotalSlides;
            slideThumbnailSource = thumbnail;
        }
        public int slideId;
        public int totalSlides;
        public enum IMPORT_STAGE { DESCRIBED, ANALYSED, EXTRACTED_IMAGES, UPLOADED_XML, UPLOADED_RESOURCES, FINISHED, PRINTING };
        public IMPORT_STAGE stage;
        public string slideThumbnailSource { get; set; }
    }
    public enum PowerpointImportType
    {
        HighDefImage,
        Image,
        Shapes
    }

    public class PowerPointLoader
    {
        public static readonly PowerpointImportType SHAPES = PowerpointImportType.Shapes;
        public static readonly PowerpointImportType IMAGE = PowerpointImportType.Image;
        public static readonly PowerpointImportType HIGHDEFIMAGE = PowerpointImportType.HighDefImage;
        private const MsoTriState FALSE = MsoTriState.msoFalse;
        private const MsoTriState TRUE = MsoTriState.msoTrue;
        public NetworkController networkController { get; protected set; }
        public PowerPointLoader(NetworkController _controller)
        {
            networkController = _controller;
        }
        protected static readonly string TOTAL = "total";
        protected static readonly string LOCAL = "local";
        protected static readonly string SLIDE = "slide";
        protected static readonly string SERVER = "server";

        public void UploadPowerpoint(PowerpointSpec spec, Action<string, string, int, int> onProgress, Action<ConversationDetails> onComplete)
        {
            var totalCount = 0;
            var totalTotal = 5;

            try
            {
                onProgress(TOTAL, "import starting", 0, totalTotal);
                var healthy = true;
                if (IsPowerPointRunning())
                {
                    MeTLMessage.Error("Microsoft Powerpoint is already running.  Using serverside import instead.  This may result in some missing elements (wordart, clipped images, etc).  Please close Microsoft Powerpoint before importing presentations.");
                    healthy = false;
                }
                onProgress(TOTAL, "powerpoint running check", totalCount++, totalTotal);
                var app = GetPowerPointApplication();
                onProgress(TOTAL, "powerpoint installation check", totalCount++, totalTotal);
                if (app == null)
                {
                    MeTLMessage.Error("MeTL prefers Microsoft PowerPoint to be installed to import a presentation.  Using serverside import instead.  This may result in some missing elements (wordart, clipped images, etc).  For more accurate import of Microsoft Powerpoint files, please install Microsoft Powerpoint.");
                    healthy = false;
                }
                onProgress(TOTAL, "ready to begin", totalCount++, totalTotal);
                switch (spec.Type)
                {
                    case PowerpointImportType.HighDefImage:
                        if (healthy)
                        {
                            LoadPowerpointAsFlatSlides(app, spec.File, spec.Magnification, onProgress, onComplete, totalCount, totalTotal);
                        }
                        else
                        {
                            LoadPowerpointAsFlatSlidesFromServer(spec.File, spec.Magnification, onProgress, onComplete, totalCount, totalTotal);
                        }
                        break;
                    case PowerpointImportType.Image:
                        if (healthy)
                        {
                            LoadPowerpointAsFlatSlides(app, spec.File, spec.Magnification, onProgress, onComplete, totalCount, totalTotal);
                        }
                        else
                        {
                            LoadPowerpointAsFlatSlidesFromServer(spec.File, spec.Magnification, onProgress, onComplete, totalCount, totalTotal);
                        }
                        break;
                    case PowerpointImportType.Shapes:
                        if (healthy)
                        {
                            LoadPowerpoint(app, spec.File, onProgress, onComplete, totalCount, totalTotal);
                        }
                        else
                        {
                            LoadPowerpointFromServer(spec.File, onProgress, onComplete, totalCount, totalTotal);
                        }
                        break;
                    default:
                        onComplete(ConversationDetails.Empty);
                        break;

                }
            }
            catch (Exception ex)
            {
                onComplete(ConversationDetails.Empty);
            }
        }
        public PowerPoint.Application GetPowerPointApplication()
        {
            try
            {
                return new PowerPoint.Application();
            }
            catch (Exception)
            {

            }
            return null;
        }
        public bool IsPowerPointInstalled()
        {
            return GetPowerPointApplication() != null;
        }
        public bool IsPowerPointRunning()
        {
            var procList = Process.GetProcessesByName("POWERPNT");
            return procList.Count() != 0;
        }
        protected XElement message(XElement inner)
        {
            var m = new XElement("message");
            m.Add(new XAttribute("timestamp", (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds.ToString()));
            m.Add(inner);
            return m;
        }

        public ConversationDetails LoadPowerpointFromServer(string file, Action<string, string, int, int> onProgress, Action<ConversationDetails> onComplete, int totalCount, int totalTotal)
        {
            var localCount = 0;
            var localTotal = 4;
            onProgress(LOCAL, "reading local file", 0, localTotal);
            var fileBytes = File.ReadAllBytes(file);
            onProgress(LOCAL, "local file read", localCount++, localTotal);
            onProgress(TOTAL, "local file read", 4, 5);
            onProgress(SERVER, "ready to send to server", 0, 2);
            onProgress(SERVER, "sending to server", 1, 2);
            var remoteString = networkController.client.resourceProvider.securePutData(networkController.config.importPowerpointFlexible(file), fileBytes);
            onProgress(SERVER, "server response available", 2, 2);
            onProgress(LOCAL, "ready to parse server response", localCount++, localTotal);
            var convXml = XElement.Parse(remoteString);
            onProgress(LOCAL, "server response parsed", localCount++, localTotal);
            var finalConv = ConversationDetails.ReadXml(convXml);
            onProgress(LOCAL, "local conversation ready", localCount++, localTotal);
            onProgress(TOTAL, "remote conversation ready", totalCount++, totalTotal);
            onComplete(finalConv);
            return finalConv;
        }
        public ConversationDetails LoadPowerpointAsFlatSlidesFromServer(string file, int MaginificationRating, Action<string, string, int, int> onProgress, Action<ConversationDetails> onComplete, int totalCount, int totalTotal)
        {
            var localCount = 0;
            var localTotal = 4;
            onProgress(LOCAL, "reading local file", 0, localTotal);
            var fileBytes = File.ReadAllBytes(file);
            onProgress(LOCAL, "local file read", localCount++, localTotal);
            onProgress(TOTAL, "local file read", 4, 5);
            onProgress(SERVER, "ready to send to server", 0, localTotal);
            onProgress(SERVER, "sending to server", 1, 2);
            var remoteString = networkController.client.resourceProvider.securePutData(networkController.config.importPowerpoint(file, MaginificationRating), fileBytes);
            onProgress(SERVER, "server response available", 2, 2);
            onProgress(LOCAL, "ready to parse server response", localCount++, localTotal);
            var convXml = XElement.Parse(remoteString);
            onProgress(LOCAL, "server response parsed", localCount++, localTotal);
            var finalConv = ConversationDetails.ReadXml(convXml);
            onProgress(LOCAL, "local conversation ready", localCount++, localTotal);
            onProgress(TOTAL, "remote conversation ready", totalCount++, totalTotal);
            onComplete(finalConv);
            return finalConv;
        }
        public ConversationDetails LoadPowerpointAsFlatSlides(PowerPoint.Application app, string file, int MagnificationRating, Action<string, string, int, int> onProgress, Action<ConversationDetails> onComplete, int totalCount, int totalTotal)
        {
            Presentation ppt = null;
            var resource = 1;
            var currentWorkingDirectory = LocalFileProvider.getUserFolder("tmp");

            var convXml = new XElement("export");
            var histories = new XElement("histories");

            var slides = new List<MeTLLib.DataTypes.Slide>();
            var permissions = new Permissions("restrictedByPowerpoint", false, false, true);
            var startingJid = 1000;
            var conversation = new ConversationDetails(file, startingJid.ToString(), networkController.credentials.name, slides, permissions, networkController.credentials.name);
            conversation.Tag = "";
            conversation.blacklist = new List<string>();
            conversation.Slides = slides;
            try
            {
                var localCount = 0;
                onProgress(LOCAL, "opening powerpoint", localCount++, 6);
                ppt = app.Presentations.Open(file, TRUE, FALSE, FALSE);
                var backgroundWidth = ppt.SlideMaster.Width * MagnificationRating;
                var backgroundHeight = ppt.SlideMaster.Height * MagnificationRating;
                var slidesCount = ppt.Slides.Count;
                var localTotal = slidesCount + 6;
                foreach (Microsoft.Office.Interop.PowerPoint.Slide slide in ppt.Slides)
                {
                    var slideCount = 0;
                    var slideTotal = 7;
                    onProgress(LOCAL, "parsing slides", localCount++, localTotal);
                    onProgress(SLIDE, "getting slide", 0, slideTotal);
                    var slideJid = startingJid + slide.SlideIndex;
                    var tempFile = currentWorkingDirectory + "background" + (++resource).ToString() + ".jpg";
                    onProgress(SLIDE, "hiding instructor content", slideCount++, slideTotal);
                    /*

                    foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in slide.Shapes)
                    {
                        shape.Visible = MsoTriState.msoFalse;
                    }
                    foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in slide.Shapes)
                    {
                        if (shape.Tags.Count > 0 && shape.Tags.Value(shape.Tags.Count) == "Instructor")
                            shape.Visible = MsoTriState.msoFalse;
                        else shape.Visible = MsoTriState.msoTrue;
                    }
                    */
                    onProgress(SLIDE, "snapshotting slide", slideCount++, slideTotal);
                    slide.Export(tempFile, "JPG", (int)backgroundWidth, (int)backgroundHeight);
                    var history = new XElement("history");
                    history.Add(new XAttribute("jid", slideJid.ToString()));
                    onProgress(SLIDE, "adding slide to conversation", slideCount++, slideTotal);
                    conversation.Slides.Add(new MeTLLib.DataTypes.Slide(
                        slideJid,
                        networkController.credentials.name,
                        MeTLLib.DataTypes.Slide.TYPE.SLIDE,
                        slide.SlideIndex - 1,
                        backgroundWidth,
                        backgroundHeight));
                    var imageElem = new XElement("image");
                    onProgress(SLIDE, "constructing metl content", slideCount++, slideTotal);
                    var imageIdentity = string.Format("{0}:{1}:{2}", networkController.credentials.name, DateTimeFactory.Now().Ticks, 1);
                    var tag = new ImageTag
                    {
                        id = imageIdentity,
                        author = networkController.credentials.name,
                        privacy = Privacy.Public,
                        isBackground = false,
                        resourceIdentity = imageIdentity,
                        timestamp = DateTime.Now.Ticks,
                        zIndex = -1
                    };
                    new List<KeyValuePair<string, string>> {
                        new KeyValuePair<string, string>("imageBytes",System.Convert.ToBase64String(File.ReadAllBytes(tempFile))),
                        new KeyValuePair<string, string>(MeTLStanzas.authorTag,networkController.credentials.name),
                        new KeyValuePair<string, string>(MeTLStanzas.targetTag,GlobalConstants.PRESENTATIONSPACE),
                        new KeyValuePair<string, string>(MeTLStanzas.privacyTag,Privacy.Public.ToString()),
                        new KeyValuePair<string, string>(MeTLStanzas.slideTag,slideJid.ToString()),
                        new KeyValuePair<string, string>(MeTLStanzas.identityTag,tag.id),
                        new KeyValuePair<string, string>(MeTLStanzas.tagTag,JsonConvert.SerializeObject(tag)),
                        new KeyValuePair<string, string>(MeTLStanzas.Image.widthTag,backgroundWidth.ToString()),
                        new KeyValuePair<string, string>(MeTLStanzas.Image.heightTag,backgroundHeight.ToString()),
                        new KeyValuePair<string, string>(MeTLStanzas.xTag,"0"),
                        new KeyValuePair<string, string>(MeTLStanzas.yTag,"0")
                    }.ForEach(kvp =>
                    {
                        imageElem.Add(new XElement(kvp.Key, kvp.Value));
                    });
                    onProgress(SLIDE, "removing snapshot", slideCount++, slideTotal);
                    File.Delete(tempFile);
                    history.Add(message(imageElem));
                    onProgress(SLIDE, "adding metl content to slide", slideCount++, slideTotal);
                    histories.Add(history);
                    onProgress(SLIDE, "adding slide to history", slideCount++, slideTotal);
                }
                onProgress(LOCAL, "constructing server request", localCount++, localTotal);
                convXml.Add(conversation.WriteXml());
                convXml.Add(histories);
                onProgress(LOCAL, "server request ready", localCount++, localTotal);
                var url = networkController.config.importConversation();
                onProgress(SERVER, "ready to send to server", 0, localTotal);
                onProgress(SERVER, "sending to server", 1, 2);
                var remoteConvString = networkController.client.resourceProvider.securePutData(url, System.Text.Encoding.UTF8.GetBytes(convXml.ToString()));
                onProgress(SERVER, "server response available", 2, 2);
                onProgress(LOCAL, "parsing server response", localCount++, localTotal);
                var remoteConvXml = XElement.Parse(remoteConvString);
                onProgress(LOCAL, "parsing conversation", localCount++, localTotal);
                var remoteConv = ConversationDetails.ReadXml(remoteConvXml);
                onProgress(LOCAL, "remote conversation ready", localCount++, localTotal);
                onProgress(TOTAL, "remote conversation ready", totalCount++, totalTotal);
                onComplete(remoteConv);
                return remoteConv;
            }
            catch (Exception e)
            {
                return LoadPowerpointAsFlatSlidesFromServer(file, MagnificationRating, onProgress, onComplete, totalCount, totalTotal);
            }
            finally
            {
                if (ppt != null)
                    ppt.Close();
            }
        }

        public ConversationDetails LoadPowerpoint(PowerPoint.Application app, string file, Action<string, string, int, int> onProgress, Action<ConversationDetails> onComplete, int totalCount, int totalTotal)
        {
            Presentation ppt = null;
            var resource = 1;
            var convXml = new XElement("export");
            var histories = new XElement("histories");

            var currentWorkingDirectory = LocalFileProvider.getUserFolder("tmp");

            var slides = new List<MeTLLib.DataTypes.Slide>();
            var permissions = new Permissions("restrictedByPowerpoint", false, false, true);
            var startingJid = 1000;
            var title = String.Format("", file, DateTime.Now);
            var conversation = new ConversationDetails(file, startingJid.ToString(), networkController.credentials.name, slides, permissions, networkController.credentials.name);
            conversation.Tag = "";
            conversation.blacklist = new List<string>();
            conversation.Slides = slides;
            try
            {
                var localCount = 0;
                var localTotal = 7;
                onProgress(LOCAL, "opening powerpoint", localCount, localTotal);
                ppt = app.Presentations.Open(file, TRUE, FALSE, FALSE);
                onProgress(LOCAL, "powerpoint opened", localCount++, localTotal);
                var backgroundWidth = (int)ppt.SlideMaster.Width;
                var backgroundHeight = (int)ppt.SlideMaster.Height;
                localTotal = localTotal + ppt.Slides.Count;
                onProgress(LOCAL, "parsing slides", localCount++, localTotal);
                foreach (Microsoft.Office.Interop.PowerPoint.Slide slide in ppt.Slides)
                {
                    try
                    {
                        var slideCount = 0;
                        var slideTotal = 6;
                        slideTotal = slide.Shapes.Count + slideTotal;
                        onProgress(SLIDE, "parsing slide", slideCount, slideTotal);
                        var shapeCount = 0;
                        if (backgroundHeight != Convert.ToInt32(slide.Master.Height))
                            backgroundHeight = Convert.ToInt32(slide.Master.Height);
                        if (backgroundWidth != Convert.ToInt32(slide.Master.Width))
                            backgroundWidth = Convert.ToInt32(slide.Master.Width);
                        var slideJid = startingJid + slide.SlideIndex;
                        var tempFile = currentWorkingDirectory + "background" + (++resource).ToString() + ".jpg";
                        slide.Export(tempFile, "JPG", (int)backgroundWidth, (int)backgroundHeight);
                        onProgress(SLIDE, "", slideCount++, slideTotal);
                        var history = new XElement("history");
                        history.Add(new XAttribute("jid", slideJid.ToString()));
                        var bgImageElem = new XElement("image");
                        conversation.Slides.Add(new MeTLLib.DataTypes.Slide(
                            slideJid,
                            networkController.credentials.name,
                            MeTLLib.DataTypes.Slide.TYPE.SLIDE,
                            slide.SlideIndex - 1,
                            backgroundWidth,
                            backgroundHeight));

                        onProgress(SLIDE, "", slideCount++, slideTotal);
                        var backgroundFile = currentWorkingDirectory + "background" + (++resource) + ".jpg";
                        foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in slide.Shapes)
                        {
                            shape.Visible = MsoTriState.msoFalse;
                        }
                        slide.Export(backgroundFile, "JPG", backgroundWidth, backgroundHeight);
                        foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in slide.Shapes)
                        {
                            if (shape.Tags.Count > 0 && shape.Tags.Value(shape.Tags.Count) == "Instructor")
                                shape.Visible = MsoTriState.msoFalse;
                            else shape.Visible = MsoTriState.msoTrue;
                        }
                        onProgress(SLIDE, "", slideCount++, slideTotal);
                        var imageIdentity = string.Format("{0}:{1}:{2}", networkController.credentials.name, DateTimeFactory.Now().Ticks, shapeCount++);
                        var tag = new ImageTag
                        {
                            id = imageIdentity,
                            author = networkController.credentials.name,
                            privacy = Privacy.Public,
                            isBackground = false,
                            resourceIdentity = imageIdentity,
                            timestamp = DateTime.Now.Ticks,
                            zIndex = -1
                        };

                        new List<KeyValuePair<string, string>> {
                            new KeyValuePair<string, string>("imageBytes",System.Convert.ToBase64String(File.ReadAllBytes(backgroundFile))),
                            new KeyValuePair<string, string>(MeTLStanzas.authorTag,networkController.credentials.name),
                            new KeyValuePair<string, string>(MeTLStanzas.targetTag,GlobalConstants.PRESENTATIONSPACE),
                            new KeyValuePair<string, string>(MeTLStanzas.privacyTag,Privacy.Public.ToString()),
                            new KeyValuePair<string, string>(MeTLStanzas.slideTag,slideJid.ToString()),
                            new KeyValuePair<string, string>(MeTLStanzas.identityTag,tempFile),
                            new KeyValuePair<string, string>(MeTLStanzas.tagTag,JsonConvert.SerializeObject(tag)),
                            new KeyValuePair<string, string>(MeTLStanzas.Image.widthTag,backgroundWidth.ToString()),
                            new KeyValuePair<string, string>(MeTLStanzas.Image.heightTag,backgroundHeight.ToString()),
                            new KeyValuePair<string, string>(MeTLStanzas.xTag,"0"),
                            new KeyValuePair<string, string>(MeTLStanzas.yTag,"0")
                        }.ForEach(kvp =>
                        {
                            bgImageElem.Add(new XElement(kvp.Key, kvp.Value));
                        });
                        File.Delete(tempFile);
                        history.Add(message(bgImageElem));
                        onProgress(SLIDE, "", slideCount++, slideTotal);
                        var z = 0;
                        var SortedShapes = new List<Microsoft.Office.Interop.PowerPoint.Shape>();
                        foreach (var shapeObj in slide.Shapes)
                            SortedShapes.Add((Microsoft.Office.Interop.PowerPoint.Shape)shapeObj);
                        foreach (var shapeObj in from p in SortedShapes orderby (p.ZOrderPosition) select p)
                        {
                            var shape = (Microsoft.Office.Interop.PowerPoint.Shape)shapeObj;
                            string tags;
                            if (shape.Type == MsoShapeType.msoInkComment)
                                tags = shape.Tags.ToString();
                            //the ink doesn't appear to have vertices - I can't find the actual ink data
                            if (shape.Type == MsoShapeType.msoPlaceholder)
                                //there're two of these on my sample slide.  They become the textboxes that have text in them, if you use the template's textbox placeholders.  Otherwise they'd be textboxes instead.
                                tags = shape.Tags.ToString();

                            else if ((shape.Tags.Count > 0 && shape.Tags.Value(shape.Tags.Count) == "Instructor") || shape.Visible == FALSE)
                            {
                                try
                                {
                                    var shapePrivacy = ((shape.Tags.Count > 0 && shape.Tags.Value(shape.Tags.Count) == "Instructor") || shape.Visible == FALSE) ? Privacy.Private : Privacy.Public;
                                    if (HasExportableText(shape))
                                    {
                                        //This should be used to create a RichTextbox, not a textbox, so that it can correctly represent PPT textboxes. 
                                        var textFrame = (Microsoft.Office.Interop.PowerPoint.TextFrame)shape.TextFrame;
                                        if (textFrame.HasText == MsoTriState.msoTrue)
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

                                            var shapeX = shape.Left;
                                            var shapeY = shape.Top;
                                            var fontSizeFactor = 1.0;
                                            // overrides for the notepad target
                                            var speakerNotes = shape.Tags["speakerNotes"];
                                            var target = "presentationSpace";
                                            if (!string.IsNullOrEmpty(speakerNotes) && speakerNotes == "true")
                                            {
                                                target = "notepad";
                                            }
                                            if (target == "notepad")
                                            {
                                                shapeX = 5;
                                                shapeY = 5;
                                                fontSizeFactor = 2.0;
                                            }
                                            var textElem = new XElement("text");
                                            var shapeTag = new TextTag
                                            {
                                                id = string.Format("{0}:{1}:{2}", networkController.credentials.name, DateTimeFactory.Now().Ticks, 1),
                                                author = networkController.credentials.name,
                                                privacy = shapePrivacy
                                            };
                                            new List<KeyValuePair<string, string>> {
                                            new KeyValuePair<string, string>(MeTLStanzas.TextBox.textTag,textFrame.TextRange.Text.Replace('\v','\n')),
                                            new KeyValuePair<string, string>(MeTLStanzas.authorTag,networkController.credentials.name),
                                            new KeyValuePair<string, string>(MeTLStanzas.targetTag,target),
                                            new KeyValuePair<string, string>(MeTLStanzas.privacyTag,shapePrivacy.ToString()),
                                            new KeyValuePair<string, string>(MeTLStanzas.slideTag,slideJid.ToString()),
                                            new KeyValuePair<string, string>(MeTLStanzas.identityTag,tag.id),
                                            new KeyValuePair<string, string>(MeTLStanzas.tagTag,JsonConvert.SerializeObject(shapeTag)),
                                            new KeyValuePair<string, string>(MeTLStanzas.TextBox.widthTag,shape.Width.ToString()),
                                            new KeyValuePair<string, string>(MeTLStanzas.TextBox.heightTag,shape.Height.ToString()),
                                            new KeyValuePair<string, string>(MeTLStanzas.xTag,shapeX.ToString()),
                                            new KeyValuePair<string, string>(MeTLStanzas.yTag,shapeY.ToString()),
                                            new KeyValuePair<string,string>(MeTLStanzas.TextBox.familyTag,safeFont),
                                            new KeyValuePair<string,string>(MeTLStanzas.TextBox.sizeTag,(textFrame.TextRange.Font.Size * fontSizeFactor).ToString()),
                                            new KeyValuePair<string,string>(MeTLStanzas.TextBox.colorTag,safeColour),
                                            new KeyValuePair<string,string>(MeTLStanzas.TextBox.decorationTag, "None"),
                                            new KeyValuePair<string,string>(MeTLStanzas.TextBox.weightTag, "Normal"),
                                            new KeyValuePair<string,string>(MeTLStanzas.TextBox.styleTag, "Normal")

                                        }.ForEach(kvp =>
                                        {
                                            textElem.Add(new XElement(kvp.Key, kvp.Value));
                                        });
                                            history.Add(message(textElem));
                                        }
                                    }
                                    else
                                    {
                                        var shapeFile = currentWorkingDirectory + "background" + (++resource).ToString() + ".jpg";
                                        shape.Export(shapeFile, PpShapeFormat.ppShapeFormatJPG, backgroundWidth, backgroundHeight, PpExportMode.ppRelativeToSlide);

                                        var imageElem = new XElement("image");
                                        var thisImageIdentity = string.Format("{0}:{1}:{2}", networkController.credentials.name, DateTimeFactory.Now().Ticks, 1);
                                        var shapeTag = new ImageTag
                                        {
                                            id = thisImageIdentity,
                                            author = networkController.credentials.name,
                                            privacy = shapePrivacy,
                                            isBackground = false,
                                            resourceIdentity = thisImageIdentity,
                                            zIndex = z++ 
                                        };
                                        new List<KeyValuePair<string, string>> {
                                            new KeyValuePair<string, string>("imageBytes",System.Convert.ToBase64String(File.ReadAllBytes(shapeFile))),
                                            new KeyValuePair<string, string>(MeTLStanzas.authorTag,networkController.credentials.name),
                                            new KeyValuePair<string, string>(MeTLStanzas.targetTag,GlobalConstants.PRESENTATIONSPACE),
                                            new KeyValuePair<string, string>(MeTLStanzas.privacyTag,shapePrivacy.ToString()),
                                            new KeyValuePair<string, string>(MeTLStanzas.slideTag,slideJid.ToString()),
                                            new KeyValuePair<string, string>(MeTLStanzas.identityTag,tempFile),
                                            new KeyValuePair<string, string>(MeTLStanzas.tagTag,JsonConvert.SerializeObject(shapeTag)),
                                            new KeyValuePair<string, string>(MeTLStanzas.Image.widthTag,shape.Width.ToString()),
                                            new KeyValuePair<string, string>(MeTLStanzas.Image.heightTag,shape.Height.ToString()),
                                            new KeyValuePair<string, string>(MeTLStanzas.xTag,shape.Left.ToString()),
                                            new KeyValuePair<string, string>(MeTLStanzas.yTag,shape.Top.ToString())
                                        }.ForEach(kvp =>
                                        {
                                            imageElem.Add(new XElement(kvp.Key, kvp.Value));
                                        });
                                        File.Delete(shapeFile);
                                        history.Add(message(imageElem));
                                    }
                                }
                                catch (Exception exc)
                                {

                                }
                                onProgress(SLIDE, "", slideCount++, slideTotal);
                            }
                        }
                        histories.Add(history);
                    }
                    catch (Exception ex)
                    {
                        //not yet sure what to do here
                    }
                }
                onProgress(LOCAL, "constructing server request", localCount++, localTotal);
                convXml.Add(conversation.WriteXml());
                convXml.Add(histories);
                onProgress(LOCAL, "sending request", localCount++, localTotal);

                var url = networkController.config.importConversation();
                onProgress(SERVER, "ready to send to server", 0, localTotal);
                onProgress(SERVER, "sending to server", 1, 2);
                var remoteConvString = networkController.client.resourceProvider.securePutData(url, System.Text.Encoding.UTF8.GetBytes(convXml.ToString()));
                onProgress(SERVER, "server response available", 2, 2);
                onProgress(LOCAL, "parsing server response", localCount++, localTotal);
                var removeConvXml = XElement.Parse(remoteConvString);
                onProgress(LOCAL, "parsing conversation", localCount++, localTotal);
                var remoteConv = ConversationDetails.ReadXml(removeConvXml);
                onProgress(LOCAL, "remote conversation ready", localCount++, localTotal);
                onProgress(TOTAL, "remote conversation ready", totalCount++, totalTotal);
                onComplete(remoteConv);
                return remoteConv;
            }
            catch (Exception e)
            {
                return LoadPowerpointFromServer(file, onProgress, onComplete, totalCount, totalTotal);
            }
            finally
            {
                if (ppt != null)
                    ppt.Close();
            }
        }

        private bool HasExportableText(PowerPoint.Shape shape)
        {
            return shape.HasTextFrame == TRUE && shape.TextFrame.HasText == TRUE && !String.IsNullOrEmpty(shape.TextFrame.TextRange.Text);
        }
    }
}