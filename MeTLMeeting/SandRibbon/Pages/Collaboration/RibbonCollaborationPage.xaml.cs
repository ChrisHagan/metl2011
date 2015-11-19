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
        public RibbonCollaborationPage(NetworkController _networkController, ConversationDetails _details, Slide slide)
        {
            networkController = _networkController;
            details = _details;
            InitializeComponent();
            DataContext = slide;
            Commands.SetLayer.RegisterCommand(new DelegateCommand<string>(SetLayer));

            loadFonts();
            InitializeComponent();
            fontFamily.ItemsSource = fontList;
            fontSize.ItemsSource = fontSizes;
            fontSize.SelectedIndex = 0;
            fontSize.SelectionChanged += fontSizeSelected;
            fontFamily.SelectionChanged += fontFamilySelected;
            Commands.SetLayer.RegisterCommandToDispatcher<string>(new DelegateCommand<string>(SetLayer));
            Commands.TextboxFocused.RegisterCommandToDispatcher(new DelegateCommand<TextInformation>(update));
            //This is used only when a text box is selected
            //A seperate command is used because TextBoxFocused command calls updateprivacy method which is not needed when a text box is selected
            Commands.TextboxSelected.RegisterCommandToDispatcher(new DelegateCommand<TextInformation>(update));            
            Commands.ToggleBold.RegisterCommand(new DelegateCommand<object>(togglebold));
            Commands.ToggleItalic.RegisterCommand(new DelegateCommand<object>(toggleItalic));
            Commands.ToggleUnderline.RegisterCommand(new DelegateCommand<object>(toggleUnderline));
        }

        private void SetLayer(string layer)
        {
            foreach (var group in new UIElement[] { inkGroup, textGroup, imageGroup }) {
                group.Visibility = Visibility.Collapsed;
            }
            switch(layer){
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

        private List<double> fontSizes = new List<double> { 8.0, 10.0, 12.0, 14.0, 16.0, 18.0, 20.0, 24.0, 28.0, 32.0, 36.0, 40.0, 48.0, 56.0, 64.0, 72.0, 96.0, 128.0, 144.0, 196.0, 240.0 };
        private List<string> fontList = new List<string> { "Arial", "Times New Roman", "Lucida", "Palatino Linotype", "Verdana", "Wingdings" };        
        private void loadFonts()
        {
            fontList = new List<string>();
            foreach (var font in System.Drawing.FontFamily.Families)
                fontList.Add(font.Name);
        }
        private void togglebold(object obj)
        {
            TextBoldButton.IsChecked = !TextBoldButton.IsChecked;
            sendValues();
        }
        private void toggleItalic(object obj)
        {
            TextItalicButton.IsChecked = !TextItalicButton.IsChecked;
            sendValues();
        }
        private void toggleUnderline(object obj)
        {
            TextUnderlineButton.IsChecked = !TextUnderlineButton.IsChecked;
            sendValues();
        }        

        private void update(TextInformation info)
        {
            fontSize.SelectionChanged -= fontSizeSelected;
            fontFamily.SelectionChanged -= fontFamilySelected;
            TextBoldButton.IsChecked = info.Bold;
            TextItalicButton.IsChecked = info.Italics;
            TextUnderlineButton.IsChecked = info.Underline;
            TextStrikethroughButton.IsChecked = info.Strikethrough;
            fontSize.SelectedItem = info.Size;
            fontFamily.SelectedItem = info.Family.ToString();
            fontSize.SelectionChanged += fontSizeSelected;
            fontFamily.SelectionChanged += fontFamilySelected;

            currentTextInfo = new TextInformation(info);
        }
        private void sendValues()
        {
            if (fontSize == null || fontFamily == null || fontFamily.SelectedItem == null || TextBoldButton == null || TextItalicButton == null || TextUnderlineButton == null || TextStrikethroughButton == null) return;
            var info = new TextInformation
            {
                Size = (double)fontSize.SelectedItem,
                Family = new FontFamily(fontFamily.SelectedItem.ToString()),
                Bold = TextBoldButton.IsChecked == true,
                Italics = TextItalicButton.IsChecked == true,
                Underline = TextUnderlineButton.IsChecked == true,
                Strikethrough = TextStrikethroughButton.IsChecked == true            
            };
            currentTextInfo = new TextInformation(info);
            Commands.UpdateTextStyling.Execute(info);
        }        
        private void setUpTools(object sender, RoutedEventArgs e)
        {
            if (currentTextInfo != null)
            {
                update(currentTextInfo);
            }
            else
                fontFamily.SelectedItem = "Arial";
        }
        private const double defaultWidth = 720;
        private const double defaultFontSize = 24.0;
        private TextInformation currentTextInfo;

        private static double generateDefaultFontSize()
        {
            try
            {
                MeTLLib.DataTypes.Slide currentSlide;
                if (Globals.slides.Count > 0)
                    currentSlide = Globals.slides.Where(s => s.id == Globals.slide).FirstOrDefault();
                else currentSlide = new MeTLLib.DataTypes.Slide(0, "", MeTLLib.DataTypes.Slide.TYPE.SLIDE, 0, 720, 540);
                var multiply = (currentSlide.defaultWidth / defaultWidth) > 0
                                     ? (int)(currentSlide.defaultWidth / defaultWidth) : 1;
                return defaultFontSize * multiply;
            }
            catch (NotSetException)
            {
                return defaultFontSize;
            }
        }
        private void decreaseFont(object sender, RoutedEventArgs e)
        {
            if (fontSize.ItemsSource == null) return;
            int currentItem = fontSize.SelectedIndex;
            if (currentItem - 1 >= 0)
            {
                fontSize.SelectedIndex = currentItem - 1;
                // value is sent over the network in the selected index changed event handler 
                //sendValues();
            }
        }
        private void increaseFont(object sender, RoutedEventArgs e)
        {
            if (fontSize.ItemsSource == null) return;
            int currentItem = fontSize.SelectedIndex;
            if (currentItem + 1 < fontSizes.Count())
            {
                fontSize.SelectedIndex = currentItem + 1;
                // value is sent over the network in the selected index changed event handler 
                //sendValues();
            }
        }
        private void fontSizeSelected(object sender, SelectionChangedEventArgs e)
        {
            if (fontSize.SelectedIndex == -1) return;
            if (e.AddedItems.Count == 0) return;
            sendValues();
        }
        private void fontFamilySelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            sendValues();
        }        

        private void valuesUpdated(object sender, RoutedEventArgs e)
        {
            var clickedButton = (RibbonToggleButton)sender;
            switch (clickedButton.Name)
            {
                case "TextStrikethroughButton":
                    if (clickedButton.IsChecked == true)
                        TextUnderlineButton.IsChecked = false;
                    break;
                case "TextUnderlineButton":
                    if (clickedButton.IsChecked == true)
                        TextStrikethroughButton.IsChecked = false;
                    break;
            }
            sendValues();
        }

        private void restoreDefaults(object sender, RoutedEventArgs e)
        {
            Commands.RestoreTextDefaults.Execute(null);
        }
    }
}
