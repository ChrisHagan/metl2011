using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MeTLLib;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Sandpit;
using SandRibbon.Components.Submissions;
using SandRibbon.Components.Utility;
using SandRibbon.Quizzing;
using SandRibbon.Utils.Connection;
using SandRibbonInterop;
using SandRibbon.Utils;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using SandRibbon.Providers;
using SandRibbon.Components.Canvas;
using MeTLLib.DataTypes;

namespace SandRibbon.Components
{
    public partial class PresentationSpace
    {
        public PresentationSpace()
        {
            privacyOverlay = new SolidColorBrush { Color = Colors.Red, Opacity = 0.2 };
            privacyOverlay.Freeze();
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo, (sender, args) => Commands.Undo.Execute(null)));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Redo, (sender, args) => Commands.Redo.Execute(null)));
            Commands.InitiateDig.RegisterCommand(new DelegateCommand<object>(InitiateDig));
            Commands.MoveTo.RegisterCommandToDispatcher(new DelegateCommand<int>(MoveTo));
            Commands.ReceiveLiveWindow.RegisterCommand(new DelegateCommand<LiveWindowSetup>(ReceiveLiveWindow));
            Commands.MirrorPresentationSpace.RegisterCommandToDispatcher(new DelegateCommand<Window1>(MirrorPresentationSpace, CanMirrorPresentationSpace));
            Commands.PreParserAvailable.RegisterCommandToDispatcher(new DelegateCommand<MeTLLib.Providers.Connection.PreParser>(PreParserAvailable));
            Commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
            Commands.ConvertPresentationSpaceToQuiz.RegisterCommand(new DelegateCommand<int>(ConvertPresentationSpaceToQuiz));
            Commands.SyncedMoveRequested.RegisterCommand(new DelegateCommand<int>(setUpSyncDisplay));
            Commands.ExploreBubble.RegisterCommand(new DelegateCommand<ThoughtBubble>(exploreBubble));
            Commands.InitiateGrabZoom.RegisterCommand(new DelegateCommand<object>(InitiateGrabZoom));
            Commands.Highlight.RegisterCommand(new DelegateCommand<HighlightParameters>(highlight));
            Commands.RemoveHighlight.RegisterCommand(new DelegateCommand<HighlightParameters>(removeHighlight));
            Commands.GenerateScreenshot.RegisterCommand(new DelegateCommand<ScreenshotDetails>(SendScreenShot));
            Commands.BanhammerSelectedItems.RegisterCommand(new DelegateCommand<object>(BanHammerSelectedItems));
            Commands.VisualizeContent.RegisterCommandToDispatcher(new DelegateCommand<object>(VisualizeContent));
            Commands.AllStaticCommandsAreRegistered();
        }
        private void VisualizeContent(object obj) {
            var authors = new List<string>();
            authors.AddRange(stack.handwriting.getSelectedAuthors());
            authors.AddRange(stack.text.GetSelectedAuthors());
            authors.AddRange(stack.images.GetSelectedAuthors());
            authors.Add(Globals.me);
            var colors = ColorLookup.GetMediaColors();
            var distinctAuthors = authors.Distinct();
            var authorColor = new Dictionary<string, Color>();
            foreach (var author in distinctAuthors)
                authorColor.Add(author, colors.ElementAt(distinctAuthors.ToList().IndexOf(author)));
            foreach (var stroke in stack.handwriting.GetSelectedStrokes())
                stroke.DrawingAttributes.Color = authorColor[stroke.tag().author];
            foreach (var elem in stack.text.GetSelectedElements())
                ((MeTLTextBox)elem).Foreground = new SolidColorBrush(authorColor[((MeTLTextBox)elem).tag().author]);
            foreach (var elem in stack.images.GetSelectedElements())
                stack.images.applyShadowEffectTo((FrameworkElement)elem, authorColor[((System.Windows.Controls.Image)elem).tag().author]);
            new colorCodedUsers(authorColor).ShowDialog();
        }
        private void BanHammerSelectedItems(object obj)
        {
            var authors = new List<string>();
            authors.AddRange(stack.handwriting.getSelectedAuthors());
            authors.AddRange(stack.text.GetSelectedAuthors());
            authors.AddRange(stack.images.GetSelectedAuthors());
            var details = Globals.conversationDetails;
            foreach(var author in authors.Distinct())
            {
                if(!details.blacklist.Contains(author))
                    details.blacklist.Add(author);
            }
            ClientFactory.Connection().UpdateConversationDetails(details);
            Commands.SetPrivacyOfItems.Execute("private");
        }

        private void exploreBubble(ThoughtBubble thoughtBubble)
        {
            var origin = new Point(0, 0);
            var marquee = new Rectangle();
            marquee.Width = this.ActualWidth;
            marquee.Height = this.ActualHeight;

            var setup = new LiveWindowSetup(Globals.location.currentSlide, Globals.me,
                    stack,
                marquee, origin, new Point(0, 0),
                MeTLLib.ClientFactory.Connection().UploadResourceToPath(
                        toByteArray(this, marquee, origin),
                        "Resource/" + Globals.location.currentSlide.ToString(),
                        "quizSnapshot.png",
                        false).ToString());

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
            Commands.ThoughtLiveWindow.ExecuteAsync(new ThoughtBubbleLiveWindow
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
                //You are listening to the channel but have not yet joined the room
            }
        }
        private void UpdateConversationDetails(ConversationDetails details)
        {
            if (ConversationDetails.Empty.Equals(details)) return;
            if (details.Jid == "" || !(Globals.credentials.authorizedGroups.Select(s => s.groupKey).Contains(details.Subject)))
            {
                foreach (FrameworkElement child in stack.canvasStack.Children)
                {
                    if (child is AbstractCanvas)
                    {
                        ((AbstractCanvas)child).Strokes.Clear();
                        ((AbstractCanvas)child).Children.Clear();
                    }
                }
                return;
            }
            try
            {
                if (Globals.conversationDetails.Author == Globals.me || Globals.conversationDetails.Permissions.studentCanPublish)
                    Commands.SetPrivacy.ExecuteAsync("public");
                else
                    Commands.SetPrivacy.ExecuteAsync("private");
            }
            catch (NotSetException)
            {
                Commands.SetPrivacy.ExecuteAsync("private");
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
            Commands.ScreenshotGenerated.Execute(generateScreenshot(details));
        }
        private string generateScreenshot(ScreenshotDetails details)
        {
            var dpi = 96;
            var ratio = ActualWidth / ActualHeight;
            int targetWidth = 1024;
            int targetHeight = (int)(targetWidth / ratio);
            var file = "";
            Dispatcher.adopt(() =>
            {
                var bitmap = new RenderTargetBitmap(targetWidth ,targetHeight, dpi, dpi, PixelFormats.Default);
                var dv = new DrawingVisual();
                using (var context = dv.RenderOpen())
                {
                    var visual = details.showPrivate ? cloneAll() : clonePublicOnly();
                    context.DrawRectangle(new VisualBrush(visual), null,
                                          new Rect(new Point(), new Size(targetWidth, targetHeight)));
                    context.DrawText(new FormattedText(
                                            details.message, 
                                            CultureInfo.CurrentCulture, 
                                            FlowDirection.LeftToRight, 
                                            new Typeface(
                                                new FontFamily("Arial"),
                                                FontStyles.Normal,
                                                FontWeights.Normal,
                                                FontStretches.Normal),
                                            12,
                                            Brushes.Black),
                                        new Point(10, 10));
                }
                bitmap.Render(dv);
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                file = string.Format("{0}{1}submission.png", DateTime.Now.Ticks, Globals.me);
                using (Stream stream = File.Create(file))
                {
                    encoder.Save(stream);
                }
            });
            return file;
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
            RenderTargetBitmap bitmap = null;
            Dispatcher.adopt(() =>
            {
                bitmap = new RenderTargetBitmap((int)dimensions.Width, (int)dimensions.Height, dpi, dpi, PixelFormats.Default);
                var dv = new DrawingVisual();
                using (var context = dv.RenderOpen())
                    context.DrawRectangle(new VisualBrush(stack), null, dimensions);
                bitmap.Render(dv);
            });
            return bitmap;
        }
        private void PreParserAvailable(MeTLLib.Providers.Connection.PreParser parser)
        {
            BeginInit();
            stack.handwriting.ReceiveStrokes(parser.ink);
            stack.images.ReceiveImages(parser.images.Values);
            foreach (var text in parser.text.Values)
                stack.text.doText(text);
            foreach (var video in parser.videos)
            {
                var srVideo = ((MeTLLib.DataTypes.TargettedVideo)video.Value).video;
                srVideo.VideoWidth = srVideo.MediaElement.NaturalVideoWidth;
                srVideo.VideoHeight = srVideo.MediaElement.NaturalVideoHeight;
                srVideo.MediaElement.LoadedBehavior = MediaState.Manual;
                srVideo.MediaElement.ScrubbingEnabled = true;
                stack.images.AddVideo(srVideo);
            } foreach (var bubble in parser.bubbleList)
                stack.ReceiveNewBubble(bubble);
            EndInit();
        }
        private void MirrorPresentationSpace(Window1 parent)
        {
            try
            {
                var mirror = new Window { Content = new Projector { viewConstraint = parent.scroll } };
                Projector.Window = mirror;
                parent.Closed += (_sender, _args) => mirror.Close();
                mirror.WindowStyle = WindowStyle.None;
                mirror.AllowsTransparency = true;
                setSecondaryWindowBounds(mirror);
                mirror.Show();
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
            Commands.DugPublicSpace.ExecuteAsync(window);
        }
        private void InitiateDig(object _param)
        {
            withDragMarquee(SendNewDig);
        }
        private void InitiateGrabZoom(object _param)
        {
            App.Now("GrabZoom pressed");
            withDragMarquee(marquee =>
            {
                Commands.EndGrabZoom.ExecuteAsync(null);
                Commands.SetZoomRect.ExecuteAsync(marquee);
            });
        }
        private void withDragMarquee(Action<Rect> doWithRect)
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
            Point origin = new Point(-1, -1);
            Rect finalRect = new Rect();
            canvas.MouseDown += (sender, e) =>
            {
                var pos = e.GetPosition(this);
                var visPos = e.GetPosition(canvas);
                System.Windows.Controls.Canvas.SetLeft(marquee, visPos.X);
                System.Windows.Controls.Canvas.SetTop(marquee, visPos.Y);
                origin = pos;
                mouseDown = true;
            };
            canvas.MouseUp += (sender, e) =>
            {
                if (origin.X == -1 || origin.Y == -1) return;
                var pos = e.GetPosition(this);
                finalRect.X = (pos.X < origin.X) ? pos.X : origin.X;
                finalRect.Y = (pos.Y < origin.Y) ? pos.Y : origin.Y;
                finalRect.Height = Math.Abs(pos.Y - origin.Y);
                finalRect.Width = Math.Abs(pos.X - origin.X);
                mouseDown = false;
                if (!isPointNear(marquee.PointToScreen(finalRect.TopLeft),marquee.PointToScreen(finalRect.BottomRight),10))
                    doWithRect(finalRect);
                adornerLayer.Remove(adorner);
            };
            canvas.MouseMove += (sender, e) =>
            {
                if (!mouseDown || origin.X == -1 || origin.Y == -1) return;
                var pos = e.GetPosition(canvas);
                System.Windows.Controls.Canvas.SetLeft(marquee, Math.Min(origin.X, pos.X));
                System.Windows.Controls.Canvas.SetTop(marquee, Math.Min(origin.Y, pos.Y));
                marquee.Width = Math.Max(origin.X, pos.X) - Math.Min(origin.X, pos.X);
                marquee.Height = Math.Max(origin.Y, pos.Y) - Math.Min(origin.Y, pos.Y);
            };
            canvas.MouseLeave += (_sender, _args) =>
            {
                adornerLayer.Remove(adorner);
                Commands.EndGrabZoom.ExecuteAsync(null);
            };
            adornerLayer.Add(adorner);
        }
        private bool isPointNear(Point a, Point b, Int32 tolerance)
        {
            return (Math.Abs(a.X - b.X) < tolerance || Math.Abs(a.Y - b.Y) < tolerance);
        }
        private void SendNewDig(Rect rect)
        {
            var marquee = new Rectangle {Height=rect.Height, Width=rect.Width };
            System.Windows.Controls.Canvas.SetLeft(marquee, rect.Left);
            System.Windows.Controls.Canvas.SetTop(marquee, rect.Top);
            
            var origin = rect.Location;
            Commands.SendLiveWindow.ExecuteAsync(new LiveWindowSetup
            (Globals.slide, Globals.me, marquee, origin, new Point(0, 0), 
            MeTLLib.ClientFactory.Connection().UploadResourceToPath(
                                            toByteArray(this, marquee, origin),
                                            "Resource/" + Globals.slide.ToString(),
                                            "quizSnapshot.png",
                                            false).ToString()));
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
                this.clonePublicOnly(),
                path,
                width,
                height,
                width,
                height);
            MeTLLib.ClientFactory.Connection().UploadResource(new Uri(path,UriKind.RelativeOrAbsolute), Globals.me).ToString();
        }
        private FrameworkElement clonePublicOnly(){
            var clone = new InkCanvas();
            clone.Height = ActualHeight;
            clone.Width = ActualWidth;
            foreach(var stroke in stack.handwriting.Strokes.Where(s=>s.tag().privacy == "public"))
                clone.Strokes.Add(stroke.Clone());
            foreach(var canvas in new AbstractCanvas[]{stack.images, stack.text})
                foreach (var child in canvas.Children)
                {
                    var fe = (FrameworkElement)child;
                    if (fe.privacy() == "public")
                        clone.Children.Add(viewFor(fe));
                }
            var size = new Size(ActualWidth,ActualHeight);
            clone.Measure(size);
            clone.Arrange(new Rect(size));
            return clone;
        }
        private FrameworkElement cloneAll(){
            var clone = new InkCanvas();
            foreach(var stroke in stack.handwriting.Strokes)
                clone.Strokes.Add(stroke.Clone());
            foreach(var canvas in new AbstractCanvas[]{stack.images, stack.text})
                foreach (var child in canvas.Children)
                {
                    var fe = (FrameworkElement)child;
                    clone.Children.Add(viewFor(fe));
                }
            var size = new Size(ActualWidth,ActualHeight);
            clone.Measure(size);
            clone.Arrange(new Rect(size));
            return clone;
        }
        private FrameworkElement viewFor(FrameworkElement element) {
            var rect = new Rectangle {
                Width = element.ActualWidth,
                Height = element.ActualHeight,
                Fill=new VisualBrush(element)
            };
            /* Okay, this bit is complicated.  The bounds of an image are the bounds of the element(x,y,height,width), 
             * not the image, but the actualHeight of an image (which is calculated after drawing the image) which is 
             * the pixels of the imageData.  So, when an image is drawn with correct aspect ratio, it may be smaller
             * than the bounds, which means that the x and y will need to be adjusted by half the difference between the
             * height and the actual height.  However, this doesn't happen for textboxes, and if you apply the difference
             * one of them way a NaN, and adding a NaN to any number makes a NaN.  So, that's why these "SetTop" and "SetLeft"
             * are so horrible.
            */
            InkCanvas.SetTop(rect, (element is TextBox)?InkCanvas.GetTop(element):InkCanvas.GetTop(element) + ((element.Height != element.ActualHeight)?((element.Height - element.ActualHeight) / 2):0));
            InkCanvas.SetLeft(rect, (element is TextBox)?InkCanvas.GetLeft(element):InkCanvas.GetLeft(element) + ((element.Width != element.ActualWidth) ? ((element.Width - element.ActualWidth) / 2) : 0));
            return rect;
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
            get { return MeTLLib.DataTypes.Permissions.InferredTypeOf(Globals.conversationDetails.Permissions).Label; }
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
