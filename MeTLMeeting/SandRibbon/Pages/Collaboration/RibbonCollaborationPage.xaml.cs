using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using Microsoft.Windows.Controls.Ribbon;
using SandRibbon.Components;
using SandRibbon.Components.Utility;
using SandRibbon.Pages.Collaboration.Layout;
using SandRibbon.Pages.Conversations;
using SandRibbon.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SandRibbon.Pages.Collaboration
{
    public class ImageSources
    {
        public ImageSource eraserImage { get; protected set; }
        public ImageSource penImage { get; protected set; }
        public ImageSource highlighterImage { get; protected set; }
        public Brush selectedBrush { get; protected set; }
        public ImageSources(ImageSource _eraserImage, ImageSource _penImage, ImageSource _highlighterImage, Brush _selectedBrush)
        {
            eraserImage = _eraserImage;
            penImage = _penImage;
            highlighterImage = _highlighterImage;
            selectedBrush = _selectedBrush;
        }
    }
    public class PenAttributes : DependencyObject
    {
        public DrawingAttributes attributes { get; protected set; }
        protected DrawingAttributes originalAttributes;
        protected InkCanvasEditingMode originalMode;
        public int id { get; protected set; }
        protected bool ready = false;
        protected ImageSources images;
        public PenAttributes(int _id, InkCanvasEditingMode _mode, DrawingAttributes _attributes, ImageSources _images)
        {
            id = _id;
            images = _images;
            attributes = _attributes;
            mode = _mode;
            attributes.StylusTip = StylusTip.Ellipse;
            width = attributes.Width;
            color = attributes.Color;
            originalAttributes = _attributes.Clone();
            originalMode = _mode;
            isSelectedPen = false;
            ready = true;
            icon = generateImageSource();
        }
        public void replaceAttributes(PenAttributes newAttributes)
        {
            mode = newAttributes.mode;
            color = newAttributes.color;
            width = newAttributes.width;
            isHighlighter = newAttributes.isHighlighter;
        }
        public void resetAttributes()
        {
            replaceAttributes(new PenAttributes(id, originalMode, originalAttributes, images));
        }
        protected static readonly Point centerBottom = new Point(128, 256);
        protected static readonly Point centerTop = new Point(128, 0);
        protected void regenerateVisual()
        {
            backgroundBrush = generateBackgroundBrush();
            icon = generateImageSource();
            description = generateDescription();
        }
        protected string generateDescription()
        {
            return ColorHelpers.describe(this);
        }
        protected Brush generateBackgroundBrush()
        {
            return isSelectedPen ? images.selectedBrush : Brushes.Transparent;
        }
        protected ImageSource generateImageSource()
        {
            DrawingVisual visual = new DrawingVisual();
            DrawingContext dc = visual.RenderOpen();
            /*
            if (isSelected)
            {
                dc.DrawRectangle(images.selectedBrush, new Pen(images.selectedBrush, 0), new Rect(new Point(0, 0), new Point(256, 256)));
                dc.DrawEllipse(new SolidColorBrush(Colors.White), new Pen(new SolidColorBrush(Colors.White), 0.0), centerTop, 128, 128);
            }
            */
            if (mode == InkCanvasEditingMode.EraseByStroke || mode == InkCanvasEditingMode.EraseByPoint)
            {
                dc.DrawImage(images.eraserImage, new Rect(0, 0, 256, 256));
            }
            else
            {
                var colorBrush = new SolidColorBrush(attributes.Color);
                //                dc.DrawEllipse(colorBrush, new Pen(colorBrush, 0), center, attributes.Width / 2, attributes.Width / 2);
                //dc.DrawEllipse(colorBrush, new Pen(colorBrush, 0), centerBottom, attributes.Width * 1.25, attributes.Width * 1.25);
                dc.DrawEllipse(colorBrush, new Pen(colorBrush, 0), centerBottom, attributes.Width + 25, attributes.Width + 25); //adjusting size to scale them and make them more visible, so that the radius will be between 25 and 125 out of a maximum diameter of 256
                if (attributes.IsHighlighter)
                {
                    dc.DrawImage(images.highlighterImage, new Rect(0, 0, 256, 256));
                }
                else
                {
                    dc.DrawImage(images.penImage, new Rect(0, 0, 256, 256));
                }
            }
            dc.Close();
            var bmp = new RenderTargetBitmap(256, 256, 96.0, 96.0, PixelFormats.Pbgra32);
            bmp.Render(visual);
            return bmp;
        }
        public double width
        {
            get { return (double)GetValue(widthProperty); }
            set
            {
                attributes.Width = value;
                attributes.Height = value;
                var changed = ((double)GetValue(widthProperty)) != value;
                if (ready && changed)
                    regenerateVisual();
                SetValue(widthProperty, value);
            }
        }

        public static readonly DependencyProperty widthProperty = DependencyProperty.Register("width", typeof(double), typeof(PenAttributes), new PropertyMetadata(1.0));
        public bool isHighlighter
        {
            get { return (bool)GetValue(isHighlighterProperty); }
            set
            {
                attributes.IsHighlighter = value;
                var changed = ((bool)GetValue(isHighlighterProperty)) != value;
                if (ready && changed)
                    regenerateVisual();
                SetValue(isHighlighterProperty, value);
            }
        }

        public static readonly DependencyProperty isHighlighterProperty = DependencyProperty.Register("isHighlighter", typeof(bool), typeof(PenAttributes), new PropertyMetadata(false));
        public Color color
        {
            get { return (Color)GetValue(colorProperty); }
            set
            {
                attributes.Color = value;
                var changed = ((Color)GetValue(colorProperty)) != value;
                if (ready && changed)
                    regenerateVisual();
                SetValue(colorProperty, value);
            }
        }

        public static readonly DependencyProperty colorProperty = DependencyProperty.Register("color", typeof(Color), typeof(PenAttributes), new PropertyMetadata(Colors.Black));
        public InkCanvasEditingMode mode
        {
            get { return (InkCanvasEditingMode)GetValue(modeProperty); }
            set
            {
                var changed = ((InkCanvasEditingMode)GetValue(modeProperty)) != value;
                if (ready && changed)
                    regenerateVisual();
                SetValue(modeProperty, value);
            }
        }

        public static readonly DependencyProperty modeProperty = DependencyProperty.Register("mode", typeof(InkCanvasEditingMode), typeof(PenAttributes), new PropertyMetadata(InkCanvasEditingMode.None));
        public Brush backgroundBrush
        {
            get { return (Brush)GetValue(backgroundBrushProperty); }
            set
            {
                SetValue(backgroundBrushProperty, value);
            }
        }

        public static readonly DependencyProperty backgroundBrushProperty = DependencyProperty.Register("backgroundBrush", typeof(Brush), typeof(PenAttributes), new PropertyMetadata(Brushes.Transparent));
        public string description
        {
            get { return (string)GetValue(descriptionProperty); }
            set
            {
                SetValue(descriptionProperty, value);
            }
        }

        public static readonly DependencyProperty descriptionProperty = DependencyProperty.Register("description", typeof(string), typeof(PenAttributes), new PropertyMetadata(""));

        public ImageSource icon
        {
            get { return (ImageSource)GetValue(iconProperty); }
            protected set { SetValue(iconProperty, value); }
        }

        public static ImageSource emptyImage = new BitmapImage();
        public static readonly DependencyProperty iconProperty = DependencyProperty.Register("icon", typeof(ImageSource), typeof(PenAttributes), new PropertyMetadata(emptyImage));
        public bool isSelectedPen
        {
            get { return (bool)GetValue(isSelectedPenProperty); }
            set
            {
                if (ready)
                    regenerateVisual();
                SetValue(isSelectedPenProperty, value);
            }
        }

        public static readonly DependencyProperty isSelectedPenProperty = DependencyProperty.Register("isSelectedPen", typeof(bool), typeof(PenAttributes), new PropertyMetadata(false));
    }
    public partial class RibbonCollaborationPage : Page, SlideAwarePage
    {
        public NetworkController networkController { get; protected set; }
        public UserGlobalState userGlobal { get; protected set; }
        public UserServerState userServer { get; protected set; }
        public UserConversationState userConv { get; protected set; }
        public UserSlideState userSlide { get; protected set; }
        public ConversationDetails details { get; protected set; }
        public Slide slide { get; protected set; }

        protected System.Collections.ObjectModel.ObservableCollection<PenAttributes> penCollection;
        /*
        private List<double> fontSizes = new List<double> { 8.0, 10.0, 12.0, 14.0, 16.0, 18.0, 20.0, 24.0, 28.0, 32.0, 36.0, 40.0, 48.0, 56.0, 64.0, 72.0, 96.0, 128.0, 144.0, 196.0, 240.0 };
        private List<string> fontList = new List<string> { "Arial", "Times New Roman", "Lucida", "Palatino Linotype", "Verdana", "Wingdings" };
        */
        public RibbonCollaborationPage(UserGlobalState _userGlobal, UserServerState _userServer, UserConversationState _userConv, UserSlideState _userSlide, NetworkController _networkController, ConversationDetails _details, Slide _slide)
        {
            networkController = _networkController;
            details = _details;
            slide = _slide;
            userGlobal = _userGlobal;
            userServer = _userServer;
            userConv = _userConv;
            userSlide = _userSlide;
            InitializeComponent();
            DataContext = this;
            var ic = new ImageSourceConverter();
            var images = new ImageSources(
                ic.ConvertFromString("pack://application:,,,/MeTL;component/Resources/ShinyEraser.png") as ImageSource,
                ic.ConvertFromString("pack://application:,,,/MeTL;component/Resources/appbar.draw.pen.png") as ImageSource,
                ic.ConvertFromString("pack://application:,,,/MeTL;component/Resources/Highlighter.png") as ImageSource,
                (Brush)FindResource("CheckedGradient")
            //                (Brush)FindResource("ShinyAbsoluteHighlightBrush")
            );
            penCollection = new System.Collections.ObjectModel.ObservableCollection<PenAttributes> {
                new PenAttributes(1,InkCanvasEditingMode.EraseByStroke,new System.Windows.Ink.DrawingAttributes {Color=Colors.White,IsHighlighter=false, Width=1 },images),
                new PenAttributes(2,InkCanvasEditingMode.Ink,new System.Windows.Ink.DrawingAttributes {Color=Colors.Black,IsHighlighter=false, Width=1 },images),
                new PenAttributes(3,InkCanvasEditingMode.Ink,new System.Windows.Ink.DrawingAttributes {Color=Colors.Red,IsHighlighter=false, Width=3 },images),
                new PenAttributes(4,InkCanvasEditingMode.Ink,new System.Windows.Ink.DrawingAttributes {Color=Colors.Blue,IsHighlighter=false, Width=3 },images),
                new PenAttributes(5,InkCanvasEditingMode.Ink,new System.Windows.Ink.DrawingAttributes {Color=Colors.Green,IsHighlighter=false, Width=5 },images),
                new PenAttributes(6,InkCanvasEditingMode.Ink,new System.Windows.Ink.DrawingAttributes {Color=Colors.Yellow,IsHighlighter=true, Width=15},images),
                new PenAttributes(7,InkCanvasEditingMode.Ink,new System.Windows.Ink.DrawingAttributes {Color=Colors.Cyan,IsHighlighter=true, Width=25},images)
            };

            pens.ItemsSource = penCollection;

            InitializeComponent();
            /*
            fontFamily.ItemsSource = fontList;
            fontSize.ItemsSource = fontSizes;
            fontSize.SetValue(Selector.SelectedIndexProperty,0);
            Commands.TextboxFocused.RegisterCommand(new DelegateCommand<TextInformation>(updateTextControls));
            Commands.RestoreTextDefaults.RegisterCommand(new DelegateCommand<object>(restoreTextDefaults));            
            */
            var setLayerCommand = new DelegateCommand<string>(SetLayer);
            var duplicateSlideCommand = new DelegateCommand<object>((obj) =>
            {
                duplicateSlide((KeyValuePair<ConversationDetails, Slide>)obj);
            }, (kvp) =>
            {
                try
                {
                    return (kvp != null && ((KeyValuePair<ConversationDetails, Slide>)kvp).Key != null) ? userMayAdministerConversation(((KeyValuePair<ConversationDetails, Slide>)kvp).Key) : false;
                }
                catch
                {
                    return false;
                }
            });
            var duplicateConversationCommand = new DelegateCommand<ConversationDetails>(duplicateConversation, userMayAdministerConversation);
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
                foreach (var p in penCollection)
                {
                    p.isSelectedPen = false;
                    //p.isSelectedPen = p.id == currentPenId;
                };
                pa.isSelectedPen = true;
            });
            var requestReplacePenAttributesCommand = new DelegateCommand<PenAttributes>(pa =>
            {
                new PenCustomizationDialog(pa).ShowDialog();
            }, pa => pa.mode != InkCanvasEditingMode.EraseByPoint && pa.mode != InkCanvasEditingMode.EraseByStroke);
            var replacePenAttributesCommand = new DelegateCommand<PenAttributes>(pa =>
            {
                penCollection.First(p => p.id == pa.id).replaceAttributes(pa);
                if (pa.id == currentPenId)
                {
                    Commands.SetPenAttributes.Execute(pa);
                }
            });
            var requestResetPenAttributesCommand = new DelegateCommand<PenAttributes>(pa =>
            {
                var foundPen = penCollection.First(p => p.id == pa.id);
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
            var updateConversationDetailsCommand = new DelegateCommand<ConversationDetails>(UpdateConversationDetails);
            var proxyMirrorPresentationSpaceCommand = new DelegateCommand<MainWindow>(openProjectorWindow);
            var moveToNextCommand = new DelegateCommand<object>(o => Shift(1));
            var moveToPreviousCommand = new DelegateCommand<object>(o => Shift(-1));

            Loaded += (cs, ce) =>
            {
                Commands.MoveToNext.RegisterCommand(moveToNextCommand);
                Commands.MoveToPrevious.RegisterCommand(moveToPreviousCommand);
                Commands.SetLayer.RegisterCommandToDispatcher<string>(setLayerCommand);
                Commands.DuplicateSlide.RegisterCommand(duplicateSlideCommand);
                Commands.DuplicateConversation.RegisterCommand(duplicateConversationCommand);
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
                Commands.UpdateConversationDetails.RegisterCommand(updateConversationDetailsCommand);
                Commands.ProxyMirrorPresentationSpace.RegisterCommand(proxyMirrorPresentationSpaceCommand);
                scroll.ScrollChanged += (s, e) =>
                    {
                        Commands.RequerySuggested(Commands.ZoomIn, Commands.ZoomOut, Commands.OriginalView, Commands.FitToView, Commands.FitToPageWidth);
                    };
                Commands.SetLayer.Execute("Sketch");
                Commands.SetPenAttributes.Execute(penCollection[1]);
                Commands.ShowProjector.Execute(null);
                networkController.client.SneakInto(details.Jid);
                networkController.client.SneakInto(slide.id.ToString());
                networkController.client.SneakInto(slide.id.ToString() + networkController.credentials.name);
            };
            this.Unloaded += (ps, pe) =>
            {
                Commands.MoveToNext.UnregisterCommand(moveToNextCommand);
                Commands.MoveToPrevious.UnregisterCommand(moveToPreviousCommand);
                Commands.HideProjector.Execute(null);
                Commands.SetLayer.UnregisterCommand(setLayerCommand);
                Commands.DuplicateSlide.UnregisterCommand(duplicateSlideCommand);
                Commands.DuplicateConversation.UnregisterCommand(duplicateConversationCommand);
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
                Commands.UpdateConversationDetails.UnregisterCommand(updateConversationDetailsCommand);
                Commands.ProxyMirrorPresentationSpace.UnregisterCommand(proxyMirrorPresentationSpaceCommand);
                networkController.client.SneakOutOf(slide.id.ToString() + networkController.credentials.name);
                networkController.client.SneakOutOf(slide.id.ToString());
                networkController.client.SneakOutOf(details.Jid);
            };
        }
        private void Shift(int direction)
        {
            var slides = details.Slides.OrderBy(s => s.index).ToList();
            var currentIndex = slides.IndexOf(slide);
            var newSlide = slides.ElementAt(currentIndex + direction);
            if (newSlide != null)
            {
                NavigationService.Navigate(new RibbonCollaborationPage(userGlobal,userServer,userConv,userSlide,networkController, details, newSlide));
            }
        }


        private void UpdateConversationDetails(ConversationDetails cd)
        {
            if (details.Jid == cd.Jid)
            {
                details = cd;
            }
        }

        protected void openProjectorWindow(MainWindow window)
        {
            Commands.MirrorPresentationSpace.Execute(new KeyValuePair<MainWindow, ScrollViewer>(window, scroll));
        }
        /*
private void restoreTextDefaults(object obj)
{
   fontFamily.SetValue(Selector.SelectedItemProperty, "Arial");
   fontSize.SetValue(Selector.SelectedItemProperty, 12.0);            
}

private void updateTextControls(TextInformation info)
{
   fontFamily.SetValue(Selector.SelectedItemProperty, info.Family.ToString());
   fontSize.SetValue(Selector.SelectedItemProperty, info.Size);            
   TextBoldButton.IsChecked = info.Bold;
   TextItalicButton.IsChecked = info.Italics;
   TextUnderlineButton.IsChecked = info.Underline;
   TextStrikethroughButton.IsChecked = info.Strikethrough;
}       

protected bool canDecreaseFont(object _unused)
{
   return fontSize != null;
}
private void decreaseFont(object _unused)
{
   if (fontSize.ItemsSource == null) return;
   var currentItem = (int) fontSize.GetValue(Selector.SelectedIndexProperty);
   if (currentItem - 1 >= 0)
   {
       var newSize = fontSizes[currentItem - 1];
       fontSize.SetValue(Selector.SelectedIndexProperty,currentItem - 1);
       Commands.FontSizeChanged.Execute(newSize);
   }
}   
protected bool canIncreaseFont(object _unused)
{
   return fontSize != null;
}     
private void increaseFont(object _unused)
{
   if (fontSize.ItemsSource == null) return;
   var currentItem = (int)fontSize.GetValue(Selector.SelectedIndexProperty);
   if (currentItem + 1 < fontSizes.Count())
   {
       var newSize = fontSizes[currentItem + 1];
       fontSize.SetValue(Selector.SelectedIndexProperty, currentItem + 1);
       Commands.FontSizeChanged.Execute(newSize);
   }
}
private void fontSizeSelected(object sender, SelectionChangedEventArgs e)
{
   var currentItem = (int)fontSize.GetValue(Selector.SelectedIndexProperty);
   if(currentItem < 0) return;
   if (e.AddedItems.Count == 0) return;
   var size = Double.Parse(e.AddedItems[0].ToString());
   Commands.FontSizeChanged.Execute(size);
}
private void fontFamilySelected(object sender, SelectionChangedEventArgs e)
{
   if (e.AddedItems.Count == 0) return;
   var font = new FontFamily(e.AddedItems[0].ToString());
   Commands.FontChanged.Execute(font);
}   
*/
        private bool userMayAdministerConversation(ConversationDetails _conversation)
        {
            var conversation = details;
            if (conversation == null)
            {
                return false;
            }
            return conversation.UserHasPermission(networkController.credentials);
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
            NavigationService.Navigate(new ConversationSearchPage(userGlobal,userServer,networkController, ""));
        }
        private void RibbonApplicationMenuItem_ConversationOverview_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ConversationOverviewPage(userGlobal, userServer, userConv, networkController, details));
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
                slide != null &&
                slide != Slide.Empty &&
                scroll.Height != slide.defaultHeight &&
                scroll.Width != slide.defaultWidth;
        }
        protected void originalView(object _unused)
        {

            if (scroll != null &&
            details != null &&
            details != ConversationDetails.Empty &&
            slide != null &&
            slide != Slide.Empty)
            {
                var currentSlide = slide;
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
        private void TextColor_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Commands.SetTextColor.Execute((Color)((System.Windows.Controls.Ribbon.RibbonGalleryItem)e.NewValue).Content);
        }
        private void duplicateSlide(KeyValuePair<ConversationDetails, Slide> _kvp)
        {
            var kvp = new KeyValuePair<ConversationDetails, Slide>(details, slide);
            if (kvp.Key.UserHasPermission(networkController.credentials) && kvp.Key.Slides.Exists(s => s.id == kvp.Value.id))
            {
                networkController.client.DuplicateSlide(kvp.Key, kvp.Value);
            }
        }
        private void duplicateConversation(ConversationDetails _conversationToDuplicate)
        {
            var conversationToDuplicate = details;
            if (conversationToDuplicate.UserHasPermission(networkController.credentials))
            {
                networkController.client.DuplicateConversation(conversationToDuplicate);
            }
        }

        private void Ribbon_Loaded(object sender, RoutedEventArgs e)
        {
            Grid child = VisualTreeHelper.GetChild((DependencyObject)sender, 0) as Grid;
            if (child != null)
            {
                child.RowDefinitions[0].Height = new GridLength(0);
            }
        }

        public Slide getSlide()
        {
            return slide;
        }

        public UserSlideState getUserSlideState()
        {
            return userSlide;
        }

        public ConversationDetails getDetails()
        {
            return details;
        }

        public UserConversationState getUserConversationState()
        {
            return userConv;
        }

        public NetworkController getNetworkController()
        {
            return networkController;
        }

        public UserServerState getUserServerState()
        {
            return userServer;
        }
        public NavigationService getNavigationService()
        {
            return NavigationService;
        }

        public UserGlobalState getUserGlobalState()
        {
            return userGlobal;
        }
    }
}
