using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
//using Divelements.SandRibbon;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;
using SandRibbonInterop.Interfaces;
using TextInformation = SandRibbon.Components.TextInformation;
using System.Windows.Controls.Ribbon;

namespace SandRibbon.Components
{
    public partial class TextTools : RibbonGroup, ITextTools
    {
        private List<double> fontSizes = new List<double> { 8.0, 10.0, 12.0, 14.0, 16.0, 18.0, 20.0, 24.0, 28.0, 32.0, 36.0, 40.0, 48.0, 56.0, 64.0, 72.0, 96.0, 128.0, 144.0, 196.0, 240.0 };
        private List<string> fontList = new List<string> { "Arial", "Times New Roman", "Lucida", "Palatino Linotype", "Verdana", "Wingdings" };
        public TextTools()
        {
            loadFonts();
            InitializeComponent();
            fontFamilyCollection.ItemsSource = fontList;
            fontFamilyCollection.SelectedItem = fontList[0];
            fontSizeCollection.ItemsSource = fontSizes;
            fontSizeCollection.SelectedItem = fontSizes[0];// SelectedIndex = 0;
            //fontSizeCollection.SelectionChanged += fontSizeSelected;
            //fontFamilyCollection.SelectionChanged += fontFamilySelected;
            Commands.SetLayer.RegisterCommandToDispatcher<string>(new DelegateCommand<string>(SetLayer));
            Commands.TextboxFocused.RegisterCommandToDispatcher(new DelegateCommand<TextInformation>(update));
            //This is used only when a text box is selected
            //A seperate command is used because TextBoxFocused command calls updateprivacy method which is not needed when a text box is selected
            Commands.TextboxSelected.RegisterCommandToDispatcher(new DelegateCommand<TextInformation>(update));
            Commands.MoveToCollaborationPage.RegisterCommandToDispatcher<object>(new DelegateCommand<object>(MoveTo));
            Commands.ToggleBold.RegisterCommand(new DelegateCommand<object>(togglebold));
            Commands.ToggleItalic.RegisterCommand(new DelegateCommand<object>(toggleItalic));
            Commands.ToggleUnderline.RegisterCommand(new DelegateCommand<object>(toggleUnderline));
            Commands.IncreaseFontSize.RegisterCommand(new DelegateCommand<object>(increaseFont, canIncreaseFont));
            Commands.DecreaseFontSize.RegisterCommand(new DelegateCommand<object>(decreaseFont, canDecreaseFont));
            Commands.TextBoldNotify.RegisterCommand(new DelegateCommand<bool>(b => TextBoldButton.IsChecked = b));
            Commands.TextItalicNotify.RegisterCommand(new DelegateCommand<bool>(b => TextItalicButton.IsChecked = b));
            Commands.TextStrikethroughNotify.RegisterCommand(new DelegateCommand<bool>(b => TextStrikethroughButton.IsChecked = b));
            Commands.TextUnderlineNotify.RegisterCommand(new DelegateCommand<bool>(b => TextUnderlineButton.IsChecked = b));
            //Commands.TextColorNotify.RegisterCommand(new DelegateCommand<Color>(b => TextBoldButton.IsChecked = b));
            Commands.FontNotify.RegisterCommand(new DelegateCommand<string>(b => {
                fontFamilyCollection.SelectedItem = b;
            }));
            Commands.FontSizeNotify.RegisterCommand(new DelegateCommand<double>(b => {
                fontSizeCollection.SelectedItem = b;
            }));
            FontColourButton.ItemsSource = new List<Color> {
                new Color { A=255, R=255, G=255, B=255 },
                new Color { A=255, R=0, G=0, B=0 },
                new Color { A=255, R=255, G=0, B=0 },
                new Color { A=255, R=0, G=255, B=0 },
                new Color { A=255, R=0, G=0, B=255 }
            };
        }
        private void loadFonts()
        {
            fontList = new List<string>();
            foreach(var font in System.Drawing.FontFamily.Families)
                fontList.Add(font.Name);
        }
        private void togglebold(object obj)
        {
            Commands.SetTextBold.Execute(!TextBoldButton.IsChecked);
        }
        private void toggleItalic(object obj)
        {
            Commands.SetTextItalic.Execute(!TextItalicButton.IsChecked);
        }
        private void toggleUnderline(object obj)
        {
            Commands.SetTextUnderline.Execute(!TextUnderlineButton.IsChecked);
        }
        private void MoveTo(object obj)
        {
            if(currentTextInfo == null)
            {
                //ColourPickerBorder.BorderBrush = new SolidColorBrush(Colors.Black); 
                fontSizeCollection.SelectedItem = generateDefaultFontSize();
            }            
        }
        protected void requeryTextCommands()
        {
            Commands.RequerySuggested(
                Commands.TextColorNotify,
                Commands.TextItalicNotify,
                Commands.TextStrikethroughNotify,
                Commands.TextUnderlineNotify,
                Commands.TextBoldNotify,
                Commands.FontNotify,
                Commands.FontSizeNotify,
                Commands.IncreaseFontSize,
                Commands.DecreaseFontSize
             );
        }
        private void update(TextInformation info)
        {
            //fontSizeCollection.SelectionChanged -= fontSizeSelected;
            //fontFamilyCollection.SelectionChanged -= fontFamilySelected;
            TextBoldButton.IsChecked = info.Bold;
            TextItalicButton.IsChecked = info.Italics;
            TextUnderlineButton.IsChecked = info.Underline;
            TextStrikethroughButton.IsChecked = info.Strikethrough;
            //ColourPickerBorder.BorderBrush = new SolidColorBrush(info.Color);
            fontSizeCollection.SelectedItem = info.Size;
            fontFamilyCollection.SelectedItem = info.Family.ToString();
            //fontSizeCollection.SelectionChanged += fontSizeSelected;
            //fontFamilyCollection.SelectionChanged += fontFamilySelected;

            currentTextInfo = new TextInformation(info);
            requeryTextCommands();
        }
        private void sendValues()
        {
            if (fontSizeCollection == null || fontSizeCollection.SelectedItem == null || fontFamily == null || fontFamilyCollection.SelectedItem == null || /*ColourPickerBorder == null || ColourPickerBorder.BorderBrush == null || */TextBoldButton == null || TextItalicButton == null || TextUnderlineButton == null || TextStrikethroughButton == null) return;
            var info = new TextInformation
            {
                Size = (double)fontSizeCollection.SelectedItem,
                Family = new FontFamily(fontFamilyCollection.SelectedItem.ToString()),
                Bold =  TextBoldButton.IsChecked == true,
                Italics = TextItalicButton.IsChecked == true,
                Underline = TextUnderlineButton.IsChecked == true,
                Strikethrough = TextStrikethroughButton.IsChecked == true,
                //Color = ((SolidColorBrush) ColourPickerBorder.BorderBrush).Color
            };
            currentTextInfo = new TextInformation(info);
            Commands.UpdateTextStyling.Execute(info);
        }

