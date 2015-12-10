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
        private ConversationDetails UploadPowerpoint(PowerpointSpec spec)
        {
            //var conversation = networkController.client.CreateConversation(spec.Details);
            try
            {
                var healthy = true;
                if (IsPowerPointRunning())
                {
                    MeTLMessage.Error("Microsoft Powerpoint is already running.  Using serverside import instead.  This may result in some missing elements (wordart, clipped images, etc).  Please close Microsoft Powerpoint before importing presentations.");
                    healthy = false;
                }

                var app = GetPowerPointApplication();
                if (app == null)
                {
                    MeTLMessage.Error("MeTL prefers Microsoft PowerPoint to be installed to import a presentation.  Using serverside import instead.  This may result in some missing elements (wordart, clipped images, etc).  For more accurate import of Microsoft Powerpoint files, please install Microsoft Powerpoint.");
                    healthy = false;
                }
                switch (spec.Type)
                {
                    case PowerpointImportType.HighDefImage:
                        Trace.TraceInformation("ImportingPowerpoint HighDef {0}", spec.File);
                        return healthy ? LoadPowerpointAsFlatSlides(app, spec.File, spec.Magnification) : ConversationDetails.ReadXml(XElement.Parse(networkController.client.resourceProvider.securePutData(networkController.config.importPowerpoint(spec.File, spec.Magnification), File.ReadAllBytes(spec.File))));
                        break;
                    case PowerpointImportType.Image:
                        Trace.TraceInformation("ImportingPowerpoint NormalDef {0}", spec.File);
                        return healthy ? LoadPowerpointAsFlatSlides(app, spec.File, spec.Magnification) : ConversationDetails.ReadXml(XElement.Parse(networkController.client.resourceProvider.securePutData(networkController.config.importPowerpoint(spec.File, spec.Magnification), File.ReadAllBytes(spec.File))));
                        break;
                    case PowerpointImportType.Shapes:
                        Trace.TraceInformation("ImportingPowerpoint Flexible {0}", spec.File);
                        return healthy ? LoadPowerpoint(app, spec.File) : ConversationDetails.ReadXml(XElement.Parse(networkController.client.resourceProvider.securePutData(networkController.config.importPowerpointFlexible(spec.File), File.ReadAllBytes(spec.File))));
                        break;
                    default:
                        return ConversationDetails.Empty;
                    
                }
            }
            catch (Exception ex)
            {
                return ConversationDetails.Empty;
            }
        }

        public ConversationDetails ImportPowerpoint(PowerpointImportType importType)
        {
            var configDialog = new ConversationConfigurationDialog(networkController, ConversationConfigurationDialog.ConversationConfigurationMode.IMPORT);
            configDialog.ChooseFileForImport();

            var spec = configDialog.Import(importType);
            if (spec == null) return ConversationDetails.Empty;
            return UploadPowerpoint(spec);
        }

        private PowerPoint.Application GetPowerPointApplication()
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

        private bool IsPowerPointRunning()
        {
            var procList = Process.GetProcessesByName("POWERPNT");
            return procList.Count() != 0;
        }
        private XElement message(XElement inner)
        {
            var m = new XElement("message");
            m.Add(new XAttribute("timestamp", (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds.ToString()));
            m.Add(inner);
            return m;
        }
        public ConversationDetails LoadPowerpointAsFlatSlides(PowerPoint.Application app, string file, int MagnificationRating)
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
                ppt = app.Presentations.Open(file, TRUE, FALSE, FALSE);
                var backgroundWidth = ppt.SlideMaster.Width * MagnificationRating;
                var backgroundHeight = ppt.SlideMaster.Height * MagnificationRating;
                foreach (Microsoft.Office.Interop.PowerPoint.Slide slide in ppt.Slides)
                {
                    var slideJid = startingJid + slide.SlideIndex;
                    var tempFile = currentWorkingDirectory + "background" + (++resource).ToString() + ".jpg";
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
                    slide.Export(tempFile, "JPG", (int)backgroundWidth, (int)backgroundHeight);
                    var history = new XElement("history");
                    history.Add(new XAttribute("jid", slideJid.ToString()));
                    conversation.Slides.Add(new MeTLLib.DataTypes.Slide(
                        slideJid,
                        networkController.credentials.name,
                        MeTLLib.DataTypes.Slide.TYPE.SLIDE,
                        slide.SlideIndex - 1,
                        backgroundWidth,
                        backgroundHeight));
                    var imageElem = new XElement("image");
                    var tag = new ImageTag
                    {
                        id = string.Format("{0}:{1}:{2}", networkController.credentials.name, DateTimeFactory.Now().Ticks, 1),
                        author = networkController.credentials.name,
                        privacy = Privacy.Public,
                        isBackground = false
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
                    File.Delete(tempFile);
                    history.Add(message(imageElem));
                    histories.Add(history);
                }
                convXml.Add(conversation.WriteXml());
                convXml.Add(histories);

                var url = networkController.config.importConversation();
                var remoteConvString = networkController.client.resourceProvider.securePutData(url, System.Text.Encoding.UTF8.GetBytes(convXml.ToString()));
                return ConversationDetails.ReadXml(XElement.Parse(remoteConvString));
            }
            catch (COMException e)
            {
                //Commands.Mark.Execute(e.Message);
                var url = networkController.config.importPowerpoint(conversation.Title, MagnificationRating);
                var remoteConvString = networkController.client.resourceProvider.securePutData(url, File.ReadAllBytes(file));
                return ConversationDetails.ReadXml(XElement.Parse(remoteConvString));
            }
            finally
            {
                if (ppt != null)
                    ppt.Close();
            }
        }

        public ConversationDetails LoadPowerpoint(PowerPoint.Application app, string file)
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
                ppt = app.Presentations.Open(file, TRUE, FALSE, FALSE);
                var backgroundWidth = (int)ppt.SlideMaster.Width;
                var backgroundHeight = (int)ppt.SlideMaster.Height;
                foreach (Microsoft.Office.Interop.PowerPoint.Slide slide in ppt.Slides)
                {
                    try
                    {

                        var shapeCount = 0;
                        if (backgroundHeight != Convert.ToInt32(slide.Master.Height))
                            backgroundHeight = Convert.ToInt32(slide.Master.Height);
                        if (backgroundWidth != Convert.ToInt32(slide.Master.Width))
                            backgroundWidth = Convert.ToInt32(slide.Master.Width);
                        var slideJid = startingJid + slide.SlideIndex;
                        var tempFile = currentWorkingDirectory + "background" + (++resource).ToString() + ".jpg";
                        slide.Export(tempFile, "JPG", (int)backgroundWidth, (int)backgroundHeight);
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



                        var tag = new ImageTag
                        {
                            id = string.Format("{0}:{1}:{2}", networkController.credentials.name, DateTimeFactory.Now().Ticks, shapeCount++),
                            author = networkController.credentials.name,
                            privacy = Privacy.Public,
                            isBackground = false
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
                                        var shapeTag = new ImageTag
                                        {
                                            id = string.Format("{0}:{1}:{2}", networkController.credentials.name, DateTimeFactory.Now().Ticks, 1),
                                            author = networkController.credentials.name,
                                            privacy = shapePrivacy,
                                            isBackground = false
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
                            }
                        }
                        histories.Add(history);
                    }
                    catch (Exception ex)
                    {
                        //not yet sure what to do here
                    }
                }
                convXml.Add(conversation.WriteXml());
                convXml.Add(histories);

                var url = networkController.config.importConversation();
                var remoteConvString = networkController.client.resourceProvider.securePutData(url, System.Text.Encoding.UTF8.GetBytes(convXml.ToString()));
                return ConversationDetails.ReadXml(XElement.Parse(remoteConvString));
            }
            catch (Exception e)
            {
                var url = networkController.config.importPowerpointFlexible(conversation.Title);
                var remoteConvString = networkController.client.resourceProvider.securePutData(url, File.ReadAllBytes(file));
                return ConversationDetails.ReadXml(XElement.Parse(remoteConvString));
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