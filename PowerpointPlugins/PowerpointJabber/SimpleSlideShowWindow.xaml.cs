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

namespace PowerpointJabber
{
    public partial class SimpleSlideShowWindow : Window
    {
        public ObservableCollection<SlideThumbnail> slideThumbs;
        public List<UbiquitousPen> pens;
        private UbiquitousPen currentPen;

        public SimpleSlideShowWindow()
        {
            InitializeComponent();
            DisableClickAdvance();
            slideThumbs = new ObservableCollection<SlideThumbnail>();
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
            PostSlideMoved();
        }
        private void PreSlideMoved()
        {
            //GenerateThumbnail(ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.Slide);
        }
        private void PostSlideMoved()
        {
            if (ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.State == PpSlideShowState.ppSlideShowRunning)
            {

                SetCanvasBackground();
                StrokeCanvas.Strokes.Clear();
                string CommentsText = "Notes: \r\n";
                foreach (Microsoft.Office.Interop.PowerPoint.Shape s in ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.Slide.NotesPage.Shapes)
                {
                    if (s.HasTextFrame == Microsoft.Office.Core.MsoTriState.msoTrue)
                        CommentsText += s.TextFrame.TextRange.Text + "\r\n";
                }
                NotesBlock.Text = CommentsText;
            }
        }
        private void SetCanvasBackground()
        {
            if (ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.State == PpSlideShowState.ppSlideShowRunning)
            {
                //GenerateThumbnail(ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.Slide);
                BackgroundOfCanvas.Source = slideThumbs.Where(c => c.slideNumber == ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.Slide.SlideNumber).First().thumbnail;
            }
        }
        private void moveToSelectedSlide(object sender, RoutedEventArgs e)
        {
            var origin = ((FrameworkElement)sender);
            if (origin.Tag != null)
            {
                PreSlideMoved();
                ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.GotoSlide((int)(Int32.Parse(origin.Tag.ToString())), Microsoft.Office.Core.MsoTriState.msoTrue);
                PostSlideMoved();
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
            {
                PreSlideMoved();
                ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.GotoSlide(currentSlide + 1, Microsoft.Office.Core.MsoTriState.msoTrue);
                PostSlideMoved();
            }
        }
        private void MoveToPrevSlide(object sender, RoutedEventArgs e)
        {
            var currentSlide = ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.CurrentShowPosition;
            if (currentSlide > 1)
            {
                PreSlideMoved();
                ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.GotoSlide(currentSlide - 1, Microsoft.Office.Core.MsoTriState.msoTrue);
                PostSlideMoved();
            }
        }
        private void MoveToNextBuild(object sender, RoutedEventArgs e)
        {
            PreSlideMoved();
            ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.Next();
            PostSlideMoved();
        }
        private void MoveToPrevBuild(object sender, RoutedEventArgs e)
        {
            PreSlideMoved();
            ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.Previous();
            PostSlideMoved();
        }
        private void ReFocusPresenter()
        {
            ThisAddIn.instance.Application.SlideShowWindows[1].Activate();
        }
        private void Pen(object sender, RoutedEventArgs e)
        {
            DisableClickAdvance();
            currentPen = pens.Where(c => c.penName.Equals(((FrameworkElement)sender).Tag.ToString())).First();
            ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.PointerColor.RGB = currentPen.RGBAasInt;
            ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.PointerType = PpSlideShowPointerType.ppSlideShowPointerPen;
            StrokeCanvas.DefaultDrawingAttributes = new System.Windows.Ink.DrawingAttributes
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
            StrokeCanvas.EditingMode = InkCanvasEditingMode.Ink;

            ReFocusPresenter();
        }
        private void Eraser(object sender, RoutedEventArgs e)
        {
            DisableClickAdvance();
            ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.PointerType = PpSlideShowPointerType.ppSlideShowPointerEraser;
            ReFocusPresenter();
        }
        private void Selector(object sender, RoutedEventArgs e)
        {
            EnableClickAdvance();
            ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.PointerType = PpSlideShowPointerType.ppSlideShowPointerAutoArrow;
            ReFocusPresenter();
        }
        private void EndSlideShow(object sender, RoutedEventArgs e)
        {
            ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.Exit();
        }
        private void hideSlide(object sender, RoutedEventArgs e)
        {
            var newState = PpSlideShowState.ppSlideShowRunning;
            switch (((FrameworkElement)sender).Tag.ToString())
            {
                case "white":
                    newState = PpSlideShowState.ppSlideShowWhiteScreen;
                    break;
                case "black":
                    newState = PpSlideShowState.ppSlideShowBlackScreen;
                    break;
            }
            ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.State = newState;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                slideThumbs.Clear();
                ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.Exit();
            }
            catch (Exception ex)
            {
            }
        }

        private void InkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            //To put the stroke onto the powerpoint document:
            try
            {
                Single[,] arrayOfPoints = new Single[e.Stroke.StylusPoints.Count, 2];
                var AP = ThisAddIn.instance.Application.ActivePresentation;
                for (int i = 0; i < e.Stroke.StylusPoints.Count; i++)
                {
                    arrayOfPoints[i, 0] = (float)e.Stroke.StylusPoints[i].X;
                    arrayOfPoints[i, 1] = (float)e.Stroke.StylusPoints[i].Y;
                }
                var newShape = AP.Slides[AP.SlideShowWindow.View.Slide.SlideNumber].Shapes.AddPolyline(arrayOfPoints);
                newShape.Line.Weight = currentPen.penWeight;
                newShape.Line.ForeColor.RGB = currentPen.RGBAasInt;
                newShape.Line.BackColor.RGB = currentPen.RGBAasInt;
                //I'm not sure whether the canvas should clear after every stroke yet.
                //StrokeCanvas.Strokes.Clear();

                //Update the slideViewer with the new stroke!
                GenerateThumbnail(ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.Slide);
            }
            catch (Exception ex)
            {
                MessageBox.Show("failed to add stroke: " + ex.Message);
            }
            //We should also send the stroke to the server here:
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
