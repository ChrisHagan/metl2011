using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components;
using SandRibbon.Components.Utility;
using SandRibbon.Providers;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace SandRibbon.Pages.Collaboration
{
    public partial class CollaborationPage : Page
    {
        int slide;
        public CollaborationPage(int slide)
        {
            InitializeComponent();
            this.slide = slide;           

            //zoom
            Commands.OriginalView.RegisterCommand(new DelegateCommand<object>(OriginalView));            
            Commands.FitToView.RegisterCommand(new DelegateCommand<object>(FitToView));
            Commands.FitToPageWidth.RegisterCommand(new DelegateCommand<object>(FitToPageWidth));            
            Commands.SetZoomRect.RegisterCommandToDispatcher(new DelegateCommand<Rect>(SetZoomRect));            

            //canvas stuff
            Commands.MoveCanvasByDelta.RegisterCommandToDispatcher(new DelegateCommand<Point>(GrabMove));

            //drawer management
            Commands.ToggleFriendsVisibility.RegisterCommand(new DelegateCommand<object>(ToggleFriendsVisibility));
            RHSDrawerDefinition.Width = new GridLength(180);

            //adorners        
            Commands.AddPrivacyToggleButton.RegisterCommand(new DelegateCommand<PrivacyToggleButton.PrivacyToggleButtonInfo>(AddPrivacyButton));
            Commands.RemovePrivacyAdorners.RegisterCommand(new DelegateCommand<string>((target) => { RemovePrivacyAdorners(target); }));

            //page up and down
            PreviewKeyDown += new KeyEventHandler(KeyPressed);

            //mirroring - add yourself to the chain so the target can reference back to you
            Commands.ProxyMirrorPresentationSpace.RegisterCommand(new DelegateCommand<object>(o => Commands.MirrorPresentationSpace.Execute(this)));            
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

        private void notepadSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePrivacyAdorners(notesAdornerScroll.Target);            
        }

        private void notepadScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            UpdatePrivacyAdorners(notesAdornerScroll.Target);
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


        private void KeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.PageUp)
            {
                scroll.ScrollToVerticalOffset(scroll.VerticalOffset - 540);
            }
            if (e.Key == Key.PageDown)
            {
                scroll.ScrollToVerticalOffset(scroll.VerticalOffset + 540);
            }
        }


        private void GrabMove(Point moveDelta)
        {
            if (moveDelta.X != 0)
                scroll.ScrollToHorizontalOffset(scroll.HorizontalOffset + moveDelta.X);
            if (moveDelta.Y != 0)
                scroll.ScrollToVerticalOffset(scroll.VerticalOffset + moveDelta.Y);
            try
            {
                if (moveDelta.X != 0)
                {
                    var HZoomRatio = (scroll.ExtentWidth / scroll.Width);
                    scroll.ScrollToHorizontalOffset(scroll.HorizontalOffset + (moveDelta.X * HZoomRatio));
                }
                if (moveDelta.Y != 0)
                {
                    var VZoomRatio = (scroll.ExtentHeight / scroll.Height);
                    scroll.ScrollToVerticalOffset(scroll.VerticalOffset + moveDelta.Y * VZoomRatio);
                }
            }
            catch (Exception)
            {//out of range exceptions and the like 
            }
        }

        private void BroadcastZoom()
        {
            var currentZoomHeight = scroll.ActualHeight / canvasViewBox.ActualHeight;
            var currentZoomWidth = scroll.ActualWidth / canvasViewBox.ActualWidth;
            var currentZoom = Math.Max(currentZoomHeight, currentZoomWidth);
            Commands.ZoomChanged.Execute(currentZoom);
        }

        private void SetZoomRect(Rect viewbox)
        {
            scroll.Width = viewbox.Width;
            scroll.Height = viewbox.Height;
            scroll.UpdateLayout();
            scroll.ScrollToHorizontalOffset(viewbox.X);
            scroll.ScrollToVerticalOffset(viewbox.Y);
            Trace.TraceInformation("ZoomRect changed to X:{0},Y:{1},W:{2},H:{3}", viewbox.X, viewbox.Y, viewbox.Width, viewbox.Height);
        }
        private void OriginalView(object _unused)
        {
            Trace.TraceInformation("ZoomToOriginalView");
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
        private void FitToView(object _unused)
        {
            scroll.Height = double.NaN;
            scroll.Width = double.NaN;
            canvas.Height = double.NaN;
            canvas.Width = double.NaN;
        }
        private void FitToPageWidth(object _unused)
        {
            if (scroll != null)
            {
                var ratio = adornerGrid.ActualWidth / adornerGrid.ActualHeight;
                scroll.Height = canvas.ActualWidth / ratio;
                scroll.Width = canvas.ActualWidth;
            }
        }

        private void ToggleFriendsVisibility(object unused)
        {
            if (chatGridsplitter.Visibility == Visibility.Visible)
            {
                chatGridsplitter.Visibility = Visibility.Collapsed;
                leftDrawer.Visibility = Visibility.Collapsed;
                LHSSplitterDefinition.Width = new GridLength(0);
                LHSDrawerDefinition.Width = new GridLength(0);
            }
            else
            {
                chatGridsplitter.Visibility = Visibility.Visible;
                leftDrawer.Visibility = Visibility.Visible;
                LHSSplitterDefinition.Width = new GridLength(10);
                LHSDrawerDefinition.Width = new GridLength((columns.ActualWidth - rightDrawer.ActualWidth) / 4);
            }
        }


        private void canZoomIn(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !(scroll == null);
        }
        private void canZoomOut(object sender, CanExecuteRoutedEventArgs e)
        {
            if (scroll == null)
                e.CanExecute = false;
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
                    e.CanExecute = hTrue;
                }
                if (scrollRatio < cvRatio)
                {
                    e.CanExecute = vTrue;
                }
                e.CanExecute = (hTrue || vTrue);
            }
        }

        private void doZoomIn(object sender, ExecutedRoutedEventArgs e)
        {
            Trace.TraceInformation("ZoomIn pressed");
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
        private void doZoomOut(object sender, ExecutedRoutedEventArgs e)
        {
            Trace.TraceInformation("ZoomOut pressed");
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

        private void EnsureConversationTabSelected()
        {
            // ensure the "pages" tab is selected
            Action selectPagesTab = () =>
            {
                if (contentTabs.SelectedIndex != 0)
                    contentTabs.SelectedIndex = 0;
            };
            Dispatcher.Invoke(selectPagesTab, System.Windows.Threading.DispatcherPriority.Normal);
        }
    }
}
