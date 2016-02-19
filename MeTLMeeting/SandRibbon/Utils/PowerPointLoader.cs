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
    public class ByteArrayValueComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] x, byte[] y)
        {
            var length = x.Count();
            if (length != y.Count())
            {
                return false;
            }
            for (int i = 0; i < length; i++)
            {
                if (x[i] != y[i])
                    return false;
            }
            return true;
        }

        public int GetHashCode(byte[] obj)
        {
            return obj.Sum(b => (int)b);
        }
    }
    public class WebThreadPool
    {
        private static Amib.Threading.SmartThreadPool pool = new Amib.Threading.SmartThreadPool
        {
            MaxThreads = 8
        };
        public static void QueueUserWorkItem(Amib.Threading.Action action)
        {
            pool.QueueWorkItem(action);
        }
    }
    public class SneakyImage : MeTLStanzas.Image
    {
        public SneakyImage(String target, ImageTag tag, string src, double x, double y, int slide) : base()
        {
            SetTag(MeTLStanzas.tagTag, JsonConvert.SerializeObject(tag));
            SetTag(sourceTag, src);
            SetTag(MeTLStanzas.xTag, x);
            SetTag(MeTLStanzas.yTag, y);
            SetTag(MeTLStanzas.authorTag, tag.author);
            SetTag(MeTLStanzas.targetTag, target);
            SetTag(MeTLStanzas.privacyTag, tag.privacy.ToString());
            SetTag(MeTLStanzas.slideTag, slide);
            SetTag(MeTLStanzas.identityTag, tag.id);
        }
        public void SetWidth(double width)
        {
            SetTag(widthTag, width);
        }
        public void SetHeight(double height)
        {
            SetTag(heightTag, height);
        }
    }
    public class SneakyText : MeTLStanzas.TextBox
    {
        public SneakyText(String target, String text, String family, double size,
            String color, TextTag tag, double x, double y, int slide)
        {
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
        public void SetWidth(double width)
        {
            SetTag(widthTag, width);
        }
        public void SetHeight(double height)
        {
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
    public class PowerPointLoader
    {
        private const MsoTriState FALSE = MsoTriState.msoFalse;
        private const MsoTriState TRUE = MsoTriState.msoTrue;
        private int resource = 1;
        private MeTLLib.IClientBehaviour clientConnection;
        public enum PowerpointImportType
        {
            HighDefImage,
            Image,
            Shapes
        }
        public PowerPointLoader(IClientBehaviour client)
        {
            clientConnection = client;
        }
        private class PowerpointLoadTracker
        {
            private int slidesUploaded = 0;
            private readonly int target;
            private Action onComplete;
            public PowerpointLoadTracker(int _target, Action onCompleteAction)
            {
                target = _target;
                onComplete = onCompleteAction;
            }
            public void increment()
            {
                Interlocked.Increment(ref slidesUploaded);
                if (slidesUploaded == target)
                {
                    onComplete();
                }
            }
        }
        public ConversationDescriptor GetFlexibleDescriptor(string filename, Action<PowerpointImportProgress> progress)
        {
            var convDescriptor = new ConversationDescriptor(filename);
            return withPowerpoint((app) => LoadPowerpoint(app, filename, convDescriptor,progress));
        }
        public ConversationDescriptor GetFlatDescriptor(string filename,int magnification, Action<PowerpointImportProgress> progress)
        {
            var convDescriptor = new ConversationDescriptor(filename);
            return withPowerpoint((app) => LoadPowerpointAsFlatSlides(app, filename, magnification, convDescriptor,progress));
        }
        protected T withPowerpoint<T>(Func<PowerPoint.Application,T> func)
        {
            if (IsPowerPointRunning())
            {
                MeTLMessage.Information("PowerPoint seems to be running.  Please do not work in PowerPoint while the conversation is importing - doing so may result in unexpected issues with the import into MeTL.");
            }
            var app = GetPowerPointApplication();
            if (app == null)
                return default(T);
            return func(app);
        }
        private void UploadPowerpoint(PowerpointSpec spec)
        {
            System.Windows.Application.Current.Dispatcher.adopt(delegate
            {
                if (IsPowerPointRunning())
                {
                    MeTLMessage.Information("PowerPoint seems to be running.  Please do not work in PowerPoint while the conversation is importing - doing so may result in unexpected issues with the import into MeTL.");
                    //                    return;
                }

                var app = GetPowerPointApplication();
                if (app == null)
                    return;
                Action<PowerpointImportProgress> progress = (PowerpointImportProgress p) => Commands.UpdatePowerpointProgress.Execute(p);
                var conversation = spec.Details;
                var convDescriptor = new ConversationDescriptor(spec.Details.Title);
                var worker = new Thread(new ParameterizedThreadStart(
                    delegate
                    {
                        try
                        {
                            progress(new PowerpointImportProgress(PowerpointImportProgress.IMPORT_STAGE.DESCRIBED, 0, 0));
                            switch (spec.Type)
                            {
                                case PowerpointImportType.HighDefImage:
                                    App.auditor.log(string.Format("ImportingPowerpoint HighDef {0}", spec.File), "PowerPointLoader");
                                    UploadFromXml(LoadPowerpointAsFlatSlides(app, spec.File, spec.Magnification, convDescriptor,progress), conversation,progress);
                                    break;
                                case PowerpointImportType.Image:
                                    App.auditor.log(string.Format("ImportingPowerpoint NormalDef {0}", spec.File), "PowerPointLoader");
                                    UploadFromXml(LoadPowerpointAsFlatSlides(app, spec.File, spec.Magnification, convDescriptor,progress), conversation,progress);
                                    break;
                                case PowerpointImportType.Shapes:
                                    App.auditor.log(string.Format("ImportingPowerpoint Flexible {0}", spec.File), "PowerPointLoader");
                                    UploadFromXml(LoadPowerpoint(app, spec.File, convDescriptor,progress), conversation,progress);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }));
                worker.SetApartmentState(ApartmentState.STA);
                worker.Start();
            });
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
            catch (Exception e)
            {
                App.auditor.log(String.Format("Exception during powerpoint load: {0}", e.Message));
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

        public ConversationDescriptor LoadPowerpointAsFlatSlides(PowerPoint.Application app, string file, int MagnificationRating, ConversationDescriptor convDescriptor,Action<PowerpointImportProgress> progress)
        {
            var ppt = app.Presentations.Open(file, TRUE, FALSE, FALSE);
            var currentWorkingDirectory = LocalFileProvider.getUserFolder("tmp");
            var backgroundWidth = ppt.SlideMaster.Width * MagnificationRating;
            var backgroundHeight = ppt.SlideMaster.Height * MagnificationRating;
            var uniqueDirectory = DateTime.Now.Ticks.ToString();
            try
            {
                foreach (Microsoft.Office.Interop.PowerPoint.Slide slide in ppt.Slides)
                {
                    var slidePath = getThumbnailPath(uniqueDirectory, slide.SlideID);
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
                    var xSlide = new SlideDescriptor(slide.SlideIndex - 1, backgroundWidth, backgroundHeight);
                    xSlide.images.Add(new ImageDescriptor(0, 0, backgroundWidth, backgroundHeight, Privacy.Public, "presentationSpace", File.ReadAllBytes(slidePath)));
                    convDescriptor.slides.Add(xSlide);
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
                    progress(new PowerpointImportProgress(PowerpointImportProgress.IMPORT_STAGE.EXTRACTED_IMAGES, slide.SlideIndex, ppt.Slides.Count));
                }
            }
            catch (COMException e)
            {
                App.auditor.error("COMException while loading FlatSlides", "PowerPointLoader", e);
            }
            finally
            {
                ppt.Close();
            }
            progress(new PowerpointImportProgress(PowerpointImportProgress.IMPORT_STAGE.ANALYSED, 0, 0));
            return convDescriptor;
        }
        private ByteArrayValueComparer byteArrayComparer = new ByteArrayValueComparer();        
        private void UploadFromXml(ConversationDescriptor conversationDescriptor, ConversationDetails details, Action<PowerpointImportProgress> progress)
        {
            var xmlSlides = conversationDescriptor.slides;
            var slideCount = xmlSlides.Count();
            if (details.Tag == null)
                details.Tag = "unTagged";
            details.Author = Globals.me;
            var created = clientConnection.CreateConversation(details);
            var startingId = created.Slides.First().id;
            created.Slides = conversationDescriptor.slides.Select(d => new MeTLLib.DataTypes.Slide(startingId++, Globals.me, MeTLLib.DataTypes.Slide.TYPE.SLIDE, d.index, (float)d.defaultWidth, (float)d.defaultHeight)).ToList();
            var updatedConversation = clientConnection.UpdateConversationDetails(created);
            if (!updatedConversation.ValueEquals(created))
            {
                App.auditor.log("PowerpointImport: Failed to update conversation", "PowerPointLoader");
            }
            var shapeMap = new Dictionary<byte[], string>();
            var me = Globals.me;
            int shapeCount = 0;
            var allBytes = conversationDescriptor.slides.SelectMany(slide => slide.images.Select(i => i.imageBytes));
            var bytesToUpload = allBytes.Distinct(byteArrayComparer);
            var uploadTracker = new PowerpointLoadTracker(bytesToUpload.Count(), () =>
            {
                var tracker = new PowerpointLoadTracker(slideCount, () => Commands.JoinConversation.Execute(updatedConversation));
                conversationDescriptor.slides.ForEach(slideXml =>
                {
                    var slideId = updatedConversation.Slides.Find(s => s.index == slideXml.index).id;
                    var slideIndex = slideXml.index;
                    WebThreadPool.QueueUserWorkItem(delegate
                    {
                        bool hasPrivate = conversationDescriptor.hasPrivateContent;
                        var privateRoom = string.Format("{0}{1}", slideId, Globals.me);
                        if (hasPrivate)
                            clientConnection.SneakInto(privateRoom);
                        clientConnection.SneakInto(slideId.ToString());
                        Thread.Sleep(1000);
                        sneakilySendShapes(slideId, slideXml.images, shapeMap);
                        sneakilySendPublicTextBoxes(slideId, slideXml.texts);
                        progress(new PowerpointImportProgress(PowerpointImportProgress.IMPORT_STAGE.UPLOADED_XML, slideIndex, slideCount));
                        tracker.increment();
                        Thread.Sleep(1000);
                        if (hasPrivate)
                            clientConnection.SneakOutOf(privateRoom);
                        clientConnection.SneakOutOf(slideId.ToString());
                    });
                });
                progress(new PowerpointImportProgress(PowerpointImportProgress.IMPORT_STAGE.FINISHED, 0, 0));
            });
            var imageUploadCounter = 0;
            foreach (var bytes in bytesToUpload)
            {
                WebThreadPool.QueueUserWorkItem(delegate
                {
                    var proposedId = me + ":" + DateTime.Now.Ticks.ToString() + ":" + shapeCount;
                    var hostedFileUriXml = clientConnection.resourceProvider.securePutData(App.controller.config.uploadResource(proposedId, updatedConversation.Jid), bytes);
                    var hostedFileUri = XDocument.Parse(hostedFileUriXml).Descendants("resourceUrl").First().Value;
                    MeTLLib.DataTypes.MeTLStanzas.ImmutableResourceCache.updateCache(App.controller.config.getResource(hostedFileUri), bytes);
                    shapeMap[bytes] = hostedFileUri;
                    progress(new PowerpointImportProgress(PowerpointImportProgress.IMPORT_STAGE.UPLOADED_RESOURCES, imageUploadCounter++ , bytesToUpload.Count()));
                    uploadTracker.increment();
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
            //var fullPath = "c:\\dev\\thumbs\\jid";
            var fullPath = LocalFileProvider.getUserFolder(new string[] { "thumbs", Globals.me, jid });
            return fullPath;
        }
        private void progress(PowerpointImportProgress.IMPORT_STAGE action, int currentSlideId, int totalNumberOfSlides)
        {
            Commands.UpdatePowerpointProgress.Execute(new PowerpointImportProgress(action, currentSlideId, totalNumberOfSlides));
        }
        public ConversationDescriptor LoadPowerpoint(PowerPoint.Application app, string file, ConversationDescriptor convDescriptor, Action<PowerpointImportProgress> progress)
        {
            try
            {
                var ppt = app.Presentations.Open(file, TRUE, FALSE, FALSE);
                try
                {
                    foreach (Microsoft.Office.Interop.PowerPoint.Slide slide in ppt.Slides)
                    {
                        importSlide(convDescriptor, (Microsoft.Office.Interop.PowerPoint.Slide)slide, progress);
                        progress(new PowerpointImportProgress(PowerpointImportProgress.IMPORT_STAGE.EXTRACTED_IMAGES, slide.SlideIndex, ppt.Slides.Count));//All the consumers count for themselves
                    }
                }
                catch (COMException e)
                {
                    App.auditor.error("COMException during LoadPowerpoint", "PowerPointLoader", e);
                }
                finally
                {
                    ppt.Close();
                }
            }
            catch (Exception e)
            {
                App.auditor.error("LoadPowerpoint", "PowerPointLoader", e);
            }
            progress(new PowerpointImportProgress(PowerpointImportProgress.IMPORT_STAGE.ANALYSED, 0, 0));
            return convDescriptor;
        }

        private void sneakilySendPublicTextBoxes(int id, IEnumerable<TextDescriptor> shapes)
        {
            int shapeCount = 0;
            foreach (var text in shapes)
            {
                var target = text.target;
                var content = text.content;
                var x = text.x;
                var y = text.y;
                var textBoxIdentity = DateTimeFactory.Now().ToString() + x + y + Globals.me + shapeCount++;
                var privacy = text.privacy;
                var family = text.fontFamily;
                var size = text.fontSize;
                var color = text.fontColor;
                var tag = new TextTag
                {
                    author = Globals.me,
                    id = textBoxIdentity,
                    privacy = privacy
                };
                var stanza = new SneakyText(target, content, family, size, color.ToString(), tag, x, y, id);
                stanza.SetHeight(text.h);
                stanza.SetWidth(text.w);
                clientConnection.SendStanza(id.ToString(), stanza);
            }
        }
        private void sneakilySendShapes(int id, IEnumerable<ImageDescriptor> shapes, Dictionary<byte[], string> shapeMap)
        {
            var me = Globals.me;
            var target = "presentationSpace";
            foreach (var shape in shapes)
            {

                var bytes = shape.imageBytes;
                var shapeId = shapeMap[shapeMap.Keys.ToList().Find(k => byteArrayComparer.Equals(bytes, k))];
                var x = shape.x;
                var y = shape.y;
                var width = shape.w;
                var height = shape.h;
                var privacy = shape.privacy;
                clientConnection.SendImage(new TargettedImage(id, me, target, privacy, shapeId, x, y, width, height, shapeId, -1L));
            }
        }
        private bool check(MsoTriState cond)
        {
            return cond == MsoTriState.msoTrue;
        }
        private void importSlide(ConversationDescriptor conversationDescriptor, Microsoft.Office.Interop.PowerPoint.Slide slide, Action<PowerpointImportProgress> progress)
        {
            var currentWorkingDirectory = LocalFileProvider.getUserFolder("tmp");
            var exportFormat = PpShapeFormat.ppShapeFormatPNG;
            var exportMode = PpExportMode.ppRelativeToSlide;
            var backgroundHeight = 540;
            var backgroundWidth = 720;
            var xSlide = new SlideDescriptor(slide.SlideIndex - 1, backgroundWidth, backgroundHeight);
            double Magnification = 1;
            try
            {
                if (backgroundHeight != Convert.ToInt32(slide.Master.Height))
                    backgroundHeight = Convert.ToInt32(slide.Master.Height);
                if (backgroundWidth != Convert.ToInt32(slide.Master.Width))
                    backgroundWidth = Convert.ToInt32(slide.Master.Width);
                var BackgroundFile = currentWorkingDirectory + "background" + (++resource) + ".jpg";
                foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in slide.Shapes)
                {
                    shape.Visible = MsoTriState.msoFalse;
                }
                slide.Export(BackgroundFile, "PNG", backgroundWidth, backgroundHeight);
                xSlide.images.Add(new ImageDescriptor(0, 0, backgroundWidth * Magnification, backgroundHeight * Magnification, Privacy.Public, "presentationSpace", File.ReadAllBytes(BackgroundFile)));
                File.Delete(BackgroundFile);
                foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in slide.Shapes)
                {
                    if (shape.Tags.Count > 0 && shape.Tags.Value(shape.Tags.Count) == "Instructor")
                        shape.Visible = MsoTriState.msoFalse;
                    else shape.Visible = MsoTriState.msoTrue;
                }
            }
            catch (Exception ex)
            {
                App.auditor.error("importSlide", "PowerPointLoader", ex);
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
            conversationDescriptor.slides.Add(xSlide);
        }

        private bool HasExportableText(PowerPoint.Shape shape)
        {
            return shape.HasTextFrame == TRUE && shape.TextFrame.HasText == TRUE && !String.IsNullOrEmpty(shape.TextFrame.TextRange.Text);
        }

        private void ExportShape(ConversationDescriptor conversationDescriptor, Microsoft.Office.Interop.PowerPoint.Shape shapeObj, SlideDescriptor xSlide, string currentWorkingDirectory, PpShapeFormat exportFormat,
            PpExportMode exportMode,
            int backgroundWidth,
            int backgroundHeight,
            double Magnification)
        {
            var shape = (Microsoft.Office.Interop.PowerPoint.Shape)shapeObj;
            var file = currentWorkingDirectory + "background" + (++resource).ToString() + ".jpg";
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
                        addPublicText(xSlide, shape, Privacy.Private, "presentationSpace", Magnification);
                    }
                    else
                    {
                        addShape(xSlide, shape, Privacy.Private, file, exportMode, backgroundWidth, backgroundHeight, Magnification);
                    }
                }
                catch (Exception ex)
                {
                    App.auditor.error("ExportShape", "PowerPointLoader", ex);
                }
            }
            else if (shape.Visible == MsoTriState.msoTrue)
            //if (shape.Tags.Count > 0 && shape.Tags.Value(shape.Tags.Count) == "_")
            {
                if (HasExportableText(shape))
                    addPublicText(xSlide, shape, Privacy.Public, "presentationSpace", Magnification);
                else
                {
                    addShape(xSlide, shape, Privacy.Public, file, exportMode, backgroundWidth, backgroundHeight, Magnification);
                }
            }
        }

        private void addShape(SlideDescriptor xSlide, PowerPoint.Shape shape, Privacy privacy, string file, PpExportMode exportMode, int backgroundWidth, int backgroundHeight, double Magnification)
        {
            try
            {
                var x = shape.Left;
                var y = shape.Top;
                shape.Export(file, PpShapeFormat.ppShapeFormatPNG, backgroundWidth, backgroundHeight, exportMode);
                xSlide.images.Add(new ImageDescriptor(x * Magnification, y * Magnification, shape.Width * Magnification, shape.Height * Magnification, privacy, "presentationSpace", File.ReadAllBytes(file)));
                File.Delete(file);
            }
            catch (Exception e)
            {
                App.auditor.error("addShape", "PowerPointLoader", e);
            }
        }

        private void addPublicText(SlideDescriptor xSlide, Microsoft.Office.Interop.PowerPoint.Shape shape, Privacy privacy, string target, double Magnification)
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
                var safeColour = (new Color { A = SystemDrawingColor.A, R = SystemDrawingColor.R, G = SystemDrawingColor.G, B = SystemDrawingColor.B });
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
                xSlide.texts.Add(new TextDescriptor(shapeX, shapeY, shape.Width * Magnification, shape.Height * Magnification, privacy, target, textFrame.TextRange.Text.Replace('\v', '\n'), safeFont, textFrame.TextRange.Font.Size * fontSizeFactor * Magnification, "", "", safeColour));
            }
        }

        private void addSpeakerNotes(ConversationDescriptor conversationDescriptor, SlideDescriptor xSlide, Microsoft.Office.Interop.PowerPoint.Shape placeholder, double Magnification)
        {
            addPublicText(xSlide, placeholder, Privacy.Private, "notepad", Magnification);
        }
    }
}