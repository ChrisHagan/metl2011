using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Sandpit;
using SandRibbon.Components.Submissions;
using SandRibbon.Components.Utility;
using SandRibbon.Quizzing;
using SandRibbon.Utils.Connection;
using SandRibbonInterop;
using SandRibbonObjects;
using SandRibbon.Utils;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using SandRibbonInterop.MeTLStanzas;
using SandRibbon.Providers;
using SandRibbon.Components.Canvas;

namespace SandRibbon.Components
{
    public partial class PresentationSpace
    {
        private bool synced = false;
        private bool joiningConversation;
        public PresentationSpace()
        {
            privacyOverlay = new SolidColorBrush { Color = Colors.Red, Opacity = 0.2 };
            InitializeComponent();
            Commands.InitiateDig.RegisterCommand(new DelegateCommand<object>(InitiateDig));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>(MoveTo));
            Commands.ReceiveLiveWindow.RegisterCommand(new DelegateCommand<LiveWindowSetup>(ReceiveLiveWindow));
            Commands.MirrorPresentationSpace.RegisterCommand(new DelegateCommand<Window1>(MirrorPresentationSpace, CanMirrorPresentationSpace));
            Commands.PreParserAvailable.RegisterCommand(new DelegateCommand<PreParser>(PreParserAvailable));
            Commands.CreateThumbnail.RegisterCommand(new DelegateCommand<int>(CreateThumbnail));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
            Commands.ConvertPresentationSpaceToQuiz.RegisterCommand(new DelegateCommand<int>(ConvertPresentationSpaceToQuiz));
            Commands.SyncedMoveRequested.RegisterCommand(new DelegateCommand<int>(setUpSyncDisplay));
            Commands.ExploreBubble.RegisterCommand(new DelegateCommand<ThoughtBubble>(exploreBubble));
            Commands.InitiateGrabZoom.RegisterCommand(new DelegateCommand<object>(InitiateGrabZoom));
            Commands.Highlight.RegisterCommand(new DelegateCommand<HighlightParameters>(highlight));
            Commands.RemoveHighlight.RegisterCommand(new DelegateCommand<HighlightParameters>(removeHighlight));
            Commands.GenerateScreenshot.RegisterCommand(new DelegateCommand<ScreenshotDetails>(SendScreenShot));
        }
        private void exploreBubble(ThoughtBubble thoughtBubble)
        {
            var origin = new Point(0, 0);
            var marquee = new Rectangle();
            marquee.Width = this.ActualWidth;
            marquee.Height = this.ActualHeight;

            var setup = new LiveWindowSetup
                {
                    frame = marquee,
                    origin = origin,
                    target = new Point(0, 0),
                    snapshotAtTimeOfCreation = ResourceUploader.uploadResourceToPath(
                        toByteArray(this, marquee, origin),
                        "Resource/" + Globals.location.currentSlide.ToString(),
                        "quizSnapshot.png",
                        false),
                    author = Globals.me,
                    slide = Globals.location.currentSlide,
                    visualSource = stack
                };

            var view = new Rect(setup.origin, new Size(setup.frame.Width, setup.frame.Height));
            var liveWindow = new Rectangle
            {
                Width = setup.frame.Width,
                Height = setup.frame.Height,
                Fill = new VisualBrush
                {
                    Visual = setup.visualSource,
                    TileMode = TileMode.None,
                    Stretch = Stretch.None,
                    AlignmentX = AlignmentX.Left,
                    AlignmentY = AlignmentY.Top,
                    ViewboxUnits = BrushMappingMode.Absolute,
                    Viewbox = view
                },
                Tag = setup.snapshotAtTimeOfCreation
            };
            Commands.ThoughtLiveWindow.Execute(new ThoughtBubbleLiveWindow
                                                   {
                                                       LiveWindow = liveWindow,
                                                       Bubble = thoughtBubble
                                                   });
        }
        private void setUpSyncDisplay(int slide)
        {
            if (!Globals.synched) return;
            try
            {
                if (Globals.conversationDetails.Author == Globals.me) return;
                if (Globals.conversationDetails.Slides.Where(s => s.id.Equals(slide)).Count() == 0) return;
                Dispatcher.adoptAsync((Action)delegate
                            {
                                var adorner = GetAdorner();
                                AdornerLayer.GetAdornerLayer(adorner).Add(new UIAdorner(adorner, new SyncDisplay()));
                            });
            }
            catch (NotSetException e)
            {
                //BOOOO
            }
        }
        private void UpdateConversationDetails(ConversationDetails details)
        {
            joiningConversation = false;
            try
            {
                if (Globals.conversationDetails.Author == Globals.me || Globals.conversationDetails.Permissions.studentCanPublish)
                    Commands.SetPrivacy.Execute(stack.handwriting.actualPrivacy);
                else
                    Commands.SetPrivacy.Execute("private");
            }
            catch (NotSetException)
            {
                Commands.SetPrivacy.Execute("private");
            }
        }
        public FrameworkElement GetAdorner()
        {
            var element = (FrameworkElement)this;
            while (element.Parent != null && !(element.Name == "adornerGrid"))
                element = (FrameworkElement)element.Parent;

            foreach (var child in ((Grid)element).Children)
                if (((FrameworkElement)child).Name == "adorner")
                    return (FrameworkElement)child;
            return null;
        }
        private void SendScreenShot(ScreenshotDetails details)
        {
            string hostedFileName = generateScreenshot(details);
            if (hostedFileName == "failed")
            {
                MessageBox.Show("cannot upload submission");
                return;
            }
            Commands.ScreenshotGenerated.Execute(hostedFileName);
       
        }

