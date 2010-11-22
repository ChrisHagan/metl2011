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
using Point = System.Windows.Point;

namespace PowerpointJabber
{
    public partial class SimplePenWindow : Window
    {
        private bool presenterView
        {
            get
            {
                //This is soluable in ppt2010.
                //bool isPresenter = ThisAddIn.instance.Application.ActivePresentation.SlideShowSettings.ShowPresenterView = Microsoft.Office.Core.MsoTriState.mso.True;
                bool isPresenter = WindowsInteropFunctions.presenterActive;
                return isPresenter;
            }
        }
        public List<UbiquitousPen> pens;
        private List<slideIndicator> slides = new List<slideIndicator>();
        private UbiquitousPen currentPen;
        public SimplePenWindow()
        {
            InitializeComponent();
            pens = new List<UbiquitousPen> 
                {
                    new UbiquitousPen{penName="Black",penColour=System.Windows.Media.Brushes.Black,penWeight=1.5f},
                    new UbiquitousPen{penName="Blue",penColour=System.Windows.Media.Brushes.Blue,penWeight=1.5f},
                    new UbiquitousPen{penName="Red",penColour=System.Windows.Media.Brushes.Red,penWeight=3f},
                    new UbiquitousPen{penName="Green",penColour=System.Windows.Media.Brushes.Green,penWeight=3f},
                    new UbiquitousPen{penName="Yellow",penColour=System.Windows.Media.Brushes.Yellow,penWeight=3f},
                    new UbiquitousPen{penName="Orange",penColour=System.Windows.Media.Brushes.Orange,penWeight=5f},
                    new UbiquitousPen{penName="White",penColour=System.Windows.Media.Brushes.White,penWeight=5f}
                };
            populateSlidesAdvanceDictionary();
            currentPen = pens[0];
            PensControl.Items.Clear();
            PensControl.ItemsSource = pens;
            if (shouldWorkaroundClickAdvance)
                foreach (var slide in slides)
                    slide.setClickAdvance(false);
        }
        private void whatever()
        {
            Selector(null, new RoutedEventArgs());
        }

        private bool shouldWorkaroundClickAdvance { get { return presenterView && !pptVersionIs2010; } }
        private bool pptVersionIs2010
        {
            get
            {
                return Double.Parse(ThisAddIn.instance.Application.Version) > 12;
            }
        }
        private void populateSlidesAdvanceDictionary()
        {
            slides.Clear();
            foreach (Slide slide in ThisAddIn.instance.Application.ActivePresentation.Slides)
            {
                slides.Add(new slideIndicator(slide.SlideID));
            }
        }
        private void setClickAdvanceOnAllSlides(bool state)
        {
            if (shouldWorkaroundClickAdvance)
                foreach (var slide in slides)
                    slide.setClickAdvance(state);
        }
        private void ReFocusPresenter()
        {
            WindowsInteropFunctions.BringAppropriateViewToFront();
        }
        private void Pen(object sender, RoutedEventArgs e)
        {
            setClickAdvanceOnAllSlides(false);
            currentPen = pens.Where(c => c.penName.Equals(((FrameworkElement)sender).Tag.ToString())).First();
            ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.PointerColor.RGB = currentPen.RGBAasInt;
            ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.PointerType = PpSlideShowPointerType.ppSlideShowPointerPen;
            ReFocusPresenter();
        }
        private void Eraser(object sender, RoutedEventArgs e)
        {
            setClickAdvanceOnAllSlides(false);
            ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.PointerType = PpSlideShowPointerType.ppSlideShowPointerEraser;
            ReFocusPresenter();
        }
        private void Selector(object sender, RoutedEventArgs e)
        {
            setClickAdvanceOnAllSlides(true);
            ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.PointerType = PpSlideShowPointerType.ppSlideShowPointerAutoArrow;
            ReFocusPresenter();
        }
        private void EndSlideShow(object sender, RoutedEventArgs e)
        {
            if (ThisAddIn.instance.Application.SlideShowWindows.Count > 0)
                ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.Exit();
            if (this != null)
            {
                try
                {
                    this.Close();
                }
                catch (Exception) { }
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.Exit();
            }
            catch (Exception ex)
            {
            }
        }

        private void pageUp(object sender, ExecutedRoutedEventArgs e)
        {
            ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.Next();
        }
        private void pageDown(object sender, ExecutedRoutedEventArgs e)
        {
            ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.Previous();
        }
        private void closeApplication(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex) { }
        }


