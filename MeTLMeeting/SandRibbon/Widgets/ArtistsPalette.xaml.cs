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
using System.Collections.ObjectModel;
using SandRibbon.Tabs.Groups;
using System.Windows.Ink;
using System.Windows.Controls.Primitives;

namespace SandRibbon.Widgets
{
    public partial class ArtistsPalette : UserControl
    {
        public CurrentColourValues currentColourValues = new CurrentColourValues();
        public ObservableCollection<PenColors.DrawingAttributesEntry> previouslySelectedDrawingAttributes = new ObservableCollection<PenColors.DrawingAttributesEntry>();
        public ArtistsPalette()
        {
            InitializeComponent();
            PenSizeSlider.Value = 1;
            previousColors.ItemsSource = previouslySelectedDrawingAttributes;
        }
        private void SwitchImage(object sender, RoutedEventArgs e)
        {
            var newImageChoice = ((System.Windows.Controls.Button)sender).Tag.ToString();
            Uri uri;
            switch (newImageChoice)
            {
                case "Custom":
                    var openDialogue = new System.Windows.Forms.OpenFileDialog();
                    openDialogue.Title = "Select Picture for Colour Chooser";
                    openDialogue.Filter = "Portable Network Graphic Files (*.png)|*.png|JPEG Files (*.jpg)|*.jpg|Bitmap Files (*.bmp)|*.bmp|All files (*.*)|*.*";
                    openDialogue.CheckFileExists = true;
                    openDialogue.FilterIndex = 2;
                    openDialogue.Multiselect = false;
                    openDialogue.RestoreDirectory = true;
                    openDialogue.ShowDialog();
                    while (openDialogue.FileName == null)
                    {
                    }
                    if (openDialogue.FileName == null || openDialogue.FileName == "")
                        return;
                    uri = new Uri(openDialogue.FileName);
                    break;
                case "ColourBars":
                    uri = new Uri("http://drawkward.adm.monash.edu/ColorBars.png", UriKind.Absolute);
                    break;
                case "Spectrum":
                    uri = new Uri("http://drawkward.adm.monash.edu/Spectrum.png", UriKind.Absolute);
                    break;
                default:
                    return;
            }
            try
            {
                var bi = new BitmapImage(uri);
                ImageSource imgSource = bi;
                ColourPicker.Source = imgSource;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Import Picture: " + ex);
            }
        }
        private void SetPenSizeByButton(object sender, RoutedEventArgs e)
        {
            var newPenSize = ((System.Windows.Controls.Button)sender).Tag.ToString();
            currentColourValues.CurrentPenSize = Double.Parse(newPenSize);
        }
        private void ChangeColor(object sender, RoutedEventArgs e)
        {
            var colorName = ((System.Windows.Controls.Button)sender).Tag.ToString();
            var drawingAttributes = (DrawingAttributes)previouslySelectedDrawingAttributes.Where(c => c.ColorName == colorName).Select(c => c.Attributes).Single();
            Commands.SetDrawingAttributes.Execute(drawingAttributes);
            e.Handled = true;
        }
        private void SetHighlighter(object sender, RoutedEventArgs e)
        {
            if (((ToggleButton)sender).IsChecked == true)
                Commands.SetHighlighterMode.Execute(true);
            else Commands.SetHighlighterMode.Execute(false);
        }

        private void setCurrentColour()
        {
        }
        private void updatePreviousDrawingAttributes(DrawingAttributes attributes)
        {
            int nextAvailableSpot = 0;
            if (previouslySelectedDrawingAttributes.Select(c => c.Attributes).Contains(attributes)) return;
            previouslySelectedDrawingAttributes.Insert(nextAvailableSpot,
                new PenColors.DrawingAttributesEntry()
                {
                    Attributes = attributes,
                    ColorName = attributes.Color.ToString() + ":" + attributes.Height.ToString() + ":" + attributes.IsHighlighter.ToString(),
                    ColorValue = attributes.Color,
                    XAMLColorName = attributes.Color.ToString(),
                    IsHighlighter = attributes.IsHighlighter,
                    PenSize = attributes.Width
                });
        }
        private void ColourPickerMouseUp(object sender, MouseEventArgs e)
        {
            var HPos = ((e.GetPosition(ColourPicker).X) / (ColourPicker.ActualWidth)) * ((BitmapSource)(ColourPicker.Source)).PixelWidth;
            var VPos = ((e.GetPosition(ColourPicker).Y) / (ColourPicker.ActualHeight)) * ((BitmapSource)(ColourPicker.Source)).PixelHeight;
            currentColourValues.currentColor = PickColor(HPos, VPos);
        }
        private void ColourPickerMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var HPos = ((e.GetPosition(ColourPicker).X) / (ColourPicker.ActualWidth)) * ((BitmapSource)(ColourPicker.Source)).PixelWidth;
                var VPos = ((e.GetPosition(ColourPicker).Y) / (ColourPicker.ActualHeight)) * ((BitmapSource)(ColourPicker.Source)).PixelHeight;
                currentColourValues.currentColor = PickColor(HPos, VPos);
            }
        }
        private void ColourPickerMouseDown(object sender, MouseEventArgs e)
        {
            var HPos = ((e.GetPosition(ColourPicker).X) / (ColourPicker.ActualWidth)) * ((BitmapSource)(ColourPicker.Source)).PixelWidth;
            var VPos = ((e.GetPosition(ColourPicker).Y) / (ColourPicker.ActualHeight)) * ((BitmapSource)(ColourPicker.Source)).PixelHeight;
            currentColourValues.currentColor = PickColor(HPos, VPos);
        }
        private Color PickColor(double x, double y)
        {
            BitmapSource bitmapSource = ColourPicker.Source as BitmapSource;
            if (bitmapSource != null)
            {
                var cb = new CroppedBitmap(bitmapSource,
                                                     new Int32Rect((int)Convert.ToInt32(x),
                                                                   (int)Convert.ToInt32(y), 1, 1));
                var pixels = new byte[4];
                try
                {
                    cb.CopyPixels(pixels, 4, 0);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                return Color.FromRgb(pixels[2], pixels[1], pixels[0]);
            }
            var emptyColor = new Color();
            emptyColor.A = 255;
            emptyColor.R = 0;
            emptyColor.G = 0;
            emptyColor.B = 0;
            return emptyColor;
        }
    }
}