        private string generateScreenshot(ScreenshotDetails details)
        {
            var dpi = 96;
            var size = 1024;
            var bitmap = new RenderTargetBitmap(size, size, dpi, dpi, PixelFormats.Default);
            var dv = new DrawingVisual();
            using (var context = dv.RenderOpen())
            {
                context.DrawRectangle(new VisualBrush(stack), null,
                                      new Rect(new Point(), new Size(size, size)));
                context.DrawText(new FormattedText(
                                     details.message,
                                     CultureInfo.GetCultureInfo("en-us"),
                                     FlowDirection.LeftToRight,
                                     new Typeface("Arial"),
                                     24,
                                     Brushes.Black
                                     ),
                                 new Point(5, 10));
            }
            bitmap.Render(dv);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            var file = string.Format("{0}submission.png", Globals.me);
            using (Stream stream = File.Create(file))
            {
                encoder.Save(stream);
            }

            var hostedFileName = ResourceUploader.uploadResource(Globals.slide.ToString(), file);

            return hostedFileName;
        }

        private void CreateThumbnail(int id)
        {
            try
            {
                var bitmap = generateCapture(512);
                Commands.ThumbnailGenerated.Execute(new UnscaledThumbnailData { id = Globals.location.currentSlide, data = bitmap });
            }
            catch (OverflowException)
            {
                //The image is too large to thumbnail.  Just leave it be.
            }
        }
        private Rect measureToAspect(double width, double height, double max)
        {
            var dominantSide = height > width ? height : width;
            var scalingFactor = max / dominantSide;
            return new Rect(0, 0, width * scalingFactor, height * scalingFactor);
        }
        private RenderTargetBitmap generateCapture(int side)
        {
            var dpi = 96;
            var dimensions = measureToAspect(ActualWidth, ActualHeight, side);
            var bitmap = new RenderTargetBitmap((int)dimensions.Width, (int)dimensions.Height, dpi, dpi, PixelFormats.Default);
            var dv = new DrawingVisual();
            using (var context = dv.RenderOpen())
                context.DrawRectangle(new VisualBrush(stack), null, dimensions);
            bitmap.Render(dv);
            return bitmap;
        }
        private void PreParserAvailable(PreParser parser)
        {
            stack.handwriting.ReceiveStrokes(parser.ink);
            stack.images.ReceiveImages(parser.images.Values);
            foreach (var text in parser.text.Values)
                stack.text.doText(text);
            foreach (var video in parser.videos)
            {
                var srVideo = ((TargettedVideo)video.Value).video;
                srVideo.VideoWidth = srVideo.MediaElement.NaturalVideoWidth;
                srVideo.VideoHeight = srVideo.MediaElement.NaturalVideoHeight;
                srVideo.MediaElement.LoadedBehavior = MediaState.Manual;
                srVideo.MediaElement.ScrubbingEnabled = true;
                stack.images.AddVideo(srVideo);
            } foreach (var bubble in parser.bubbleList)
                stack.ReceiveNewBubble(bubble);
            Worm.heart.Interval = TimeSpan.FromMilliseconds(1500);
            if (parser.location.currentSlide == Globals.location.currentSlide)
                if (snapshotTimer == null)
                    snapshotTimer = new Timer(delegate {
                        Dispatcher.adoptAsync(delegate { snapshot(); }); 
                    }, null, 600, Timeout.Infinite);
                else snapshotTimer.Change(600, Timeout.Infinite);
        }
        private Timer snapshotTimer;
        private void snapshot()
        {
            foreach (AbstractCanvas ac in stack.canvasStack.Children)
            {
                ac.hidePrivateContent();
            }
            stack.UpdateLayout();
            Commands.CreateThumbnail.Execute(Globals.slide);
            foreach (AbstractCanvas ac in stack.canvasStack.Children)
            {
                ac.showPrivateContent();
            }
        }
        private void MirrorPresentationSpace(Window1 parent)
        {
            try
            {
                var currentAttributes = stack.handwriting.DefaultDrawingAttributes;
                var mirror = new Window { Content = new Projector { viewConstraint = parent.scroll } };
                Projector.Window = mirror;
                parent.Closed += (_sender, _args) => mirror.Close();
                mirror.WindowStyle = WindowStyle.None;
                mirror.AllowsTransparency = true;
                setSecondaryWindowBounds(mirror);
                mirror.Show();
                Commands.SetDrawingAttributes.Execute(currentAttributes);
                Commands.SetPrivacy.Execute(stack.handwriting.privacy);
            }
            catch (NotSetException)
            {
                //Fine it's not time yet anyway.  I don't care.
            }
        }
        private static bool CanMirrorPresentationSpace(object _param)
        {
            return Projector.Window == null && System.Windows.Forms.Screen.AllScreens.Length > 1;
        }
        private static System.Windows.Forms.Screen getSecondaryScreen()
        {
            foreach (System.Windows.Forms.Screen s in System.Windows.Forms.Screen.AllScreens)
            {
                if (!s.Primary)
                    return s;
            }
            return System.Windows.Forms.Screen.PrimaryScreen;
        }
        public System.Drawing.Rectangle getSecondaryScreenBounds()
        {
            System.Windows.Forms.Screen s = getSecondaryScreen();
            return s.Bounds;
        }
        private void setSecondaryWindowBounds(Window w)
        {
            System.Drawing.Rectangle r = getSecondaryScreenBounds();
            w.Top = r.Top;
            w.Left = r.Left;
            w.Width = r.Width;
            w.Height = r.Height;
        }
        private void MoveTo(int slide)
        {
            ClearAdorners();
            stack.Flush();
        }
        private void ClearAdorners()
        {
            Dispatcher.adoptAsync(delegate
                          {
                              removeAdornerItems(this);
                              ClearPrivacy();
                              removeSyncDisplay();
                          });

        }
        private void removeSyncDisplay()
        {
            var adorner = GetAdorner();
            var adornerLayer = AdornerLayer.GetAdornerLayer(adorner);
            var adorners = adornerLayer.GetAdorners(adorner);
            if (adorners != null)
                foreach (var element in adorners)
                    if (element is UIAdorner)
                        if (((UIAdorner)element).contentType.Name.Equals("SyncDisplay"))
                            adornerLayer.Remove(element);
        }