        public class UbiquitousPen
        {
            public UbiquitousPen()
            {
            }
            private bool internalupdate = false;
            private DrawingAttributes generatedAttributes { get { return new DrawingAttributes { Color = penColour.Color, Height = (double)penWeight, Width = (double)penWeight, IsHighlighter = false }; } }
            private DrawingAttributes attributes;
            public DrawingAttributes Attributes
            {
                get { return attributes; }
                set
                {
                    attributes = value;
                    internalupdate = true;
                    IsHighlighter = value.IsHighlighter;
                    PenSize = value.Height;
                    ColorValue = value.Color;
                    XAMLColorName = value.Color.ToString();
                    ColorName = value.Color.ToString() + ":" + value.Height.ToString() + ":" + value.IsHighlighter.ToString();
                    internalupdate = false;
                }
            }
            private string colorname;
            public string ColorName
            {
                get { return colorname; }
                set
                {
                    colorname = value;
                    if (internalupdate)
                        return;
                    internalupdate = true;
                    attributes = new DrawingAttributes()
                    {
                        Color = ColorValue,
                        Height = PenSize,
                        IsHighlighter = ishighlighter,
                        Width = PenSize
                    };
                    internalupdate = false;
                }
            }
            private string xamlcolorname;
            public string XAMLColorName { get; set; }
            private System.Windows.Media.Color colorvalue { get; set; }
            public System.Windows.Media.Color ColorValue
            {
                get { return colorvalue; }
                set
                {
                    colorvalue = value;
                    if (internalupdate)
                        return;
                    internalupdate = true;
                    attributes = new DrawingAttributes()
                    {
                        Color = value,
                        Height = PenSize,
                        IsHighlighter = ishighlighter,
                        Width = PenSize
                    };
                    internalupdate = false;
                }
            }
            private double pensize { get; set; }
            public double PenSize
            {
                get { return pensize; }
                set
                {
                    pensize = value;
                    if (internalupdate)
                        return;
                    internalupdate = true;
                    attributes = new DrawingAttributes()
                    {
                        Color = ColorValue,
                        Height = value,
                        IsHighlighter = ishighlighter,
                        Width = value
                    };
                    internalupdate = false;
                }
            }
            private bool ishighlighter;
            public bool IsHighlighter
            {
                get { return ishighlighter; }
                set
                {
                    ishighlighter = value;
                    if (internalupdate)
                        return;
                    internalupdate = true;
                    attributes = new DrawingAttributes()
                    {
                        Color = ColorValue,
                        Height = PenSize,
                        IsHighlighter = value,
                        Width = PenSize
                    };
                    internalupdate = false;
                }
            }

            public string penName { get; set; }
            public float penWeight { get; set; }
            public SolidColorBrush penColour { get; set; }
            public int R { get { return penColour.Color.R; } }
            public int G { get { return penColour.Color.G; } }
            public int B { get { return penColour.Color.B; } }
            public int A { get { return penColour.Color.A; } }
            public int RGBAasInt { get { return ColorTranslator.ToOle(System.Drawing.Color.FromArgb(A, R, G, B)); } }

