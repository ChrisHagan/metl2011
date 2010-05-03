using System;
using System.Collections.Generic;
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
using SandRibbon.Components.Utility;
using SandRibbon.Quizzing;
using SandRibbon.Utils.Connection;
using SandRibbonInterop;
using SandRibbonObjects;
using SandRibbon.Utils;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace SandRibbon.Components
{
    public partial class PresentationSpace
    {
        public JabberWire.Location currentLocation = new JabberWire.Location();
        private string me;
        public ConversationDetails currentDetails;
        private bool synced = false;
        private bool joiningConversation;
        //public static PrivacyTools currentPrivacyTools = new PrivacyTools();
        public PresentationSpace()
        {
            InitializeComponent();
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<JabberWire.Credentials>( who => me = who.name));
            Commands.SetSync.RegisterCommand(new DelegateCommand<object>(setSync));
            Commands.InitiateDig.RegisterCommand(new DelegateCommand<object>(InitiateDig));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>(MoveSlide));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(JoinConversation));
            Commands.ReceiveLiveWindow.RegisterCommand(new DelegateCommand<LiveWindowSetup>(ReceiveLiveWindow));
            Commands.ReceiveQuiz.RegisterCommand(new DelegateCommand<QuizDetails>(receiveQuiz));
            Commands.MirrorPresentationSpace.RegisterCommand(new DelegateCommand<Window1>( MirrorPresentationSpace, CanMirrorPresentationSpace));
            Commands.SetPrivacy.RegisterCommand(new DelegateCommand<string>(p => { }, CanSetPrivacy));
            Commands.PreParserAvailable.RegisterCommand(new DelegateCommand<PreParser>(PreParserAvailable));
            Commands.CreateThumbnail.RegisterCommand(new DelegateCommand<int>(CreateThumbnail));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
            Commands.ConvertPresentationSpaceToQuiz.RegisterCommand(new DelegateCommand<int>(ConvertPresentationSpaceToQuiz));
            Commands.SyncedMoveRequested.RegisterCommand(new DelegateCommand<int>(setUpSyncDisplay));
            Loaded += presentationSpaceLoaded;
        
        }
        private void presentationSpaceLoaded(object sender, RoutedEventArgs e)
        {
            //remove these if you want the on-canvas privacy buttons to disappear.  If you do that, you MUST uncomment the static declaration of currentPrivacyTools
            var adorner = GetAdorner();
            AdornerLayer.GetAdornerLayer(adorner).Add(new UIAdorner(adorner, new PrivacyTools()));
        }

        private void setSync(object obj)
        {
            synced = !synced;
        }
        private void setUpSyncDisplay(int slide)
        {
            if(!synced) return;
            if(currentDetails == null) return;
            if (currentDetails.Author == me) return;
            if(currentDetails.Slides.Where(s => s.id.Equals(slide)).Count() == 0)return;
            Dispatcher.BeginInvoke((Action) delegate
                        {
                            var adorner = GetAdorner();
                            AdornerLayer.GetAdornerLayer(adorner).Add(new UIAdorner(adorner, new SyncDisplay()));
                        });
        }
        private void UpdateConversationDetails(ConversationDetails details)
        {
            currentDetails = details;
            joiningConversation = false;
            if(currentDetails.Author == me || currentDetails.Permissions.studentCanPublish)
                Commands.SetPrivacy.Execute(stack.handwriting.actualPrivacy);
            else
                Commands.SetPrivacy.Execute("private");
        }
        private FrameworkElement GetAdorner()
        {
            var element = (FrameworkElement)this;
            while(element.Parent != null && !(element.Name == "adornerGrid"))
                element = (FrameworkElement)element.Parent;
               
            foreach(var child in ((Grid)element).Children)
                if(((FrameworkElement)child).Name == "adorner")
                   return (FrameworkElement)child;
            return null;
        }
        private bool CanSetPrivacy(string s)
        {
            if(currentDetails == null)
                return false;
            return (currentDetails.Permissions.studentCanPublish || currentDetails.Author == me);
        }
        private void CreateThumbnail(int id)
        {
            try
            {
                const int side = 96;
                var dpi = 96;
                var bitmap = new RenderTargetBitmap(side, side, dpi, dpi, PixelFormats.Default);
                var dv = new DrawingVisual();
                using (var context = dv.RenderOpen())
                    context.DrawRectangle(new VisualBrush(stack), null, new Rect(new Point(), new Size(side, side)));
                bitmap.Render(dv);
                Commands.ThumbnailGenerated.Execute(new UnscaledThumbnailData { id = currentLocation.currentSlide, data = bitmap });
            }
            catch (OverflowException)
            {
                //The image is too large to thumbnail.  Just leave it be.
            }
        }
        private void PreParserAvailable(PreParser parser)
        {
            stack.handwriting.ReceiveStrokes(parser.ink);
            stack.images.ReceiveImages(parser.images.Values);
            foreach (var text in parser.text.Values)
                stack.text.doText(text);
            foreach(var quizDetails in parser.quizs)
                receiveQuiz(quizDetails);
            Worm.heart.Interval = TimeSpan.FromMilliseconds(1500);
        }
        private void MirrorPresentationSpace(Window1 parent)
        {
            var currentAttributes = stack.handwriting.DefaultDrawingAttributes;
            var mirror = new Window { Content = new Projector { viewConstraint = parent.scroll} };
            Projector.Window = mirror;
            parent.Closed += (_sender, _args) => mirror.Close();
            mirror.WindowStyle = WindowStyle.None;
            mirror.AllowsTransparency = true;
            setSecondaryWindowBounds( mirror);
            mirror.Show();
            Commands.SetDrawingAttributes.Execute(currentAttributes);
            Commands.SetPrivacy.Execute(stack.handwriting.privacy);
            //We rerequest the current slide data because we're not storing it in reusable form - it's in thread affine objects in the logical tree.
            Commands.MoveTo.Execute(currentLocation.currentSlide);
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
           System.Drawing.Rectangle r= getSecondaryScreenBounds();
           w.Top = r.Top;
           w.Left = r.Left;
           w.Width = r.Width;
           w.Height = r.Height;
        }
        private void MoveSlide(int slide)
        {
            if(currentDetails == null) return;
            currentLocation.currentSlide = slide;
            ClearAdorners();
        }
        private void ClearAdorners()
        {
            var doClear = (Action)delegate
                                      {
                                          removeAdornerItems(this);
                                          ClearPrivacy();
                                          removeSyncDisplay();
                                      };
            if (Thread.CurrentThread != Dispatcher.Thread)
                Dispatcher.BeginInvoke(doClear);
            else
                doClear();
        }
        private void removeSyncDisplay()
        {
            var adorner = GetAdorner();
            var adornerLayer = AdornerLayer.GetAdornerLayer(adorner);
            var adorners = adornerLayer.GetAdorners(adorner);
            if(adorners != null)
                foreach(var element in adorners)
                    if(element is UIAdorner)
                        if(((UIAdorner)element).contentType.Name.Equals("SyncDisplay"))
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
        private void JoinConversation(string jid)
        {
            joiningConversation = true;
            currentLocation.activeConversation = jid;

        }
        private void ReceiveLiveWindow(LiveWindowSetup window)
        {
            if (window.slide != currentLocation.currentSlide || window.author != me) return;
            window.visualSource = stack;
            Commands.DugPublicSpace.Execute(window);
        }
        private void InitiateDig(object _param)
        {
            var canvas = new System.Windows.Controls.Canvas();
            var adornerLayer = AdornerLayer.GetAdornerLayer(this);
            var adorners = adornerLayer.GetAdorners(this);
            if (adorners != null)
                foreach (var a in adorners)
                    if (a is UIAdorner && ((UIAdorner)a).contentType == typeof(System.Windows.Controls.Canvas))
                        return;
            var adorner = new UIAdorner(this, canvas);
            var adornee = this;
            var marquee = new Rectangle { Fill = Brushes.Purple };
            canvas.Background = new SolidColorBrush { Color = Colors.Wheat, Opacity = 0.1 };
            canvas.Children.Add(marquee);
            bool mouseDown = false;
            canvas.MouseDown += (sender, e)=>
            {
                var pos = e.GetPosition(canvas);
                System.Windows.Controls.Canvas.SetLeft(marquee, pos.X);
                System.Windows.Controls.Canvas.SetTop(marquee, pos.Y);
                mouseDown = true;
            };
            canvas.MouseUp += (sender, e)=>
            {
                mouseDown = false;
                var pos = e.GetPosition(adornee);
                var origin = new Point(pos.X - marquee.Width, pos.Y - marquee.Height);
                Commands.SendLiveWindow.Execute(new LiveWindowSetup {
                    frame=marquee,
                    origin=origin,
                    target=new Point(0,0), 
                    snapshotAtTimeOfCreation = ResourceUploader.uploadResourceToPath(
                        toByteArray(adornee, marquee, origin),
                        "Resource/"+currentLocation.currentSlide.ToString(), 
                        "quizSnapshot.png", 
                        false),
                    author=me,
                    slide=currentLocation.currentSlide
                });
                adornerLayer.Remove(adorner);
            };
            canvas.MouseMove += (sender, e)=>
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
        private void receiveQuiz(QuizDetails details)
        {
            var doReceive = (Action) delegate
            {
                var adorner = AdornerLayer.GetAdornerLayer(this);
                if (adorner != null)
                {
                    var adorners = adorner.GetAdorners(this);
                    if(adorners != null)
                        foreach (var a in adorners)
                            if (a is PollMarker && ((PollMarker)a).target == details.targetSlide)
                                return;
                    var x = 30 * (adorners == null ? 1 : adorners.Length);
                    var y = 5 * (adorners == null ? 1 : adorners.Length);
                    adorner.Add(new PollMarker(x, y, this, details));
                }
            };
            if (Thread.CurrentThread != Dispatcher.Thread)
                Dispatcher.BeginInvoke(doReceive);
            else
                doReceive();
        }
        private System.Windows.Shapes.Path privacyAdorner = new System.Windows.Shapes.Path();
        private readonly Brush privacyOverlay = new SolidColorBrush { Color = Colors.Red, Opacity = 0.2 };
        public void ClearPrivacy()
        {
            var geometry = new PathGeometry();
            privacyAdorner = new System.Windows.Shapes.Path { 
                Fill = privacyOverlay,  Data=geometry};
            geometry.FillRule = FillRule.Nonzero;
            var adornerLayer = AdornerLayer.GetAdornerLayer(this);
            if (adornerLayer == null) return;
            privacyAdorner.IsHitTestVisible = false;
            adornerLayer.Add(new UIAdorner(this, privacyAdorner));
        }
        public void AddPrivateRegion(IEnumerable<Point> vertices)
        {
            RemovePrivateRegion(vertices);
            var segments = vertices.Select(v => (PathSegment)new LineSegment(v, true));
            ((PathGeometry)privacyAdorner.Data).Figures.Add(new PathFigure(vertices.First(), segments, true));
        }
        public void RemovePrivateRegion(IEnumerable<Point> vertices)
        {
            if(vertices == null) return;
            var sum = vertices.Aggregate(0.0, (acc, v) => acc + v.X + v.Y);
            var geometry = (PathGeometry)privacyAdorner.Data;
            var regionToRemove = geometry.Figures.Where(
                f=>{
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
            var hostedFileName = ResourceUploader.uploadResource(me, path);
            Commands.SendQuiz.Execute(new QuizDetails
            {
                author = me,
                optionCount = options,
                quizPath = hostedFileName,
                returnSlide = currentLocation.currentSlide,
                targetSlide = currentLocation.currentSlide,
                target = "presentationSpace"
            });
        }
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new PresentationSpaceAutomationPeer(this);
        }
    }
    public class PresentationSpaceAutomationPeer : FrameworkElementAutomationPeer, IValueProvider
    {
        public PresentationSpaceAutomationPeer(FrameworkElement parent):base(parent) {}
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
            get { return Permissions.InferredTypeOf(this.PresentationSpace.currentDetails.Permissions).Label; }
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
}
