using Microsoft.Practices.Composite.Presentation.Commands;
using Microsoft.Win32;
using SandRibbon.Components.Utility;
using SandRibbon.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace SandRibbon.Components.Pages
{
    /// <summary>
    /// Interaction logic for InConversationPage.xaml
    /// </summary>
    public partial class InConversationPage : Page
    {
        private int slide;

        public InConversationPage(int slide)
        {
            InitializeComponent();
            PreviewKeyDown += new KeyEventHandler(KeyPressed);
            //zoom            
            Commands.OriginalView.RegisterCommand(new DelegateCommand<object>(OriginalView));            
            Commands.FitToView.RegisterCommand(new DelegateCommand<object>(FitToView));
            Commands.FitToPageWidth.RegisterCommand(new DelegateCommand<object>(FitToPageWidth));           
            Commands.SetZoomRect.RegisterCommandToDispatcher(new DelegateCommand<Rect>(SetZoomRect));
            Commands.MoveCanvasByDelta.RegisterCommandToDispatcher(new DelegateCommand<Point>(GrabMove));
            Commands.ProxyMirrorPresentationSpace.RegisterCommand(new DelegateCommand<object>(ProxyMirrorPresentationSpace));
            Commands.AddPrivacyToggleButton.RegisterCommand(new DelegateCommand<PrivacyToggleButton.PrivacyToggleButtonInfo>(AddPrivacyButton));
            Commands.RemovePrivacyAdorners.RegisterCommand(new DelegateCommand<string>((target) => { RemovePrivacyAdorners(target); }));
            Commands.PresentVideo.RegisterCommandToDispatcher(new DelegateCommand<object>(presentVideo));
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
            //BroadcastZoom();
        }

        private void notepadScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            UpdatePrivacyAdorners(notesAdornerScroll.Target);
            //BroadcastZoom();
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
        private void canZoomIn(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (scroll != null);
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
        private void MoveTo(int slide)
        {
            Dispatcher.adoptAsync(delegate
            {
                if (canvas.Visibility == Visibility.Collapsed)
                    canvas.Visibility = Visibility.Visible;
                scroll.Width = Double.NaN;
                scroll.Height = Double.NaN;
                canvas.Width = Double.NaN;
                canvas.Height = Double.NaN;
            });
            CommandManager.InvalidateRequerySuggested();
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
        private void presentVideo(object _arg)
        {
            var chooseVideo = new OpenFileDialog();
            var result = chooseVideo.ShowDialog(Window.GetWindow(this));
            if (result == true)
            {
                var popup = new Window();
                var sp = new StackPanel();
                var player = new MediaElement
                {
                    Source = new Uri(chooseVideo.FileName),
                    LoadedBehavior = MediaState.Manual
                };
                player.Play();
                var buttons = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };
                bool isPaused = false;
                player.MouseLeftButtonUp += delegate
                {
                    if (isPaused)
                        player.Play();
                    else
                        player.Pause();
                };
                Func<Func<Point>, RoutedEventHandler> handler = point =>
                {
                    return new RoutedEventHandler(delegate
                    {
                        var location = String.Format("{0}{1}.jpg", System.IO.Path.GetTempPath(), DateTime.Now.Ticks);
                        if (player.HasVideo)
                        {
                            var width = Convert.ToInt32(player.ActualWidth);
                            var height = Convert.ToInt32(player.ActualHeight);
                            var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                            rtb.Render(player);
                            var encoder = new JpegBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(rtb));
                            using (var fs = new FileStream(location, FileMode.CreateNew))
                            {
                                encoder.Save(fs);
                            }
                            Commands.ImageDropped.Execute(new ImageDrop
                            {
                                Filename = location,
                                Point = point(),
                                Position = 1,
                                Target = "presentationSpace"
                            });
                        }
                    });
                };
                var under = new SandRibbon.Components.ResourceDictionaries.Button
                {
                    Text = "Drop"
                };
                under.Click += handler(() => popup.TranslatePoint(new Point(0, 0), canvasViewBox));
                var fullScreen = new SandRibbon.Components.ResourceDictionaries.Button
                {
                    Text = "Fill"
                };
                fullScreen.Click += handler(() => new Point(0, 0));
                buttons.Children.Add(under);
                buttons.Children.Add(fullScreen);
                sp.Children.Add(player);
                sp.Children.Add(buttons);
                popup.Content = sp;
                popup.Topmost = true;
                popup.Owner = Window.GetWindow(this);
                popup.Show();
            }
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

        private void ProxyMirrorPresentationSpace(object unused)
        {
            Commands.MirrorPresentationSpace.ExecuteAsync(this);
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
    }
}