        private static void removeAdornerItems(UIElement element)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(element);
            var adorners = adornerLayer.GetAdorners(element);
            if (adorners != null)
                foreach (var adorner in adorners)
                    adornerLayer.Remove(adorner);
        }
        private void ReceiveLiveWindow(LiveWindowSetup window)
        {
            if (window.slide != Globals.slide || window.author != Globals.me) return;
            window.visualSource = stack;
            Commands.DugPublicSpace.Execute(window);
        }
        private void InitiateDig(object _param)
        {
            withDragMarquee(SendNewDig);
        }
        private void InitiateGrabZoom(object _param)
        {
            withDragMarquee(marquee =>
            {
                Commands.SetZoomRect.Execute(marquee);
            });
        }
        private void withDragMarquee(Action<Rectangle> doWithRect)
        {
            var canvas = new System.Windows.Controls.Canvas();
            var adornerLayer = AdornerLayer.GetAdornerLayer(this);
            var adorners = adornerLayer.GetAdorners(this);
            if (adorners != null)
                foreach (var a in adorners)
                    if (a is UIAdorner && ((UIAdorner)a).contentType == typeof(System.Windows.Controls.Canvas))
                        return;
            var adorner = new UIAdorner(this, canvas);
            var marquee = new Rectangle { Fill = Brushes.Purple, Opacity = 0.4 };
            canvas.Background = new SolidColorBrush { Color = Colors.Wheat, Opacity = 0.1 };
            canvas.Children.Add(marquee);
            bool mouseDown = false;
            canvas.MouseDown += (sender, e) =>
            {
                var pos = e.GetPosition(canvas);
                System.Windows.Controls.Canvas.SetLeft(marquee, pos.X);
                System.Windows.Controls.Canvas.SetTop(marquee, pos.Y);
                mouseDown = true;
            };
            canvas.MouseUp += (sender, e) =>
            {
                mouseDown = false;
                var pos = e.GetPosition(this);
                var origin = new Point(pos.X - marquee.Width, pos.Y - marquee.Height);
                doWithRect(marquee);
                adornerLayer.Remove(adorner);
            };
            canvas.MouseMove += (sender, e) =>
            {
                if (!mouseDown) return;
                var pos = e.GetPosition(canvas);
                var prevX = System.Windows.Controls.Canvas.GetLeft(marquee);
                var prevY = System.Windows.Controls.Canvas.GetTop(marquee);
                marquee.Width = Math.Max(prevX, pos.X) - Math.Min(prevX, pos.X);
                marquee.Height = Math.Max(prevY, pos.Y) - Math.Min(prevY, pos.Y);
            };
            canvas.MouseLeave += (_sender, _args) => adornerLayer.Remove(adorner);
            adornerLayer.Add(adorner);
        }
        private void SendNewDig(Rectangle marquee)
        {
            var origin = new Point(
                System.Windows.Controls.Canvas.GetLeft(marquee),
                System.Windows.Controls.Canvas.GetTop(marquee));
            Commands.SendLiveWindow.Execute(new LiveWindowSetup
                                    {
                                        frame = marquee,
                                        origin = origin,
                                        target = new Point(0, 0),
                                        snapshotAtTimeOfCreation = ResourceUploader.uploadResourceToPath(
                                            toByteArray(this, marquee, origin),
                                            "Resource/" + Globals.slide.ToString(),
                                            "quizSnapshot.png",
                                            false),
                                        author = Globals.me,
                                        slide = Globals.slide
                                    });
        }
        private static byte[] toByteArray(Visual adornee, FrameworkElement marquee, Point origin)
        {
            try
            {
                const double dpi = 96;
                var encoder = new PngBitmapEncoder();
                var dv = new DrawingVisual();
                var rtb = new RenderTargetBitmap(
                    (Int32)marquee.ActualWidth,
                    (Int32)marquee.ActualHeight,
                    dpi,
                    dpi,
                    PixelFormats.Default);
                using (DrawingContext dc = dv.RenderOpen())
                {
                    var vb = new VisualBrush(adornee)
                                 {
                                     Viewbox = new Rect(origin, new Size(marquee.ActualWidth, marquee.ActualHeight))
                                 };
                    dc.DrawRectangle(vb, null, new Rect(origin, new Point(marquee.ActualWidth, marquee.ActualHeight)));
                }
                rtb.Render(dv);

                byte[] buffer;
                using (var ms = new MemoryStream())
                {
                    encoder.Frames.Add(BitmapFrame.Create(rtb));
                    encoder.Save(ms);
                    buffer = new Byte[ms.Length];
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.Read(buffer, 0, buffer.Length);
                }
                return buffer;
            }
            catch (Exception)
            {
                return new byte[0];
            }
        }
        private void receiveQuiz(QuizQuestion details)
        {

        }
        private System.Windows.Shapes.Path privacyAdorner = new System.Windows.Shapes.Path();
        public Brush privacyOverlay;
        public void ClearPrivacy()
        {
            var geometry = new PathGeometry();
            privacyAdorner = new System.Windows.Shapes.Path
            {
                Fill = privacyOverlay,
                Data = geometry
            };
            geometry.FillRule = FillRule.Nonzero;
            var adornerLayer = AdornerLayer.GetAdornerLayer(this);
            if (adornerLayer == null) return;
            privacyAdorner.IsHitTestVisible = false;
            adornerLayer.Add(new UIAdorner(this, privacyAdorner));
        }

        private void removeHighlight(HighlightParameters param)
        {
            RemovePrivateRegion(param.verticies);
        }
        private void highlight(HighlightParameters param)
        {
            AddPrivateRegion(param.verticies, param.color);
        }
        public void AddPrivateRegion(IEnumerable<Point> vertices, Color color)
        {
            try
            {
                if (Globals.pedagogy.code < 3) return;
                privacyOverlay = new SolidColorBrush { Color = color, Opacity = 0.2 };
                privacyAdorner.Fill = privacyOverlay;
                RemovePrivateRegion(vertices);
                var segments = vertices.Select(v => (PathSegment)new LineSegment(v, true));
                if (privacyAdorner.Data == null) return;
                ((PathGeometry)privacyAdorner.Data).Figures.Add(new PathFigure(vertices.First(), segments, true));
            }
            catch (NotSetException e)
            {
            }
        }
        public void AddPrivateRegion(IEnumerable<Point> vertices)
        {
            AddPrivateRegion(vertices, Colors.Red);
        }
        public void RemovePrivateRegion(IEnumerable<Point> vertices)
        {
            if (vertices == null) return;
            var sum = vertices.Aggregate(0.0, (acc, v) => acc + v.X + v.Y);
            var geometry = (PathGeometry)privacyAdorner.Data;
            if (geometry == null) return;
            var regionToRemove = geometry.Figures.Where(
                f =>
                {
                    var figureSum = f.Segments.Select(s => ((LineSegment)s).Point).Aggregate(0.0, (acc, p) => acc + p.X + p.Y);
                    return Math.Abs(figureSum - sum) < 1;
                }).ToList();
            foreach (var region in regionToRemove)
                geometry.Figures.Remove(region);
        }
        public void ConvertPresentationSpaceToQuiz(int options)
        {
            const string path = "quizSample.png";
            var width = (int)ActualWidth;
            var height = (int)ActualHeight;
            new PrintingHost().saveCanvasToDisk(
                this,
                path,
                width,
                height,
                width,
                height);
            var hostedFileName = ResourceUploader.uploadResource(Globals.me, path);
            var location = Globals.location;

        }
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new PresentationSpaceAutomationPeer(this);
        }
    }
    public class PresentationSpaceAutomationPeer : FrameworkElementAutomationPeer, IValueProvider
    {
        public PresentationSpaceAutomationPeer(FrameworkElement parent) : base(parent) { }
        public PresentationSpace PresentationSpace
        {
            get { return (PresentationSpace)base.Owner; }
        }
        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Value)
                return this;
            return base.GetPattern(patternInterface);
        }
        public bool IsReadOnly
        {
            get { return false; }
        }
        public string Value
        {
            get { return Permissions.InferredTypeOf(Globals.conversationDetails.Permissions).Label; }
        }
        public void SetValue(string value)
        {
            throw new NotImplementedException();
        }
    }
    public class UnscaledThumbnailData
    {
        public int id;
        public RenderTargetBitmap data;
    }
    public class HighlightParameters
    {
        public IEnumerable<Point> verticies;
        public Color color;
    }
}