            public StrokeCollection DrawnPenPreviewStroke
            {
                get
                {
                    return new StrokeCollection(
                        new[]{
                            new Stroke(
                                new StylusPointCollection(
                                    new StylusPoint[]{
                                        new StylusPoint(30.6666666666667,90,0.5f),
                                        new StylusPoint(32.6666666666667,91.3333333333333,0.5f),
                                        new StylusPoint(33.6666666666667,91.6666666666667,0.5f),
                                        new StylusPoint(35,92,0.5f),
                                        new StylusPoint(35.6666666666667,92.3333333333333,0.5f),
                                        new StylusPoint(36.3333333333333,92.6666666666667,0.5f),
                                        new StylusPoint(37.3333333333333,93,0.5f),
                                        new StylusPoint(38,93.3333333333333,0.5f),
                                        new StylusPoint(39,93.6666666666667,0.5f),
                                        new StylusPoint(40.3333333333333,94,0.5f),
                                        new StylusPoint(41.3333333333333,94.3333333333333,0.5f),
                                        new StylusPoint(42.6666666666667,94.3333333333333,0.5f),
                                        new StylusPoint(43.6666666666667,94.6666666666667,0.5f),
                                        new StylusPoint(45.3333333333333,95,0.5f),
                                        new StylusPoint(46.6666666666667,95.3333333333333,0.5f),
                                        new StylusPoint(48,95.3333333333333,0.5f),
                                        new StylusPoint(49.3333333333333,95.3333333333333,0.5f),
                                        new StylusPoint(51,95.6666666666667,0.5f),
                                        new StylusPoint(52.6666666666667,95.6666666666667,0.5f),
                                        new StylusPoint(54,95.3333333333333,0.5f),
                                        new StylusPoint(55.6666666666667,95.3333333333333,0.5f),
                                        new StylusPoint(57.3333333333333,95,0.5f),
                                        new StylusPoint(59,94.6666666666667,0.5f),
                                        new StylusPoint(60.6666666666667,94.3333333333333,0.5f),
                                        new StylusPoint(62.3333333333333,94,0.5f),
                                        new StylusPoint(64.3333333333333,93.3333333333333,0.5f),
                                        new StylusPoint(65.6666666666667,93,0.5f),
                                        new StylusPoint(67.3333333333333,92.3333333333333,0.5f),
                                        new StylusPoint(69,91.6666666666667,0.5f),
                                        new StylusPoint(70.6666666666667,91,0.5f),
                                        new StylusPoint(72,90.3333333333333,0.5f),
                                        new StylusPoint(73.6666666666667,89.3333333333333,0.5f),
                                        new StylusPoint(75,88.6666666666667,0.5f),
                                        new StylusPoint(76.3333333333333,87.6666666666667,0.5f),
                                        new StylusPoint(77.3333333333333,86.6666666666667,0.5f),
                                        new StylusPoint(78.6666666666667,85.6666666666667,0.5f),
                                        new StylusPoint(79.6666666666667,84.6666666666667,0.5f),
                                        new StylusPoint(80.6666666666667,83.6666666666667,0.5f),
                                        new StylusPoint(81.6666666666667,82.3333333333333,0.5f),
                                        new StylusPoint(82.6666666666667,81,0.5f),
                                        new StylusPoint(83.3333333333333,80,0.5f),
                                        new StylusPoint(84,78.6666666666667,0.5f),
                                        new StylusPoint(84.3333333333333,77.3333333333333,0.5f),
                                        new StylusPoint(85,76,0.5f),
                                        new StylusPoint(85.3333333333333,74.6666666666667,0.5f),
                                        new StylusPoint(85.6666666666667,73,0.5f),
                                        new StylusPoint(86,71.6666666666667,0.5f),
                                        new StylusPoint(86,70.3333333333333,0.5f),
                                        new StylusPoint(86,69,0.5f),
                                        new StylusPoint(86,67.6666666666667,0.5f),
                                        new StylusPoint(85.6666666666667,66.3333333333333,0.5f),
                                        new StylusPoint(85.6666666666667,65,0.5f),
                                        new StylusPoint(85.3333333333333,63.6666666666667,0.5f),
                                        new StylusPoint(85,62.3333333333333,0.5f),
                                        new StylusPoint(84.3333333333333,61,0.5f),
                                        new StylusPoint(83.6666666666667,59.6666666666667,0.5f),
                                        new StylusPoint(83,58.6666666666667,0.5f),
                                        new StylusPoint(82.3333333333333,57.3333333333333,0.5f),
                                        new StylusPoint(81.6666666666667,56,0.5f),
                                        new StylusPoint(80.6666666666667,55,0.5f),
                                        new StylusPoint(79.6666666666667,53.6666666666667,0.5f),
                                        new StylusPoint(78.6666666666667,52.6666666666667,0.5f),
                                        new StylusPoint(77.3333333333333,51.3333333333333,0.5f),
                                        new StylusPoint(76,50.3333333333333,0.5f),
                                        new StylusPoint(74.6666666666667,49.3333333333333,0.5f),
                                        new StylusPoint(73.3333333333333,48.3333333333333,0.5f),
                                        new StylusPoint(71.6666666666667,47.3333333333333,0.5f),
                                        new StylusPoint(70,46.3333333333333,0.5f),
                                        new StylusPoint(68.3333333333333,45.6666666666667,0.5f),
                                        new StylusPoint(66.6666666666667,45,0.5f),
                                        new StylusPoint(65,44.3333333333333,0.5f),
                                        new StylusPoint(63,43.6666666666667,0.5f),
                                        new StylusPoint(61.3333333333333,43.3333333333333,0.5f),
                                        new StylusPoint(59.3333333333333,43,0.5f),
                                        new StylusPoint(57.3333333333333,42.6666666666667,0.5f),
                                        new StylusPoint(55.3333333333333,42.3333333333333,0.5f),
                                        new StylusPoint(53.3333333333333,42.3333333333333,0.5f),
                                        new StylusPoint(51,42,0.5f),
                                        new StylusPoint(49,42.3333333333333,0.5f),
                                        new StylusPoint(46.6666666666667,42.3333333333333,0.5f),
                                        new StylusPoint(44.3333333333333,42.6666666666667,0.5f),
                                        new StylusPoint(42.3333333333333,43,0.5f),
                                        new StylusPoint(40,43.6666666666667,0.5f),
                                        new StylusPoint(37.6666666666667,44.3333333333333,0.5f),
                                        new StylusPoint(35.3333333333333,45,0.5f),
                                        new StylusPoint(33,46,0.5f),
                                        new StylusPoint(30.6666666666667,47.3333333333333,0.5f),
                                        new StylusPoint(28.3333333333333,48.6666666666667,0.5f),
                                        new StylusPoint(26,50,0.5f),
                                        new StylusPoint(24,51.6666666666667,0.5f),
                                        new StylusPoint(21.6666666666667,53.6666666666667,0.5f),
                                        new StylusPoint(19.3333333333333,55.3333333333333,0.5f),
                                        new StylusPoint(17,57.3333333333333,0.5f),
                                    }
                                ),
                                (attributes!=null)?attributes:new DrawingAttributes
                                {
                                    Color=Colors.Black,
                                    Height=2,
                                    Width=2,
                                    IsHighlighter=false
                                }
                            )
                        }
                    );
                }
            }
            public StrokeCollection DrawnHighlighterPreviewStroke
            {
                get
                {
                    return new StrokeCollection(
                        new[]{
                            new Stroke(
                                new StylusPointCollection(
                                    new StylusPoint[]{
                                        new StylusPoint(17.6666666666667,86,0.5f),
                                        new StylusPoint(18,87.3333333333333,0.5f),
                                        new StylusPoint(18,87.6666666666667,0.5f),
                                        new StylusPoint(18.3333333333333,87.6666666666667,0.5f),
                                        new StylusPoint(18.6666666666667,87.6666666666667,0.5f),
                                        new StylusPoint(19.3333333333333,88.3333333333333,0.5f),
                                        new StylusPoint(19.6666666666667,88.3333333333333,0.5f),
                                        new StylusPoint(20,88.6666666666667,0.5f),
                                        new StylusPoint(20.3333333333333,89,0.5f),
                                        new StylusPoint(21,89.3333333333333,0.5f),
                                        new StylusPoint(21.6666666666667,89.6666666666667,0.5f),
                                        new StylusPoint(22.3333333333333,90,0.5f),
                                        new StylusPoint(23,90.6666666666667,0.5f),
                                        new StylusPoint(23.6666666666667,91,0.5f),
                                        new StylusPoint(24.6666666666667,91.3333333333333,0.5f),
                                        new StylusPoint(25.6666666666667,91.6666666666667,0.5f),
                                        new StylusPoint(26.6666666666667,92,0.5f),
                                        new StylusPoint(27.6666666666667,92.6666666666667,0.5f),
                                        new StylusPoint(28.6666666666667,93,0.5f),
                                        new StylusPoint(30,93.6666666666667,0.5f),
                                        new StylusPoint(31.3333333333333,94,0.5f),
                                        new StylusPoint(32.3333333333333,94.6666666666667,0.5f),
                                        new StylusPoint(33.6666666666667,95,0.5f),
                                        new StylusPoint(35.3333333333333,95.6666666666667,0.5f),
                                        new StylusPoint(36.6666666666667,96,0.5f),
                                        new StylusPoint(38.3333333333333,96.3333333333333,0.5f),
                                        new StylusPoint(39.6666666666667,96.6666666666667,0.5f),
                                        new StylusPoint(41.3333333333333,97,0.5f),
                                        new StylusPoint(43,97.3333333333333,0.5f),
                                        new StylusPoint(44.6666666666667,97.6666666666667,0.5f),
                                        new StylusPoint(46.6666666666667,97.6666666666667,0.5f),
                                        new StylusPoint(48.3333333333333,97.6666666666667,0.5f),
                                        new StylusPoint(50.3333333333333,98,0.5f),
                                        new StylusPoint(52,98,0.5f),
                                        new StylusPoint(54,97.6666666666667,0.5f),
                                        new StylusPoint(56,97.6666666666667,0.5f),
                                        new StylusPoint(57.6666666666667,97.3333333333333,0.5f),
                                        new StylusPoint(59.6666666666667,97,0.5f),
                                        new StylusPoint(61.3333333333333,96.6666666666667,0.5f),
                                        new StylusPoint(69,94.3333333333333,0.5f),
                                        new StylusPoint(70.6666666666667,93.3333333333333,0.5f),
                                        new StylusPoint(74,91.6666666666667,0.5f),
                                        new StylusPoint(75.6666666666667,90.6666666666667,0.5f),
                                        new StylusPoint(77,89.3333333333333,0.5f),
                                        new StylusPoint(78.3333333333333,88,0.5f),
                                        new StylusPoint(79.6666666666667,86.6666666666667,0.5f),
                                        new StylusPoint(81,85.3333333333333,0.5f),
                                        new StylusPoint(82,84,0.5f),
                                        new StylusPoint(83.3333333333333,82.3333333333333,0.5f),
                                        new StylusPoint(84,80.6666666666667,0.5f),
                                        new StylusPoint(85,79,0.5f),
                                        new StylusPoint(85.6666666666667,77.3333333333333,0.5f),
                                        new StylusPoint(86.3333333333333,75.6666666666667,0.5f),
                                        new StylusPoint(87,73.6666666666667,0.5f),
                                        new StylusPoint(87.3333333333333,72,0.5f),
                                        new StylusPoint(87.3333333333333,70,0.5f),
                                        new StylusPoint(87.3333333333333,68,0.5f),
                                        new StylusPoint(87.3333333333333,66,0.5f),
                                        new StylusPoint(87.3333333333333,64.3333333333333,0.5f),
                                        new StylusPoint(86.6666666666667,62.3333333333333,0.5f),
                                        new StylusPoint(86.3333333333333,60.3333333333333,0.5f),
                                        new StylusPoint(85.6666666666667,58.6666666666667,0.5f),
                                        new StylusPoint(85,57,0.5f),
                                        new StylusPoint(84,55.3333333333333,0.5f),
                                        new StylusPoint(83,53.6666666666667,0.5f),
                                        new StylusPoint(82,52,0.5f),
                                        new StylusPoint(81,50.6666666666667,0.5f),
                                        new StylusPoint(79.6666666666667,49,0.5f),
                                        new StylusPoint(78.3333333333333,47.6666666666667,0.5f),
                                        new StylusPoint(76.6666666666667,46.3333333333333,0.5f),
                                        new StylusPoint(75,45,0.5f),
                                        new StylusPoint(73.3333333333333,43.6666666666667,0.5f),
                                        new StylusPoint(71.6666666666667,42.3333333333333,0.5f),
                                        new StylusPoint(69.6666666666667,41.3333333333333,0.5f),
                                        new StylusPoint(67.6666666666667,40.3333333333333,0.5f),
                                        new StylusPoint(65.6666666666667,39.3333333333333,0.5f),
                                        new StylusPoint(63.6666666666667,38.6666666666667,0.5f),
                                        new StylusPoint(61.6666666666667,37.6666666666667,0.5f),
                                        new StylusPoint(59.6666666666667,37,0.5f),
                                        new StylusPoint(57.3333333333333,36.6666666666667,0.5f),
                                        new StylusPoint(55.3333333333333,36,0.5f),
                                        new StylusPoint(53,35.6666666666667,0.5f),
                                        new StylusPoint(51,35.3333333333333,0.5f),
                                        new StylusPoint(48.6666666666667,35.3333333333333,0.5f),
                                        new StylusPoint(46.6666666666667,35,0.5f),
                                        new StylusPoint(44.3333333333333,35,0.5f),
                                        new StylusPoint(42.3333333333333,35,0.5f),
                                        new StylusPoint(40,35,0.5f),
                                        new StylusPoint(38,35.3333333333333,0.5f),
                                        new StylusPoint(35.6666666666667,35.6666666666667,0.5f),
                                        new StylusPoint(33.6666666666667,36,0.5f),
                                        new StylusPoint(31.3333333333333,36.6666666666667,0.5f),
                                        new StylusPoint(29.3333333333333,37.3333333333333,0.5f),
                                        new StylusPoint(27.3333333333333,38,0.5f),
                                        new StylusPoint(25.3333333333333,38.6666666666667,0.5f),
                                        new StylusPoint(23.6666666666667,39.6666666666667,0.5f),
                                        new StylusPoint(21.6666666666667,40.6666666666667,0.5f),
                                        new StylusPoint(19.6666666666667,42,0.5f),
                                        new StylusPoint(18,43.3333333333333,0.5f),
                                        new StylusPoint(16.3333333333333,44.6666666666667,0.5f),
                                        new StylusPoint(14.6666666666667,46,0.5f),
                                        new StylusPoint(13,47.3333333333333,0.5f),
                                        new StylusPoint(11.6666666666667,48.6666666666667,0.5f),
                                    }
                                ),
                                (attributes!=null)?attributes:new DrawingAttributes
                                {
                                    Color=Colors.Black,
                                    Height=2,
                                    Width=2,
                                    IsHighlighter=false
                                }
                            )
                        }
                    );
                }
            }
            public PointCollection HighlighterPreviewPoints
            {
                get
                {
                    return new PointCollection{
                        new Point(400,0),
                        new Point(222,0),
                        new Point(167,70),
                        new Point(167,74),
                        new Point(158,74),
                        new Point(154,79),
                        new Point(130,108),
                        new Point(127,106),
                        new Point(115,123),
                        new Point(119,130),
                        new Point(125,155),
                        new Point(125,178),
                        new Point(122,210),
                        new Point(112,239),
                        new Point(98,261),
                        new Point(74,292),
                        new Point(73,296),
                        new Point(74,306),
                        new Point(60,321),
                        new Point(49,341),
                        new Point(48,345),
                        new Point(50,347),
                        new Point(86,362),
                        new Point(106,342),
                        new Point(114,336),
                        new Point(125,335),
                        new Point(163,295),
                        new Point(204,271),
                        new Point(252,261),
                        new Point(274,262),
                        new Point(282,266),
                        new Point(297,250),
                        new Point(296,249),
                        new Point(323,217),
                        new Point(322,215),
                        new Point(326,210),
                        new Point(330,209),
                        new Point(300,121),
                        new Point(400,0)
                    };
                }
            }
            public PointCollection BrushPreviewPoints
            {
                get
                {
                    return new PointCollection{
                        new Point(100,0),
                        new Point(71,0),
                        new Point(62,12),
                        new Point(62,20),
                        new Point(48,47),
                        new Point(37,65),
                        new Point(37,69),
                        new Point(31,83),
                        new Point(29,89),
                        new Point(30,90),
                        new Point(32,91),
                        new Point(37,85),
                        new Point(48,75),
                        new Point(52,75),
                        new Point(77,43),
                        new Point(91,32),
                        new Point(100,21),
                        new Point(100,0)
                    };
                }
            }

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Selector(SelectionButton, new RoutedEventArgs());
        }
    }
    class slideIndicator
    {
        public slideIndicator(int Id)
        {
            slideId = Id;
        }
        public int slideId { get; private set; }
        public Slide slide
        {
            get
            {
                return ThisAddIn.instance.Application.ActivePresentation.Slides.FindBySlideID(slideId);
            }
        }
        public bool isCurrentSlide { get { return slide.SlideIndex == ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.CurrentShowPosition; } }
        public bool clickAdvance { get { return slide.SlideShowTransition.AdvanceOnClick == Microsoft.Office.Core.MsoTriState.msoTrue; } }
        public void setClickAdvance(bool state)
        {
            if (clickAdvance != state)
            {
                var currentPosition = ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.GetClickIndex();
                slide.SlideShowTransition.AdvanceOnClick = state ? Microsoft.Office.Core.MsoTriState.msoTrue : Microsoft.Office.Core.MsoTriState.msoFalse;
                if (isCurrentSlide)
                {
                    ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.GotoClick(currentPosition);
                    ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.State = PpSlideShowState.ppSlideShowRunning;
                }
            }
        }
    }
}