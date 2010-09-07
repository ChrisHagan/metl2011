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
    public partial class SimplePenWindow : Window
    {
        public List<UbiquitousPen> pens;
        private UbiquitousPen currentPen;
        public SimplePenWindow()
        {
            InitializeComponent();
            DisableClickAdvance();
            pens = new List<UbiquitousPen> 
                {
                    new UbiquitousPen{penName="thinBlack",penColour=System.Windows.Media.Brushes.Black,penWeight=1.5f},
                    new UbiquitousPen{penName="thinRed",penColour=System.Windows.Media.Brushes.Red,penWeight=1.5f},
                    //new UbiquitousPen{penName="thinYellow",penColour=System.Windows.Media.Brushes.Yellow,penWeight=1.5f},
                    new UbiquitousPen{penName="thinBlue",penColour=System.Windows.Media.Brushes.Blue,penWeight=1.5f},
                    //new UbiquitousPen{penName="thinGreen",penColour=System.Windows.Media.Brushes.Green,penWeight=1.5f},
                    //new UbiquitousPen{penName="thinDarkBlue",penColour=System.Windows.Media.Brushes.DarkBlue,penWeight=1.5f},
                    new UbiquitousPen{penName="medRed",penColour=System.Windows.Media.Brushes.Red,penWeight=3f},
                    new UbiquitousPen{penName="medBlue",penColour=System.Windows.Media.Brushes.Blue,penWeight=3f},
                    new UbiquitousPen{penName="medwhite",penColour=System.Windows.Media.Brushes.Yellow,penWeight=3f},
                    new UbiquitousPen{penName="thinWhite",penColour=System.Windows.Media.Brushes.White,penWeight=1.5f}
                };
            currentPen = pens[0];
            PensControl.Items.Clear();
            PensControl.ItemsSource = pens;
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
        private void ReFocusPresenter()
        {
            this.Focus();
            ThisAddIn.instance.Application.SlideShowWindows[1].Activate();
        }
        private void Pen(object sender, RoutedEventArgs e)
        {
            DisableClickAdvance();
            currentPen = pens.Where(c => c.penName.Equals(((FrameworkElement)sender).Tag.ToString())).First();
            ThisAddIn.instance.Application.ActivePresentation.SlideShowWindow.View.PointerColor.RGB = currentPen.RGBAasInt;
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
}