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

namespace PowerpointJabber
{
    public class SlideThumbnail
    {
        public SlideThumbnail()
        {
        }
        public ImageSource thumbnail { get; set; }
        public int slideNumber { get; set; }
    }
    public partial class SimpleSlideShowWindow : Window
    {
        public ObservableCollection<SlideThumbnail> slideThumbs;
        public SimpleSlideShowWindow()
        {
            InitializeComponent();
            DisableClickAdvance();
            slideThumbs = new ObservableCollection<SlideThumbnail>();
            GenerateThumbnails();
            SlideViewer.Items.Clear();
            SlideViewer.ItemsSource = slideThumbs;
        }
        private void moveToSelectedSlide(object sender, RoutedEventArgs e)
        {
            var origin = ((FrameworkElement)sender);
            if (origin.Tag != null)
                ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.GotoSlide((int)(Int32.Parse(origin.Tag.ToString())), Microsoft.Office.Core.MsoTriState.msoTrue);
        }
        private void GenerateThumbnails()
        {
            foreach (Slide slide in ThisAddIn.instance.Application.ActivePresentation.Slides)
            {

                var filename = System.IO.Directory.GetCurrentDirectory().ToString() + "\\pptSlideThumbnail" + slide.SlideNumber + ".png";
                try
                {
                    slide.Export(filename, "PNG", 100, 100);
                }
                catch (Exception ex)
                {
                }
                var image = ((ImageSource)new ImageSourceConverter().ConvertFromString(filename));
                slideThumbs.Add(new SlideThumbnail { thumbnail = image, slideNumber = slide.SlideNumber });
            }
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
            ThisAddIn.instance.Application.SlideShowWindows[1].Activate();
        }
        private void Pen(object sender, RoutedEventArgs e)
        {
            DisableClickAdvance();
            var colour = ((FrameworkElement)sender).Tag.ToString();
            int colourAsInt = ColorTranslator.ToOle(System.Drawing.Color.FromArgb(255, 0, 0, 0));
            switch (colour)
            {
                case "red":
                    colourAsInt = ColorTranslator.ToOle(System.Drawing.Color.FromArgb(255, 255, 0, 0));
                    break;
                case "black":
                    colourAsInt = ColorTranslator.ToOle(System.Drawing.Color.FromArgb(255, 0, 0, 0));
                    break;
                case "blue":
                    colourAsInt = ColorTranslator.ToOle(System.Drawing.Color.FromArgb(255, 0, 0, 255));
                    break;
                case "yellow":
                    colourAsInt = ColorTranslator.ToOle(System.Drawing.Color.FromArgb(255, 255, 255, 0));
                    break;
            }
            ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.PointerColor.RGB = colourAsInt;
            ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.PointerType = PpSlideShowPointerType.ppSlideShowPointerPen;
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
    }
}
