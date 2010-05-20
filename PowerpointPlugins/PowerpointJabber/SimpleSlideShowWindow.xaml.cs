using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Office.Interop.PowerPoint;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Ink;

namespace PowerpointJabber
{
    public partial class SimpleSlideShowWindow : Window
    {
        public ObservableCollection<SlideThumbnail> slideThumbs;
        public List<UbiquitousPen> pens;
        private UbiquitousPen currentPen;
        private Dictionary<int, StrokeCollection> strokeCollectionsForSlides;
        private List<InkCanvas> ActiveCanvasses;
        private int lastSlide;
        public Window slideshowMembrane;
        private Grid slideshowMembraneBounds;

        public SimpleSlideShowWindow()
        {
            InitializeComponent();
            SetUpCanvasses();
            DisableClickAdvance();
            isExtendedDesktopMode = false;
            isExtendedDesktopMode = true;
            slideThumbs = new ObservableCollection<SlideThumbnail>();
            strokeCollectionsForSlides = new Dictionary<int, StrokeCollection>();
            pens = new List<UbiquitousPen> 
                {
                    new UbiquitousPen{penName="thinBlack",penColour=System.Windows.Media.Brushes.Black,penWeight=1.5f},
                    new UbiquitousPen{penName="thinRed",penColour=System.Windows.Media.Brushes.Red,penWeight=1.5f},
                    new UbiquitousPen{penName="thinYellow",penColour=System.Windows.Media.Brushes.Yellow,penWeight=1.5f},
                    new UbiquitousPen{penName="thinBlue",penColour=System.Windows.Media.Brushes.Blue,penWeight=1.5f},
                    new UbiquitousPen{penName="thinGreen",penColour=System.Windows.Media.Brushes.Green,penWeight=1.5f},
                    new UbiquitousPen{penName="thinDarkBlue",penColour=System.Windows.Media.Brushes.DarkBlue,penWeight=1.5f},
                    new UbiquitousPen{penName="medRed",penColour=System.Windows.Media.Brushes.Red,penWeight=3f},
                    new UbiquitousPen{penName="medBlue",penColour=System.Windows.Media.Brushes.Blue,penWeight=3f},
                    new UbiquitousPen{penName="medwhite",penColour=System.Windows.Media.Brushes.Yellow,penWeight=3f},
                    new UbiquitousPen{penName="thinWhite",penColour=System.Windows.Media.Brushes.White,penWeight=1.5f}
                };
            currentPen = pens[0];
            GenerateThumbnails();
            SlideViewer.Items.Clear();
            SlideViewer.ItemsSource = slideThumbs;
            SlideViewer.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("slideNumber", System.ComponentModel.ListSortDirection.Ascending));
            PensControl.Items.Clear();
            PensControl.ItemsSource = pens;
            lastSlide = ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.Slide.SlideID;
            SlideMoved();
        }
        private void switchDisplayMode(object sender, RoutedEventArgs e)
        {
            if (isExtendedDesktopMode)
                isExtendedDesktopMode = false;
            else
                isExtendedDesktopMode = true;
        }
        private void SetUpCanvasses()
        {
            slideshowMembraneBounds = new Grid();
            var SlideShowMembraneCanvas = new InkCanvas
            {
                Name = "slideShowMembraneCanvas",
                Background = new SolidColorBrush { Color = new System.Windows.Media.Color {A=1,R=255,G=255,B=255 } },
                EditingMode = InkCanvasEditingMode.Ink,
                IsHitTestVisible = true,
                Height = double.NaN,
                Width = double.NaN,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            slideshowMembraneBounds.Children.Add(SlideShowMembraneCanvas);
            var viewbox = new Viewbox { Child = slideshowMembraneBounds };
            slideshowMembrane = new Window
            {
                Content = viewbox,
                Topmost = true,
                AllowsTransparency = true,
                Background = System.Windows.Media.Brushes.Transparent,
                WindowStyle = WindowStyle.None
            };
            slideshowMembrane.Show();
            ActiveCanvasses = new List<InkCanvas> { StrokeCanvas, StrokeCanvas2, SlideShowMembraneCanvas };
            foreach (InkCanvas canvas in ActiveCanvasses)
            {
                canvas.StrokesReplaced += InkCanvas_StrokesReplaced;
                attachInkCanvasHandlers(canvas);
            }
        }
        private void attachInkCanvasHandlers(InkCanvas canvas)
        {
            canvas.StrokeCollected += InkCanvas_StrokeCollected;
            canvas.StrokeErasing += InkCanvas_StrokeErasing;
            canvas.SelectionMoved += InkCanvas_SelectionMoved;
        }
        private void detachInkCanvasHandlers(InkCanvas canvas)
        {
            canvas.StrokeCollected -= InkCanvas_StrokeCollected;
            canvas.StrokeErasing -= InkCanvas_StrokeErasing;
            canvas.SelectionMoved -= InkCanvas_SelectionMoved;
        }
        public bool isExtendedDesktopMode
        {
            get
            {
                if (SlideShowGridContainer.Visibility == Visibility.Visible)
                    return true;
                else
                    return false;
            }
            set
            {
                var currentWidthOfButtons = ButtonSection.Width;
                var currentWidthOfSlideViewer = SlideViewerSection.Width;
                switch (value)
                {
                    case true:
                        if (System.Windows.Forms.Screen.AllScreens.Length > 1)
                        {
                            SlideShowGridContainer.Visibility = Visibility.Visible;
                            MeTLGridContainer.Visibility = Visibility.Visible;
                            BetweenSlideShowAndMeTL.Visibility = Visibility.Visible;
                            this.Background = System.Windows.Media.Brushes.Black;
                            SlideViewerSection.Width = currentWidthOfSlideViewer;
                            ButtonSection.Width = currentWidthOfButtons;
                            var SecondaryScreen = System.Windows.Forms.Screen.AllScreens[1];
                            //I'm not sure why there's a natural zoom on all slideshow windows, but there is.  It's very annoying.
                            ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.Left = ((SecondaryScreen.Bounds.Left) / 4) * 3;
                            ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.Top = ((SecondaryScreen.Bounds.Top) / 4) * 3;
                            ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.Width = ((SecondaryScreen.Bounds.Width) / 4) * 3;
                            ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.Height = ((SecondaryScreen.Bounds.Height) / 4) * 3;
                            attachSlideShowMembraneToSlideShow(SecondaryScreen);
                        }
                        break;
                    case false:
                        SlideShowGridContainer.Visibility = Visibility.Collapsed;
                        MeTLGridContainer.Visibility = Visibility.Collapsed;
                        BetweenSlideShowAndMeTL.Visibility = Visibility.Collapsed;
                        this.Background = System.Windows.Media.Brushes.Transparent;
                        SlideViewerSection.Width = currentWidthOfSlideViewer;
                        ButtonSection.Width = currentWidthOfButtons;
                        var PrimaryScreen = System.Windows.Forms.Screen.AllScreens[0];
                        ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.Left = ((PrimaryScreen.Bounds.Left) / 4) * 3;
                        ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.Top = ((PrimaryScreen.Bounds.Top) / 4) * 3;
                        ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.Width = ((PrimaryScreen.Bounds.Width) / 4) * 3;
                        ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.Height = ((PrimaryScreen.Bounds.Height) / 4) * 3;
                        attachSlideShowMembraneToSlideShow(PrimaryScreen);
                        break;
                }
            }
        }
        public void attachSlideShowMembraneToSlideShow(System.Windows.Forms.Screen PrimaryScreen)
        {
            slideshowMembraneBounds.Width = ThisAddIn.instance.Application.ActivePresentation.SlideMaster.Width;
            slideshowMembraneBounds.Height = ThisAddIn.instance.Application.ActivePresentation.SlideMaster.Height;
            slideshowMembrane.Top = PrimaryScreen.Bounds.Top;
            slideshowMembrane.Left = PrimaryScreen.Bounds.Left;
            slideshowMembrane.Height = PrimaryScreen.Bounds.Height;
            slideshowMembrane.Width = PrimaryScreen.Bounds.Width;
        }
        public void saveAllStrokesToPresentation()
        {
            if (!(strokeCollectionsForSlides.Any(s => s.Value.Count > 0)))
                return;
            SlideMoved();
            var result = MessageBox.Show("Would you like to save the ink from this presentation?",
                "Save ink?", MessageBoxButton.YesNo);
            if (new[] { MessageBoxResult.Cancel, MessageBoxResult.No, MessageBoxResult.None }.Any(s => s == result))
                return;
            foreach (KeyValuePair<int, StrokeCollection> entry in strokeCollectionsForSlides)
            {
                var slideID = entry.Key;
                var slideNumber = ThisAddIn.instance.Application.ActivePresentation.Slides.FindBySlideID(slideID).SlideNumber;
                foreach (Stroke s in entry.Value)
                {
                    addStrokeToPowerpointPresentation(s, slideNumber);
                }
            }
        }
        public void OnSlideChanged()
        {
            SlideMoved();
        }
        private void SlideMoved()
        {
            var AP = ThisAddIn.instance.Application.ActivePresentation;
            if (strokeCollectionsForSlides.Any(s => s.Key == lastSlide))
            {
                var strokesForCurrentSlide = strokeCollectionsForSlides[lastSlide];
                strokesForCurrentSlide.Clear();
                foreach (InkCanvas canvas in ActiveCanvasses)
                    foreach (Stroke stroke in canvas.Strokes)
                        if (strokesForCurrentSlide.Count(s => stroke.sum().checksum.ToString().Equals(s.sum().checksum.ToString())) == 0)
                            strokesForCurrentSlide.Add(stroke.Clone());
            }
            else
            {
                strokeCollectionsForSlides.Add(lastSlide, new StrokeCollection());
                foreach (InkCanvas canvas in ActiveCanvasses)
                    strokeCollectionsForSlides[lastSlide].Add(canvas.Strokes.Clone());
            }
            if (ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.State == PpSlideShowState.ppSlideShowRunning)
            {
                SetCanvasBackground();
                foreach (InkCanvas canvas in ActiveCanvasses)
                    canvas.Strokes.Clear();
                if (strokeCollectionsForSlides.Any(s => s.Key == ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.Slide.SlideID))
                {
                    var strokesForCurrentSlide = strokeCollectionsForSlides[ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.Slide.SlideID];
                    foreach (Stroke stroke in strokesForCurrentSlide)
                        foreach (InkCanvas canvas in ActiveCanvasses)
                            canvas.Strokes.Add(stroke.Clone());
                }
                string CommentsText = "Notes: \r\n";
                foreach (Microsoft.Office.Interop.PowerPoint.Shape s in ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.Slide.NotesPage.Shapes)
                {
                    if (s.HasTextFrame == Microsoft.Office.Core.MsoTriState.msoTrue)
                        CommentsText += s.TextFrame.TextRange.Text + "\r\n";
                }
                NotesBlock.Text = CommentsText;
                lastSlide = AP.SlideShowWindow.View.Slide.SlideID;
            }
        }
        private void SetCanvasBackground()
        {
            if (ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.State == PpSlideShowState.ppSlideShowRunning)
                BackgroundOfCanvas.Source = slideThumbs.Where(c => c.slideNumber == ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.Slide.SlideNumber).First().thumbnail;
        }
        private void moveToSelectedSlide(object sender, RoutedEventArgs e)
        {
            var origin = ((FrameworkElement)sender);
            if (origin.Tag != null)
            {
                ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.GotoSlide((int)(Int32.Parse(origin.Tag.ToString())), Microsoft.Office.Core.MsoTriState.msoTrue);
            }
        }
        private void GenerateThumbnails()
        {
            foreach (Slide slide in ThisAddIn.instance.Application.ActivePresentation.Slides)
            {
                GenerateThumbnail(slide);
            }
        }
        private void GenerateThumbnail(Slide slide)
        {
            var Width = Convert.ToInt32(ThisAddIn.instance.Application.ActivePresentation.SlideMaster.Width);
            var Height = Convert.ToInt32(ThisAddIn.instance.Application.ActivePresentation.SlideMaster.Height);
            var filename = System.IO.Directory.GetCurrentDirectory().ToString() + "\\pptSlideThumbnail" + slide.SlideNumber + ".png";
            try
            {
                slide.Export(filename, "PNG", Width, Height);
            }
            catch (Exception ex)
            {
            }
            BitmapImage img = new BitmapImage();
            MemoryStream ms = new MemoryStream(File.ReadAllBytes(filename));
            PngBitmapDecoder bd = new PngBitmapDecoder(ms, BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.None);

            ImageSource image = bd.Frames[0];
            if (slideThumbs.Any(c => c.slideNumber == slide.SlideNumber))
                slideThumbs.Remove(slideThumbs.Where(c => c.slideNumber == slide.SlideNumber).First());
            slideThumbs.Add(new SlideThumbnail { thumbnail = image, slideNumber = slide.SlideNumber });
        }
        private void DisableClickAdvance()
        {
            foreach (Slide slide in ThisAddIn.instance.Application.ActivePresentation.Slides)
            {
                slide.SlideShowTransition.AdvanceOnClick = Microsoft.Office.Core.MsoTriState.msoFalse;
            }
        }
        private void EnableClickAdvance()
        {
            foreach (Slide slide in ThisAddIn.instance.Application.ActivePresentation.Slides)
            {
                slide.SlideShowTransition.AdvanceOnClick = Microsoft.Office.Core.MsoTriState.msoTrue;
            }
        }
        private void MoveToNextSlide(object sender, RoutedEventArgs e)
        {
            var currentSlide = ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.CurrentShowPosition;
            if (currentSlide < ThisAddIn.instance.Application.ActivePresentation.Slides.Count)
                ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.GotoSlide(currentSlide + 1, Microsoft.Office.Core.MsoTriState.msoTrue);
        }
        private void MoveToPrevSlide(object sender, RoutedEventArgs e)
        {
            var currentSlide = ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.CurrentShowPosition;
            if (currentSlide > 1)
                ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.GotoSlide(currentSlide - 1, Microsoft.Office.Core.MsoTriState.msoTrue);
        }
        private void MoveToNextBuild(object sender, RoutedEventArgs e)
        {
            ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.Next();
        }
        private void MoveToPrevBuild(object sender, RoutedEventArgs e)
        {
            ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.Previous();
        }
        private void ReFocusPresenter()
        {
            slideshowMembrane.Focus();
            this.Focus();
            //ThisAddIn.instance.Application.SlideShowWindows[1].Activate();
        }
        private void Pen(object sender, RoutedEventArgs e)
        {
            DisableClickAdvance();
            currentPen = pens.Where(c => c.penName.Equals(((FrameworkElement)sender).Tag.ToString())).First();
            ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.PointerColor.RGB = currentPen.RGBAasInt;
            ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.PointerType = PpSlideShowPointerType.ppSlideShowPointerPen;
            foreach (InkCanvas canvas in ActiveCanvasses)
            {
                canvas.DefaultDrawingAttributes = new System.Windows.Ink.DrawingAttributes
                    {
                        Width = currentPen.penWeight,
                        Height = currentPen.penWeight,
                        Color = new System.Windows.Media.Color
                        {
                            A = (byte)currentPen.A,
                            R = (byte)currentPen.R,
                            G = (byte)currentPen.G,
                            B = (byte)currentPen.B
                        }
                    };
                canvas.EditingMode = InkCanvasEditingMode.Ink;
            }
            ReFocusPresenter();
        }
        private void Eraser(object sender, RoutedEventArgs e)
        {
            DisableClickAdvance();
            ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.PointerType = PpSlideShowPointerType.ppSlideShowPointerEraser;
            foreach (InkCanvas canvas in ActiveCanvasses)
            {
                canvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
            }
            ReFocusPresenter();
        }
        private void Selector(object sender, RoutedEventArgs e)
        {
            EnableClickAdvance();
            ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.PointerType = PpSlideShowPointerType.ppSlideShowPointerAutoArrow;
            foreach (InkCanvas canvas in ActiveCanvasses)
            {
                canvas.EditingMode = InkCanvasEditingMode.Select;
            }
            ReFocusPresenter();
        }
        private void EndSlideShow(object sender, RoutedEventArgs e)
        {
            if (ThisAddIn.instance.Application.SlideShowWindows.Count > 0)
                ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.Exit();
            if (this != null)
            {
                this.Close();
            }
        }
        private void hideSlide(object sender, RoutedEventArgs e)
        {
            var newState = PpSlideShowState.ppSlideShowRunning;
            if (!String.IsNullOrEmpty(((FrameworkElement)sender).Tag.ToString()))
            {
                switch (((FrameworkElement)sender).Tag.ToString())
                {
                    case "white":
                        newState = PpSlideShowState.ppSlideShowWhiteScreen;
                        break;
                    case "black":
                        newState = PpSlideShowState.ppSlideShowBlackScreen;
                        break;
                }
            }
            ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.State = newState;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                saveAllStrokesToPresentation();
                slideThumbs.Clear();
                ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.Exit();
            }
            catch (Exception ex)
            {
            }
        }
        private void addStrokeToPowerpointPresentation(System.Windows.Ink.Stroke stroke, int SlideNumber)
        {
            try
            {
                Single[,] arrayOfPoints = new Single[stroke.StylusPoints.Count, 2];
                var AP = ThisAddIn.instance.Application.ActivePresentation;
                for (int i = 0; i < stroke.StylusPoints.Count; i++)
                {
                    arrayOfPoints[i, 0] = (float)stroke.StylusPoints[i].X;
                    arrayOfPoints[i, 1] = (float)stroke.StylusPoints[i].Y;
                }
                var strokeColor = new SolidColorBrush(new System.Windows.Media.Color
                {
                    A = stroke.DrawingAttributes.Color.A,
                    R = stroke.DrawingAttributes.Color.R,
                    G = stroke.DrawingAttributes.Color.G,
                    B = stroke.DrawingAttributes.Color.B
                });
                UbiquitousPen currentStrokeAttributes = new UbiquitousPen
                {
                    penColour = strokeColor,
                    penWeight = (float)stroke.DrawingAttributes.Height,
                    penName = "temporaryBrush"
                };
                var currentSlide = AP.Slides[SlideNumber];
                var newShape = currentSlide.Shapes.AddPolyline(arrayOfPoints);
                newShape.Line.Weight = currentStrokeAttributes.penWeight;
                newShape.Line.ForeColor.RGB = currentStrokeAttributes.RGBAasInt;
                newShape.Line.BackColor.RGB = currentStrokeAttributes.RGBAasInt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("failed to add stroke: " + ex.Message);
            }

        }
        private void InkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            if (ThisAddIn.instance.wire.isConnected && ThisAddIn.instance.wire.isInConversation)
                ThisAddIn.instance.wire.sendRawStroke(e.Stroke);
            InkCanvas source = (InkCanvas)sender;
            foreach (InkCanvas canvas in ActiveCanvasses)
                if (canvas != source)
                {
                    detachInkCanvasHandlers(canvas);
                    if (canvas.Strokes.Count(s => s.sum().checksum.ToString().Equals(e.Stroke.sum().checksum.ToString())) == 0)
                        canvas.Strokes.Add(e.Stroke);
                    attachInkCanvasHandlers(canvas);
                }
        }
        private void InkCanvas_StrokeErasing(object sender, InkCanvasStrokeErasingEventArgs e)
        {
            if (ThisAddIn.instance.wire.isConnected && ThisAddIn.instance.wire.isInConversation)
                ThisAddIn.instance.wire.sendRawDirtyStroke(e.Stroke);
            InkCanvas source = (InkCanvas)sender;
            foreach (InkCanvas canvas in ActiveCanvasses)
                if (canvas != source)
                {
                    detachInkCanvasHandlers(canvas);
                    var StrokesToRemove = new StrokeCollection();
                    foreach (Stroke s in canvas.Strokes)
                        if (e.Stroke.sum().checksum.ToString().Equals(s.sum().checksum.ToString()))
                            StrokesToRemove.Add(s);
                    canvas.Strokes.Remove(StrokesToRemove);
                    attachInkCanvasHandlers(canvas);
                }
        }
        private void InkCanvas_SelectionMoved(object sender, EventArgs e)
        {
            InkCanvas source = (InkCanvas)sender;
            foreach (InkCanvas canvas in ActiveCanvasses)
            {
                if (canvas != source)
                {
                    detachInkCanvasHandlers(canvas);
                    canvas.Strokes = source.Strokes.Clone();
                    attachInkCanvasHandlers(canvas);
                }
            }
        }
        private void InkCanvas_StrokesReplaced(object sender, InkCanvasStrokesReplacedEventArgs e)
        {
            foreach (Stroke droppedStroke in (e.PreviousStrokes.Where(s => !e.NewStrokes.Contains(s)).ToList()))
            {
                if (ThisAddIn.instance.wire.isConnected && ThisAddIn.instance.wire.isInConversation)
                    ThisAddIn.instance.wire.sendRawDirtyStroke(droppedStroke);
                foreach (InkCanvas canvas in ActiveCanvasses)
                    if (canvas.Strokes.Contains(droppedStroke))
                        canvas.Strokes.Remove(droppedStroke);
            }
            foreach (Stroke newStroke in (e.NewStrokes.Where(s => !e.NewStrokes.Contains(s)).ToList()))
            {
                if (ThisAddIn.instance.wire.isConnected && ThisAddIn.instance.wire.isInConversation)
                    ThisAddIn.instance.wire.sendRawStroke(newStroke);
                foreach (InkCanvas canvas in ActiveCanvasses)
                    if (!canvas.Strokes.Contains(newStroke))
                        canvas.Strokes.Add(newStroke);
            }
        }
    }

    public class SlideThumbnail
    {
        public SlideThumbnail()
        {
        }
        public ImageSource thumbnail { get; set; }
        public int slideNumber { get; set; }
    }
    public class UbiquitousPen
    {
        public UbiquitousPen()
        {
        }
        public string penName { get; set; }
        public float penWeight { get; set; }
        public SolidColorBrush penColour { get; set; }
        public int R { get { return penColour.Color.R; } }
        public int G { get { return penColour.Color.G; } }
        public int B { get { return penColour.Color.B; } }
        public int A { get { return penColour.Color.A; } }
        public int RGBAasInt { get { return ColorTranslator.ToOle(System.Drawing.Color.FromArgb(A, R, G, B)); } }
    }
}
