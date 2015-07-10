using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using SandRibbon.Providers;
using MeTLLib.DataTypes;
using SandRibbon.Components.Pedagogicometry;
using Image = System.Windows.Controls.Image;
using SandRibbon.Quizzing;
using System.Windows.Media.Effects;

namespace SandRibbon.Components
{
    public partial class PresentationSpace
    {
        private bool inConversation;
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
<<<<<<< HEAD
=======
            Commands.ExploreBubble.RegisterCommand(new DelegateCommand<ThoughtBubble>(ExploreBubble));
>>>>>>> 719c8e43b84766b4fcbff20be9b761dc1f0604b0
            Commands.InitiateGrabZoom.RegisterCommand(new DelegateCommand<object>(InitiateGrabZoom));
            Commands.Highlight.RegisterCommand(new DelegateCommand<HighlightParameters>(highlight));
            Commands.RemoveHighlight.RegisterCommand(new DelegateCommand<HighlightParameters>(removeHighlight));
            Commands.GenerateScreenshot.RegisterCommand(new DelegateCommand<ScreenshotDetails>(SendScreenShot));
            Commands.BanhammerSelectedItems.RegisterCommand(new DelegateCommand<object>(BanHammerSelectedItems));
            Commands.ShowConversationSearchBox.RegisterCommand(new DelegateCommand<object>(showConversationSearch));
            Commands.HideConversationSearchBox.RegisterCommand(new DelegateCommand<object>(hideConversationSearch));
            Commands.AllStaticCommandsAreRegistered();
            inConversation = true;
        }
<<<<<<< HEAD
=======
        private void ExploreBubble(ThoughtBubble thoughtBubble)
        {
            var origin = new Point(0, 0);
            var marquee = new Rectangle();
            marquee.Width = this.ActualWidth;
            marquee.Height = this.ActualHeight;
>>>>>>> 719c8e43b84766b4fcbff20be9b761dc1f0604b0

        private void hideConversationSearch(object obj)
        {
            inConversation = true;
        }
<<<<<<< HEAD

        private void showConversationSearch(object obj)
=======
        private void presentationSpaceLoaded(object sender, RoutedEventArgs e)
>>>>>>> 719c8e43b84766b4fcbff20be9b761dc1f0604b0
        {
            inConversation = false;
        }

        private void BanHammerSelectedItems(object obj)
        {
            var authorList = stack.GetSelectedAuthors();
            var authorColor = stack.ColourSelectedByAuthor(authorList);
            var details = Globals.conversationDetails;
            foreach (var author in authorList)
            {
                if (!details.blacklist.Contains(author))
                    details.blacklist.Add(author);
            }
            ClientFactory.Connection().UpdateConversationDetails(details);
            GenerateBannedContentScreenshot(authorColor);
            Commands.SetPrivacyOfItems.Execute(Privacy.Private);
        }

        private void GenerateBannedContentScreenshot(Dictionary<string, Color> blacklisted)
        {
            var time = SandRibbonObjects.DateTimeFactory.Now().Ticks;
            DelegateCommand<string> sendScreenshot = null;
            sendScreenshot = new DelegateCommand<string>(hostedFileName =>
                             {
                                 Commands.ScreenshotGenerated.UnregisterCommand(sendScreenshot);
                                 var conn = MeTLLib.ClientFactory.Connection();
                                 var slide = Globals.slides.Where(s => s.id == Globals.slide).First(); // grab the current slide index instead of the slide id
                                 conn.UploadAndSendSubmission(new MeTLStanzas.LocalSubmissionInformation(/*conn.location.currentSlide*/slide.index + 1,Globals.me,"bannedcontent",
                                     Privacy.Private, -1L, hostedFileName, Globals.conversationDetails.Title, blacklisted,Globals.generateId(hostedFileName)));
                             });
            Commands.ScreenshotGenerated.RegisterCommand(sendScreenshot);
            Commands.GenerateScreenshot.ExecuteAsync(new ScreenshotDetails
                                                    {
                                                        time = time,
                                                        message = string.Format("Banned content submission at {0}", new DateTime(time)),
                                                        showPrivate = false,
                                                        dimensions = new Size(1024, 768)
                                                    });
        }

        private void setUpSyncDisplay(int slide)
        {
            if (!Globals.synched) return;
            if(!inConversation) return;
            if (slide == Globals.slide) return;
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
            catch (NotSetException)
            {
                //You are listening to the channel but have not yet joined the room
            }
        }
        private void UpdateConversationDetails(ConversationDetails _details)
        {
<<<<<<< HEAD
            if (details.IsEmpty) return;
            if (string.IsNullOrEmpty(details.Jid) || !(Globals.credentials.authorizedGroups.Select(s => s.groupKey.ToLower()).Contains(details.Subject.ToLower()))) return;
            try
            {
                if (Globals.conversationDetails == null)
                    Commands.SetPrivacy.ExecuteAsync("private");
                else if (Globals.conversationDetails.Author == Globals.me) 
                        Commands.SetPrivacy.ExecuteAsync("public");
=======
            joiningConversation = false;
            try
            {
                if (currentDetails.Author == Globals.me || Globals.conversationDetails.Permissions.studentCanPublish)
                    Commands.SetPrivacy.Execute(stack.handwriting.actualPrivacy);
>>>>>>> 719c8e43b84766b4fcbff20be9b761dc1f0604b0
            }
            catch (NotSetException)
            {
                Commands.SetPrivacy.ExecuteAsync("private");
            }
        }
        private FrameworkElement GetAdorner()
        {
            var element = (FrameworkElement)this;
            while (element.Parent != null && !(element.Name == "adornerGrid"))
                element = (FrameworkElement)element.Parent;

            foreach (var child in ((Grid)element).Children)
                if (((FrameworkElement)child).Name == "syncPageOverlay")
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
            var file = "";
            Dispatcher.adopt(() =>
            {
                var targetSize = ResizeHelper.ScaleMajorAxisToCanvasSize(stack, details.dimensions);
                var bitmap = new RenderTargetBitmap((int)targetSize.Width ,(int)targetSize.Height, dpi, dpi, PixelFormats.Default);
                var dv = new DrawingVisual();
                using (var context = dv.RenderOpen())
                {
                    var visual = details.showPrivate ? cloneAll() : clonePublicOnly();
                    context.DrawRectangle(new VisualBrush(visual), null, targetSize);
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
        private void PreParserAvailable(MeTLLib.Providers.Connection.PreParser parser)
        {
            BeginInit();
            stack.ReceiveStrokes(parser.ink);
            stack.ReceiveImages(parser.images.Values);
            foreach (var text in parser.text.Values)
                stack.DoText(text);
            /*foreach (var moveDelta in parser.moveDeltas)
                stack.ReceiveMoveDelta(moveDelta, processHistory: true);
            */
            stack.RefreshCanvas();
            EndInit();
        }
        private void MirrorPresentationSpace(Window1 parent)
        {
            try
            {
                var mirror = new Window { Content = new Projector { viewConstraint = parent.scroll } };
                if (Projector.Window != null)
                    Projector.Window.Close();
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
                if (Globals.pedagogy.code < PedagogyCode.CollaborativePresentation) return;
                privacyOverlay = new SolidColorBrush { Color = color, Opacity = 0.2 };
                privacyAdorner.Fill = privacyOverlay;
                RemovePrivateRegion(vertices);
                var segments = vertices.Select(v => (PathSegment)new LineSegment(v, true));
                if (privacyAdorner.Data == null) return;
                ((PathGeometry)privacyAdorner.Data).Figures.Add(new PathFigure(vertices.First(), segments, true));
            }
            catch (NotSetException)
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
            foreach(var stroke in stack.PublicStrokes)
                clone.Strokes.Add(stroke.Clone());
                foreach (var child in stack.Work.Children)
                {
                    var fe = (FrameworkElement)child;
                    if (fe.privacy() == Privacy.Public)
                    {
                        if (child is Image)
                        {
                            var image = (Image)child;
                            var e = viewFor((FrameworkElement)child);
                            Panel.SetZIndex(e, image.tag().author == Globals.me ? 3 : 2);
                            clone.Children.Add(e);
                        }
                        else
                        {
                            var e = viewFor(fe);
                            Panel.SetZIndex(e, 4);
                            clone.Children.Add(e);
                        }
                    }
                }
            var size = new Size(ActualWidth,ActualHeight);
            clone.Measure(size);
            clone.Arrange(new Rect(size));
            return clone;
        }
        private FrameworkElement cloneAll(){
            var clone = new InkCanvas();
            foreach(var stroke in stack.AllStrokes)
                clone.Strokes.Add(stroke.Clone());
                foreach (var child in stack.Work.Children)
                {
                    var fe = (FrameworkElement)child;
                    if(child is Image)
                    {
                        var image = (Image)child;
                        var e = viewFor(image);
                        Panel.SetZIndex(e, image.tag().author == Globals.me ? 3 : 1);
                        clone.Children.Add(e);
                    }
                    else
                    {
                        var e = viewFor(fe);
                        Panel.SetZIndex(e, 4);
                        clone.Children.Add(e);
                    }
                }
            var size = new Size(ActualWidth,ActualHeight);
            clone.Measure(size);
            clone.Arrange(new Rect(size));
            return clone;
        }

        /*
         * The effect size is taken into consideration when drawn 
         */
        private double GetEffectRadius(FrameworkElement element)
        {
            var dropShadow = element.Effect as DropShadowEffect;
            if (dropShadow != null)
            {
                return dropShadow.BlurRadius;
            }

            return 0.0;
        }

        private FrameworkElement viewFor(FrameworkElement element) 
        {
            var effectDiameter = 2 * GetEffectRadius(element);

            var rect = new Rectangle {
                Width = element.ActualWidth + effectDiameter,
                Height = element.ActualHeight + effectDiameter,
                Fill = new VisualBrush(element)
            };
            /*
             * Okay, this bit is complicated.  The bounds of an image are the bounds of the element(x,y,height,width), 
             * not the image, but the actualHeight of an image (which is calculated after drawing the image) which is 
             * the pixels of the imageData.  So, when an image is drawn with correct aspect ratio, it may be smaller
             * than the bounds, which means that the x and y will need to be adjusted by half the difference between the
             * height and the actual height.  However, this doesn't happen for textboxes, and if you apply the difference
             * one of them way a NaN, and adding a NaN to any number makes a NaN.  So, that's why these "SetTop" and "SetLeft"
             * are so horrible.
             */
            var left = InkCanvas.GetLeft(element);
            var top = InkCanvas.GetTop(element);
            if (element is Image)
            {
                if(!Double.IsNaN(element.Height) && element.Height != element.ActualHeight)
                  top += ((element.Height - element.ActualHeight)/2);
                if (!Double.IsNaN(element.Width) && element.Width != element.ActualWidth)
                  left += ((element.Width - element.ActualWidth)/2); 
            }
            left -= effectDiameter / 2;
            top -= effectDiameter / 2;
            
            InkCanvas.SetTop(rect, top);
            InkCanvas.SetLeft(rect,left);

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
