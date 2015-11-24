using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using Microsoft.Windows.Controls.Ribbon;
using SandRibbon.Components;
using SandRibbon.Components.Utility;
using SandRibbon.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SandRibbon.Pages.Collaboration
{
    public partial class RibbonCollaborationPage : Page
    {
        protected NetworkController networkController;
        protected ConversationDetails details;
        protected string conversationJid;
        protected Slide slide;
        public RibbonCollaborationPage(NetworkController _networkController/*, ConversationDetails _details, Slide slide*/)
        {
            networkController = _networkController;
            //details = _details;
            details = ConversationDetails.Empty;
            InitializeComponent();
            //DataContext = slide;
            slide = Slide.Empty;
            DataContext = slide;            
            //loadFonts();
            InitializeComponent();
            /*
            fontFamily.ItemsSource = fontList;
            fontSize.ItemsSource = fontSizes;
            fontSize.SelectedIndex = 0;
            fontSize.SelectionChanged += fontSizeSelected;
            fontFamily.SelectionChanged += fontFamilySelected;
            */
            Commands.SetLayer.RegisterCommandToDispatcher<string>(new DelegateCommand<string>(SetLayer));
            /*
            Commands.TextboxFocused.RegisterCommandToDispatcher(new DelegateCommand<TextInformation>(update));
            //This is used only when a text box is selected
            //A seperate command is used because TextBoxFocused command calls updateprivacy method which is not needed when a text box is selected
            Commands.TextboxSelected.RegisterCommandToDispatcher(new DelegateCommand<TextInformation>(update));
            Commands.ToggleBold.RegisterCommand(new DelegateCommand<object>(togglebold));
            Commands.ToggleItalic.RegisterCommand(new DelegateCommand<object>(toggleItalic));
            Commands.ToggleUnderline.RegisterCommand(new DelegateCommand<object>(toggleUnderline));
            */
            Commands.FitToView.RegisterCommand(new DelegateCommand<object>(fitToView, canFitToView));
            Commands.OriginalView.RegisterCommand(new DelegateCommand<object>(originalView, canOriginalView));
            Commands.ZoomIn.RegisterCommand(new DelegateCommand<object>(doZoomIn, canZoomIn));
            Commands.ZoomOut.RegisterCommand(new DelegateCommand<object>(doZoomOut, canZoomOut));
            Commands.SetZoomRect.RegisterCommandToDispatcher(new DelegateCommand<Rect>(SetZoomRect));

            //adding these as a workaround while we're doing singletons of this page and re-using it
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>((convJid) => conversationJid = convJid));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>((convDetails) => details = convDetails));
            Commands.MoveToCollaborationPage.RegisterCommand(new DelegateCommand<int>((slideId) =>
            {
                var newSlide = details.Slides.Find(s => s.id == slideId);
                if (newSlide != null)
                {
                    slide = newSlide;
                    DataContext = newSlide;
                }
            }));
            this.Loaded += (ps, pe) =>
            {
                scroll.ScrollChanged += (s, e) =>
                {
                    Commands.RequerySuggested(Commands.ZoomIn, Commands.ZoomOut, Commands.OriginalView, Commands.FitToView, Commands.FitToPageWidth);
                };
                /*
                //watching the navigation away from this page so that we can do cleanup.  This won't be necessary until we stop using a singleton on the network controller.
                NavigationService.Navigated += (s, e) =>
                {
                    if ((Page)e.Content != this)
                    {
                        Console.WriteLine("navigatedAwayFromthis");
                    }

                };
                */
                /*
                //firing these, until we work out the XAML binding up and down the chain.
                Commands.JoinConversation.Execute(details.Jid);
                Commands.MoveToCollaborationPage.Execute(slide.id);
                Commands.SetContentVisibility.Execute(ContentFilterVisibility.defaultVisibilities);
                */
            };
        }

        private void SetLayer(string layer)
        {
            foreach (var group in new UIElement[] { inkGroup, textGroup, imageGroup })
            {
                group.Visibility = Visibility.Collapsed;
            }
            switch (layer)
            {
                case "Sketch": inkGroup.Visibility = Visibility.Visible; break;
                case "Text": textGroup.Visibility = Visibility.Visible; break;
                case "Image": imageGroup.Visibility = Visibility.Visible; break;
            }
        }

        private void AddPrivacyButton(PrivacyToggleButton.PrivacyToggleButtonInfo info)
        {
            Viewbox viewbox = null;
            UIElement container = null;
            GetViewboxAndCanvasFromTarget(info.AdornerTarget, out viewbox, out container);
            Dispatcher.adoptAsync(() =>
            {
                var adornerRect = new Rect(container.TranslatePoint(info.ElementBounds.TopLeft, viewbox), container.TranslatePoint(info.ElementBounds.BottomRight, viewbox));
                if (LessThan(adornerRect.Right, 0, 0.001) || GreaterThan(adornerRect.Right, viewbox.ActualWidth, 0.001)
                    || LessThan(adornerRect.Top, 0, 0.001) || GreaterThan(adornerRect.Top, viewbox.ActualHeight, 0.001))
                    return;
                var adornerLayer = AdornerLayer.GetAdornerLayer(viewbox);
                adornerLayer.Add(new UIAdorner(viewbox, new PrivacyToggleButton(info, adornerRect)));
            });
        }

        private bool LessThan(double val1, double val2, double tolerance)
        {
            var difference = val2 * tolerance;
            return val1 < (val2 - difference) && val1 < (val2 + difference);
        }
        private bool GreaterThan(double val1, double val2, double tolerance)
        {
            var difference = val2 * tolerance;
            return val1 > (val2 - difference) && val1 > (val2 + difference);
        }

        private Adorner[] GetPrivacyAdorners(Viewbox viewbox, out AdornerLayer adornerLayer)
        {
            adornerLayer = AdornerLayer.GetAdornerLayer(viewbox);
            if (adornerLayer == null)
                return null;

            return adornerLayer.GetAdorners(viewbox);
        }

        private void UpdatePrivacyAdorners(string targetName)
        {
            if (RemovePrivacyAdorners(targetName))
                try
                {
                    var lastValue = Commands.AddPrivacyToggleButton.LastValue();
                    if (lastValue != null)
                        AddPrivacyButton((PrivacyToggleButton.PrivacyToggleButtonInfo)lastValue);
                }
                catch (NotSetException) { }
        }

        private void GetViewboxAndCanvasFromTarget(string targetName, out Viewbox viewbox, out UIElement container)
        {
            if (targetName == "presentationSpace")
            {
                viewbox = canvasViewBox;
                container = canvas;
                return;
            }
            if (targetName == "notepad")
            {
                viewbox = notesViewBox;
                container = privateNotes;
                return;
            }

            throw new ArgumentException(string.Format("Specified target {0} does not match a declared ViewBox", targetName));
        }

        private bool RemovePrivacyAdorners(string targetName)
        {
            Viewbox viewbox;
            UIElement container;
            GetViewboxAndCanvasFromTarget(targetName, out viewbox, out container);

            bool hasAdorners = false;
            AdornerLayer adornerLayer;
            var adorners = GetPrivacyAdorners(viewbox, out adornerLayer);
            Dispatcher.adopt(() =>
            {
                if (adorners != null && adorners.Count() > 0)
                {
                    hasAdorners = true;
                    foreach (var adorner in adorners)
                        adornerLayer.Remove(adorner);
                }
            });

            return hasAdorners;
        }

        private void zoomConcernedControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePrivacyAdorners(adornerScroll.Target);
            BroadcastZoom();
        }

        private void scroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            UpdatePrivacyAdorners(adornerScroll.Target);
            BroadcastZoom();
        }

        private void BroadcastZoom()
        {
            var currentZoomHeight = scroll.ActualHeight / canvasViewBox.ActualHeight;
            var currentZoomWidth = scroll.ActualWidth / canvasViewBox.ActualWidth;
            var currentZoom = Math.Max(currentZoomHeight, currentZoomWidth);
            Commands.ZoomChanged.Execute(currentZoom);
        }

        private void notepadSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePrivacyAdorners(notesAdornerScroll.Target);
            BroadcastZoom();
        }

        private void notepadScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            UpdatePrivacyAdorners(notesAdornerScroll.Target);
            BroadcastZoom();
        }
        private void RibbonApplicationMenuItem_SearchConversations_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(networkController.conversationSearchPage);
        }
        private void RibbonApplicationMenuItem_ConversationOverview_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ConversationOverviewPage(networkController, details));
        }
        private bool canZoomIn(object sender)
        {
            return !(scroll == null) && details != ConversationDetails.Empty;
        }
        private bool canZoomOut(object sender)
        {
            var result = false;
            if (scroll == null)
                result = false;
            else
            {
                var cvHeight = adornerGrid.ActualHeight;
                var cvWidth = adornerGrid.ActualWidth;
                var cvRatio = cvWidth / cvHeight;
                bool hTrue = scroll.ViewportWidth < scroll.ExtentWidth;
                bool vTrue = scroll.ViewportHeight < scroll.ExtentHeight;
                var scrollRatio = scroll.ActualWidth / scroll.ActualHeight;
                if (scrollRatio > cvRatio)
                {
                    result = hTrue;
                }
                if (scrollRatio < cvRatio)
                {
                    result = vTrue;
                }
                result = (hTrue || vTrue) && details != ConversationDetails.Empty;
            }
            return result;
        }

        private void doZoomIn(object sender)
        {
            var ZoomValue = 0.9;
            var scrollHOffset = scroll.HorizontalOffset;
            var scrollVOffset = scroll.VerticalOffset;
            var cvHeight = adornerGrid.ActualHeight;
            var cvWidth = adornerGrid.ActualWidth;
            var cvRatio = cvWidth / cvHeight;
            double newWidth = 0;
            double newHeight = 0;
            double oldWidth = scroll.ActualWidth;
            double oldHeight = scroll.ActualHeight;
            var scrollRatio = oldWidth / oldHeight;
            if (scrollRatio > cvRatio)
            {
                newWidth = scroll.ActualWidth * ZoomValue;
                if (newWidth > scroll.ExtentWidth)
                    newWidth = scroll.ExtentWidth;
                scroll.Width = newWidth;
                newHeight = newWidth / cvRatio;
                if (newHeight > scroll.ExtentHeight)
                    newHeight = scroll.ExtentHeight;
                scroll.Height = newHeight;
            }
            if (scrollRatio < cvRatio)
            {
                newHeight = scroll.ActualHeight * ZoomValue;
                if (newHeight > scroll.ExtentHeight)
                    newHeight = scroll.ExtentHeight;
                scroll.Height = newHeight;
                newWidth = newHeight * cvRatio;
                if (newWidth > scroll.ExtentWidth)
                    newWidth = scroll.ExtentWidth;
                scroll.Width = newWidth;
            }
            if (scrollRatio == cvRatio)
            {
                newHeight = scroll.ActualHeight * ZoomValue;
                if (newHeight > scroll.ExtentHeight)
                    newHeight = scroll.ExtentHeight;
                scroll.Height = newHeight;
                newWidth = scroll.ActualWidth * ZoomValue;
                if (newWidth > scroll.ExtentWidth)
                    newWidth = scroll.ExtentWidth;
                scroll.Width = newWidth;
            }
            scroll.ScrollToHorizontalOffset(scrollHOffset + ((oldWidth - newWidth) / 2));
            scroll.ScrollToVerticalOffset(scrollVOffset + ((oldHeight - newHeight) / 2));
        }
        private void doZoomOut(object sender)
        {
            var ZoomValue = 1.1;
            var scrollHOffset = scroll.HorizontalOffset;
            var scrollVOffset = scroll.VerticalOffset;
            var cvHeight = adornerGrid.ActualHeight;
            var cvWidth = adornerGrid.ActualWidth;
            var cvRatio = cvWidth / cvHeight;
            var scrollRatio = scroll.ActualWidth / scroll.ActualHeight;
            double newWidth = 0;
            double newHeight = 0;
            double oldWidth = scroll.ActualWidth;
            double oldHeight = scroll.ActualHeight;
            if (scrollRatio > cvRatio)
            {
                newWidth = scroll.ActualWidth * ZoomValue;
                if (newWidth > scroll.ExtentWidth)
                    newWidth = scroll.ExtentWidth;
                scroll.Width = newWidth;
                newHeight = newWidth / cvRatio;
                if (newHeight > scroll.ExtentHeight)
                    newHeight = scroll.ExtentHeight;
                scroll.Height = newHeight;
            }
            if (scrollRatio < cvRatio)
            {
                newHeight = scroll.ActualHeight * ZoomValue;
                if (newHeight > scroll.ExtentHeight)
                    newHeight = scroll.ExtentHeight;
                scroll.Height = newHeight;
                newWidth = newHeight * cvRatio;
                if (newWidth > scroll.ExtentWidth)
                    newWidth = scroll.ExtentWidth;
                scroll.Width = newWidth;
            }
            if (scrollRatio == cvRatio)
            {
                newHeight = scroll.ActualHeight * ZoomValue;
                if (newHeight > scroll.ExtentHeight)
                    newHeight = scroll.ExtentHeight;
                scroll.Height = newHeight;
                newWidth = scroll.ActualWidth * ZoomValue;
                if (newWidth > scroll.ExtentWidth)
                    newWidth = scroll.ExtentWidth;
                scroll.Width = newWidth;
            }
            scroll.ScrollToHorizontalOffset(scrollHOffset + ((oldWidth - newWidth) / 2));
            scroll.ScrollToVerticalOffset(scrollVOffset + ((oldHeight - newHeight) / 2));
        }
        private void SetZoomRect(Rect viewbox)
        {
            scroll.Width = viewbox.Width;
            scroll.Height = viewbox.Height;
            scroll.UpdateLayout();
            scroll.ScrollToHorizontalOffset(viewbox.X);
            scroll.ScrollToVerticalOffset(viewbox.Y);
            //Trace.TraceInformation("ZoomRect changed to X:{0},Y:{1},W:{2},H:{3}", viewbox.X, viewbox.Y, viewbox.Width, viewbox.Height);
        }
        protected bool canFitToView(object _unused)
        {
            return scroll != null && !(double.IsNaN(scroll.Height) && double.IsNaN(scroll.Width) && double.IsNaN(canvas.Height) && double.IsNaN(canvas.Width));
            //return scroll != null && (scroll.Height != double.NaN || scroll.Width != double.NaN || canvas.Height != double.NaN || canvas.Width != double.NaN);
        }
        protected void fitToView(object _unused)
        {
            if (scroll != null)
            {
                scroll.Height = double.NaN;
                scroll.Width = double.NaN;
                canvas.Height = double.NaN;
                canvas.Width = double.NaN;
            }
        }
        protected bool canOriginalView(object _unused)
        {
            return
                scroll != null &&
                details != null &&
                details != ConversationDetails.Empty &&
                Globals.slideDetails != null &&
                Globals.slideDetails != Slide.Empty &&
                scroll.Height != Globals.slideDetails.defaultHeight &&
                scroll.Width != Globals.slideDetails.defaultWidth;
        }
        protected void originalView(object _unused)
        {

            if (scroll != null &&
            details != null &&
            details != ConversationDetails.Empty &&
            Globals.slideDetails != null &&
            Globals.slideDetails != Slide.Empty)
            {
                var currentSlide = Globals.conversationDetails.Slides.Where(s => s.id == Globals.slide).FirstOrDefault();
                if (currentSlide == null || currentSlide.defaultHeight == 0 || currentSlide.defaultWidth == 0) return;
                scroll.Width = currentSlide.defaultWidth;
                scroll.Height = currentSlide.defaultHeight;
                if (canvas != null && canvas.stack != null && !Double.IsNaN(canvas.stack.offsetX) && !Double.IsNaN(canvas.stack.offsetY))
                {
                    scroll.ScrollToHorizontalOffset(Math.Min(scroll.ExtentWidth, Math.Max(0, -canvas.stack.offsetX)));
                    scroll.ScrollToVerticalOffset(Math.Min(scroll.ExtentHeight, Math.Max(0, -canvas.stack.offsetY)));
                }
                else
                {
                    scroll.ScrollToLeftEnd();
                    scroll.ScrollToTop();
                }
            }
        }
    }
}
