using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components;
using SandRibbon.Components.Utility;
using SandRibbon.Pages.Collaboration.Models;
using SandRibbon.Pages.Conversations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;

namespace SandRibbon.Pages.Collaboration
{
    public partial class RibbonCollaborationPage
    {        
        public RibbonCollaborationPage(UserGlobalState _userGlobal, UserServerState _userServer, UserConversationState _userConv, ConversationState _convState, UserSlideState _userSlide, NetworkController _networkController)
        {
            var root = new DataContextRoot
            {
                NetworkController = _networkController,
                UserGlobalState = _userGlobal,
                UserServerState = _userServer,
                UserConversationState = _userConv,
                ConversationState = _convState,
                UserSlideState = _userSlide
            };
            UserGlobalState.Images.selectedBrush = FindResource("CheckedGradient") as Brush;
            DataContext = root;
            InitializeComponent();            
            
            var duplicateSlideCommand = new DelegateCommand<object>((obj) =>
            {
                duplicateSlide((KeyValuePair<ConversationDetails, Slide>)obj);
            }, (kvp) =>
            {
                try
                {
                    return (kvp != null && ((KeyValuePair<ConversationDetails, Slide>)kvp).Key != null) ? userMayAdministerConversation(ConversationState) : false;
                }
                catch
                {
                    return false;
                }
            });
            var setLayerCommand = new DelegateCommand<string>(SetLayer);
            var addPrivacyToggleButtonCommand = new DelegateCommand<PrivacyToggleButton.PrivacyToggleButtonInfo>(AddPrivacyButton);
            var removePrivacyAdornersCommand = new DelegateCommand<string>((s) => RemovePrivacyAdorners(s));
            var fitToViewCommand = new DelegateCommand<object>(fitToView, canFitToView);
            var originalViewCommand = new DelegateCommand<object>(originalView, canOriginalView);
            var zoomInCommand = new DelegateCommand<object>(doZoomIn, canZoomIn);
            var zoomOutCommand = new DelegateCommand<object>(doZoomOut, canZoomOut);
            var setZoomRectCommand = new DelegateCommand<Rect>(SetZoomRect);
            var currentPenId = 0;
            var setPenAttributesCommand = new DelegateCommand<PenAttributes>(pa =>
            {
                currentPenId = pa.id;
                foreach (var p in root.UserGlobalState.Pens)
                {
                    p.IsSelectedPen = false;                 
                };
                pa.IsSelectedPen = true;
            });
            var requestReplacePenAttributesCommand = new DelegateCommand<PenAttributes>(pa =>
            {
                new PenCustomizationDialog(pa).ShowDialog();
            }, pa => pa.mode != InkCanvasEditingMode.EraseByPoint && pa.mode != InkCanvasEditingMode.EraseByStroke);
            var replacePenAttributesCommand = new DelegateCommand<PenAttributes>(pa =>
            {
                root.UserGlobalState.Pens.First(p => p.id == pa.id).replaceAttributes(pa);
                if (pa.id == currentPenId)
                {
                    Commands.SetPenAttributes.Execute(pa);
                }
            });
            var requestResetPenAttributesCommand = new DelegateCommand<PenAttributes>(pa =>
            {
                var foundPen = root.UserGlobalState.Pens.First(p => p.id == pa.id);
                foundPen.resetAttributes();
                if (pa.id == currentPenId)
                {
                    Commands.SetPenAttributes.Execute(foundPen);
                }
            }, pa => pa.mode != InkCanvasEditingMode.EraseByPoint && pa.mode != InkCanvasEditingMode.EraseByStroke);
            var joinConversationCommand = new DelegateCommand<string>((convJid) =>
            {
                Commands.RequerySuggested();
            });            
            var proxyMirrorPresentationSpaceCommand = new DelegateCommand<MainWindow>(openProjectorWindow);
            var moveToNextCommand = new DelegateCommand<object>(o => Shift(1));
            var moveToPreviousCommand = new DelegateCommand<object>(o => Shift(-1));

            Loaded += (cs, ce) =>
            {
                UserConversationState.ContentVisibility = ContentFilterVisibility.isGroupSlide(ConversationState.Slide) ? ContentFilterVisibility.defaultGroupVisibilities : ContentFilterVisibility.defaultVisibilities;
                Commands.MoveToNext.RegisterCommand(moveToNextCommand);
                Commands.MoveToPrevious.RegisterCommand(moveToPreviousCommand);
                Commands.SetLayer.RegisterCommandToDispatcher<string>(setLayerCommand);
                Commands.DuplicateSlide.RegisterCommand(duplicateSlideCommand);                
                Commands.AddPrivacyToggleButton.RegisterCommand(addPrivacyToggleButtonCommand);
                Commands.RemovePrivacyAdorners.RegisterCommand(removePrivacyAdornersCommand);
                Commands.FitToView.RegisterCommand(fitToViewCommand);
                Commands.OriginalView.RegisterCommand(originalViewCommand);
                Commands.ZoomIn.RegisterCommand(zoomInCommand);
                Commands.ZoomOut.RegisterCommand(zoomOutCommand);
                Commands.SetZoomRect.RegisterCommandToDispatcher(setZoomRectCommand);
                Commands.SetPenAttributes.RegisterCommand(setPenAttributesCommand);
                Commands.RequestReplacePenAttributes.RegisterCommand(requestReplacePenAttributesCommand);
                Commands.ReplacePenAttributes.RegisterCommand(replacePenAttributesCommand);
                Commands.RequestResetPenAttributes.RegisterCommand(requestResetPenAttributesCommand);
                Commands.JoinConversation.RegisterCommand(joinConversationCommand);               
                Commands.ProxyMirrorPresentationSpace.RegisterCommand(proxyMirrorPresentationSpaceCommand);
                scroll.ScrollChanged += (s, e) =>
                    {
                        Commands.RequerySuggested(Commands.ZoomIn, Commands.ZoomOut, Commands.OriginalView, Commands.FitToView, Commands.FitToPageWidth);
                    };
                Commands.SetLayer.Execute("Sketch");                                
                Commands.ShowProjector.Execute(null);
                root.UserGlobalState.Pens[1].IsSelectedPen = true;

                NetworkController.client.SneakInto(ConversationState.Jid);
                NetworkController.client.SneakInto(ConversationState.Slide.id.ToString());
                NetworkController.client.SneakInto(ConversationState.Slide.id.ToString() + NetworkController.credentials.name);
            };
            this.Unloaded += (ps, pe) =>
            {
                Commands.MoveToNext.UnregisterCommand(moveToNextCommand);
                Commands.MoveToPrevious.UnregisterCommand(moveToPreviousCommand);
                Commands.HideProjector.Execute(null);
                Commands.SetLayer.UnregisterCommand(setLayerCommand);
                Commands.DuplicateSlide.UnregisterCommand(duplicateSlideCommand);      
                Commands.AddPrivacyToggleButton.UnregisterCommand(addPrivacyToggleButtonCommand);
                Commands.RemovePrivacyAdorners.UnregisterCommand(removePrivacyAdornersCommand);
                Commands.FitToView.UnregisterCommand(fitToViewCommand);
                Commands.OriginalView.UnregisterCommand(originalViewCommand);
                Commands.ZoomIn.UnregisterCommand(zoomInCommand);
                Commands.ZoomOut.UnregisterCommand(zoomOutCommand);
                Commands.SetZoomRect.UnregisterCommand(setZoomRectCommand);
                Commands.SetPenAttributes.UnregisterCommand(setPenAttributesCommand);
                Commands.RequestReplacePenAttributes.UnregisterCommand(requestReplacePenAttributesCommand);
                Commands.ReplacePenAttributes.UnregisterCommand(replacePenAttributesCommand);
                Commands.RequestResetPenAttributes.UnregisterCommand(requestResetPenAttributesCommand);
                Commands.JoinConversation.UnregisterCommand(joinConversationCommand);                
                Commands.ProxyMirrorPresentationSpace.UnregisterCommand(proxyMirrorPresentationSpaceCommand);
                UserConversationState.ContentVisibility = ContentFilterVisibility.defaultVisibilities;
                NetworkController.client.SneakOutOf(ConversationState.Slide.id.ToString() + NetworkController.credentials.name);
                NetworkController.client.SneakOutOf(ConversationState.Slide.id.ToString());
                NetworkController.client.SneakOutOf(ConversationState.Jid);
            };
        }
        private void Shift(int direction)
        {
            var slides = ConversationState.Slides.OrderBy(s => s.index).ToList();
            var currentIndex = slides.IndexOf(ConversationState.Slide);
            var newSlide = slides.ElementAt(currentIndex + direction);
            if (newSlide != null)
            {
                ConversationState.Slide = newSlide;                
            }
        }        
        protected void openProjectorWindow(MainWindow window)
        {
            Commands.MirrorPresentationSpace.Execute(new KeyValuePair<MainWindow, ScrollViewer>(window, scroll));
        }       
        private bool userMayAdministerConversation(ConversationState state)
        {
            return ConversationState.StudentsCanDuplicate;
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
                case "Html": imageGroup.Visibility = Visibility.Visible; break;
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
            if (targetName == null) //default
            {
                viewbox = canvasViewBox;
                container = canvas;
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
            NavigationService.Navigate(new ConversationSearchPage(UserGlobalState, UserServerState, NetworkController, ""));
        }
        private void RibbonApplicationMenuItem_ConversationOverview_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ConversationOverviewPage(UserGlobalState, UserServerState, UserConversationState, ConversationState, NetworkController));
        }
        private bool canZoomIn(object sender)
        {
            return !(scroll == null);
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
                result = (hTrue || vTrue);
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
        }
        protected bool canFitToView(object _unused)
        {
            return scroll != null && !(double.IsNaN(scroll.Height) && double.IsNaN(scroll.Width) && double.IsNaN(canvas.Height) && double.IsNaN(canvas.Width));            
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
                ConversationState.Slide != null &&
                ConversationState.Slide != Slide.Empty &&
                scroll.Height != ConversationState.Slide.defaultHeight &&
                scroll.Width != ConversationState.Slide.defaultWidth;
        }
        protected void originalView(object _unused)
        {
            if (scroll != null &&
            ConversationState.Slide != null &&
            ConversationState.Slide != Slide.Empty)
            {                
                if (ConversationState.Slide.defaultHeight == 0 || ConversationState.Slide.defaultWidth == 0) return;
                scroll.Width = ConversationState.Slide.defaultWidth;
                scroll.Height = ConversationState.Slide.defaultHeight;
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
        private void duplicateSlide(KeyValuePair<ConversationDetails, Slide> _kvp)
        {
            if (ConversationState.CanDuplicate) ConversationState.DuplicateSlide();            
        }        
        private void Ribbon_Loaded(object sender, RoutedEventArgs e)
        {//Remove the QuickLaunchBar; it's too small to use and we have our own Frame
            Grid child = VisualTreeHelper.GetChild((DependencyObject)sender, 0) as Grid;
            if (child != null)
            {
                child.RowDefinitions[0].Height = new GridLength(0);
            }
        }
    }
}