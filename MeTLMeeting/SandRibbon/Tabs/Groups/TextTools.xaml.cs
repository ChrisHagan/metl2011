using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Divelements.SandRibbon;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Canvas;
using SandRibbon.Providers;
using SandRibbonInterop.Interfaces;

namespace SandRibbon.Tabs.Groups
{
    public partial class TextTools : UserControl, ITextTools 
    {
        private List<double> fontSizes = new List<double> { 8.0, 10.0, 12.0, 14.0, 16.0, 18.0, 20.0, 24.0, 28.0, 32.0, 36.0, 40.0, 48.0, 56.0, 64.0, 72.0, 96.0, 128.0, 144.0, 196.0, 240.0 };
        private List<string> fontList = new List<string> {"Arial", "Times New Roman", "Lucida", "Palatino Linotype", "Verdana", "Wingdings"};
        
        public TextTools()
        {
            InitializeComponent();
            fontFamily.ItemsSource = fontList;
            fontSize.ItemsSource = fontSizes;
            fontSize.SelectedIndex = 0;
            Commands.SetLayer.RegisterCommand(new DelegateCommand<string>(updateToolBox));
            Commands.TextboxFocused.RegisterCommand(new DelegateCommand<TextInformation>(update));
            Commands.RestoreTextDefaults.RegisterCommand(new DelegateCommand<object>(restoreTextDefaults));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<object>(moveTo));
        }

        private void moveTo(object obj)
        {
            fontSize.SelectedItem = generateDefaultFontSize();
        }   

        private void restoreTextDefaults(object obj)
        {
            ColourPickerBorder.BorderBrush = Brushes.Black;
            fontSize.SelectedItem = 10;
            fontFamily.SelectedItem = "Arial";
        }

        private void update(TextInformation info)
        {
            fontSize.SelectedItem = info.size;
            fontFamily.SelectedItem = info.family.ToString();
            TextBoldButton.IsChecked = info.bold;
            TextItalicButton.IsChecked = info.italics;
            TextUnderlineButton.IsChecked = info.underline;
            TextStrikethroughButton.IsChecked = info.strikethrough;
        }

        private void updateToolBox(string layer)
        {
            fontSize.SelectedItem = generateDefaultFontSize();
            if (layer == "Text")
                Visibility = Visibility.Visible;
            else
                Visibility = Visibility.Collapsed;
        }
        private void setUpTools(object sender, RoutedEventArgs e)
        {
            fontFamily.SelectedItem = "Arial";
            fontSize.SelectedItem = generateDefaultFontSize();
        }
        private const double defaultWidth  = 720;
        private const double defaultFontSize = 24.0;
        private static double generateDefaultFontSize()
        {
            try
            {
                var currentSlide = Globals.slides.Where(s => s.id == Globals.slide).First();
                var multiply = (currentSlide.defaultWidth/defaultWidth) > 0
                                     ? (int)(currentSlide.defaultWidth/defaultWidth) : 1;
                return defaultFontSize* multiply;
            }
            catch(NotSetException e)
            {
                return defaultFontSize;
            }
        }

        private void decreaseFont(object sender, RoutedEventArgs e)
        {
            if(fontSize.ItemsSource == null) return; 
            int currentItem = fontSize.SelectedIndex;
            if (currentItem - 1 >= 0)
            {
                var newSize = fontSizes[currentItem - 1];
                fontSize.SelectedIndex = currentItem - 1;
                Commands.FontSizeChanged.Execute(newSize);
            }
        }
        private void increaseFont(object sender, RoutedEventArgs e)
        {
            if (fontSize.ItemsSource == null) return;
            int currentItem = fontSize.SelectedIndex;
            if (currentItem + 1 < fontSizes.Count())
            {
                var newSize = fontSizes[currentItem + 1];
                fontSize.SelectedIndex = currentItem +1;
                Commands.FontSizeChanged.Execute(newSize);
            }
        }
        private void fontSizeSelected(object sender, SelectionChangedEventArgs e)
        {
            if(fontSize.SelectedIndex == -1) return;
            if(e.AddedItems.Count == 0) return;
            var size = Double.Parse(e.AddedItems[0].ToString());
            Commands.FontSizeChanged.Execute(size);
        }
        private void fontFamilySelected(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count == 0) return;
            var font = new FontFamily(e.AddedItems[0].ToString());
            Commands.FontChanged.Execute(font);
        }
        private void textColorSelected(object sender, ColorEventArgs e)
        {
            ColourPickerBorder.BorderBrush = new SolidColorBrush(e.Color);
            ((System.Windows.Controls.Primitives.Popup)ColourSelection.Parent).IsOpen = false;
            Commands.SetTextColor.Execute(e.Color);
        }

        private void ShowColourSelector(object sender, RoutedEventArgs e)
        {
            ((System.Windows.Controls.Primitives.Popup)ColourSelection.Parent).IsOpen = true;
        }
    }
}