        private void SetLayer(string layer)
        {
            if (layer == "Text")
                Visibility = Visibility.Visible;
            else
                Visibility = Visibility.Collapsed;
        }
        private void setUpTools(object sender, RoutedEventArgs e)
        {
            if (currentTextInfo != null)
            {
                update(currentTextInfo);
            }
            else
                fontFamilyCollection.SelectedItem = "Arial";
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
        private bool canDecreaseFont(object _unused)
        {
            if (fontSizeCollection.Items.Count > 0 && fontSizeCollection.SelectedItem != null)
            {
                var thisCurrentItem = (double)fontSizeCollection.SelectedItem;
                var thisElemIndex = fontSizes.IndexOf(thisCurrentItem);
                return thisElemIndex > 1;
            }
            else return false;
        }
        private void decreaseFont(object _unused)
        {
            if (fontSizeCollection.Items.Count > 0)
            {
                var thisCurrentItem = (double)fontSizeCollection.SelectedItem;
                var thisElemIndex = fontSizes.IndexOf(thisCurrentItem);
                if (thisElemIndex > 1)
                {
                    var newItem = fontSizes.ElementAt(thisElemIndex - 1);
                    fontSizeCollection.SelectedItem = newItem;
                    Commands.FontSizeNotify.Execute(newItem);
                }
            }
        }
        private bool canIncreaseFont(object _unused)
        {
            if (fontSizeCollection.Items.Count > 0 && fontSizeCollection.SelectedItem != null)
            {
                var thisCurrentItem = (double)fontSizeCollection.SelectedItem;
                var thisElemIndex = fontSizes.IndexOf(thisCurrentItem);
                return thisElemIndex < (fontSizes.Count - 1);
            }
            else return false;
        }
        private void increaseFont(object _unused)
        {
            if (fontSizeCollection.Items.Count > 0)
            {
                var thisCurrentItem = (double)fontSizeCollection.SelectedItem;
                var thisElemIndex = fontSizes.IndexOf(thisCurrentItem);
                if (thisElemIndex < (fontSizes.Count - 1))
                {
                    var newItem = fontSizes.ElementAt(thisElemIndex + 1);
                    fontSizeCollection.SelectedItem = newItem;
                    Commands.FontSizeNotify.Execute(newItem);
                }
            }
        }
        private void fontSizeSelected(object sender, RoutedEventArgs e)
        {
            if (fontSizeCollection.SelectedItem == null) return;
            //if (fontSize.SelectedIndex == -1) return;
            //if (e.AddedItems.Count == 0) return;
            sendValues();
        }
        private void fontFamilySelected(object sender, RoutedEventArgs e)
        {
            if (fontFamilyCollection.SelectedItem == null) return;
            //if (e.AddedItems.Count == 0) return;
            sendValues();
        }
        /*
        private void textColorSelected(object sender, ColorEventArgs e)
        {
            //ColourPickerBorder.BorderBrush = new SolidColorBrush(e.Color);
            //((System.Windows.Controls.Primitives.Popup)ColourSelection.Parent).IsOpen = false;
            sendValues();
        }
        */
        private void ShowColourSelector(object sender, RoutedEventArgs e)
        {
            //((System.Windows.Controls.Primitives.Popup)ColourSelection.Parent).IsOpen = true;
        }

        private void valuesUpdated(object sender, RoutedEventArgs e)
        {
            var clickedButton = (RibbonToggleButton)sender;
            if (clickedButton == TextStrikethroughButton)
                if (clickedButton.IsChecked == true)
                    TextUnderlineButton.IsChecked = false;
            if (clickedButton == TextUnderlineButton)
                if (clickedButton.IsChecked == true)
                    TextStrikethroughButton.IsChecked = false;
            sendValues();
        }

        private void restoreDefaults(object sender, RoutedEventArgs e)
        {
            Commands.RestoreTextDefaults.Execute(null); 
        }
    }
}
