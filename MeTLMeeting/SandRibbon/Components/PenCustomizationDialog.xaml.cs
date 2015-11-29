using SandRibbon.Pages.Collaboration;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Shapes;

namespace SandRibbon.Components
{
    public class ArgbConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var r = System.Convert.ToByte(values[0]);
            var g = System.Convert.ToByte(values[1]);
            var b = System.Convert.ToByte(values[2]);
            var a = System.Convert.ToByte(values[3]);
            return Color.FromArgb(a, r, g, b);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Interaction logic for PenCustomizationDialog.xaml
    /// </summary>
    public partial class PenCustomizationDialog : Window
    {
        protected static List<Color> colors = new List<Color> {
            Colors.Black,
            Colors.White,
            Colors.Blue,
            Colors.Green,
            Colors.Red,
            Colors.Yellow,
            Colors.Orange,
            Colors.Cyan,
            Colors.Brown,
            Colors.AliceBlue,
            Colors.AntiqueWhite,
            Colors.Aqua,
            Colors.Aquamarine,
            Colors.Azure,
            Colors.Beige,
            Colors.Bisque,
            Colors.BlanchedAlmond,
            Colors.BlueViolet,
            Colors.BurlyWood,
            Colors.CadetBlue,
            Colors.Chartreuse,
            Colors.Chocolate,
            Colors.Coral,
            Colors.CornflowerBlue,
            Colors.Cornsilk,
            Colors.Crimson,
            Colors.DarkBlue,
            Colors.DarkCyan,
            Colors.DarkGoldenrod,
            Colors.DarkGray,
            Colors.DarkGreen,
            Colors.DarkKhaki,
            Colors.DarkMagenta,
            Colors.DarkOliveGreen,
            Colors.DarkOrange,
            Colors.DarkOrchid,
            Colors.DarkRed,
            Colors.DarkSalmon,
            Colors.DarkSeaGreen,
            Colors.DarkSlateBlue,
            Colors.DarkSlateGray,
            Colors.DarkTurquoise,
            Colors.DarkViolet,
            Colors.DeepPink,
            Colors.DeepSkyBlue,
            Colors.DimGray,
            Colors.DodgerBlue,
            Colors.Firebrick,
            Colors.FloralWhite,
            Colors.ForestGreen,
            Colors.Fuchsia,
            Colors.Gainsboro,
            Colors.GhostWhite,
            Colors.Gold,
            Colors.Goldenrod,
            Colors.Gray,
            Colors.GreenYellow,
            Colors.Honeydew,
            Colors.HotPink,
            Colors.IndianRed,
            Colors.Indigo,
            Colors.Ivory,
            Colors.Khaki,
            Colors.Lavender,
            Colors.LavenderBlush,
            Colors.LawnGreen,
            Colors.LemonChiffon,
            Colors.LightBlue,
            Colors.LightCoral,
            Colors.LightCyan,
            Colors.LightGoldenrodYellow,
            Colors.LightGray,
            Colors.LightGreen,
            Colors.LightPink,
            Colors.LightSalmon,
            Colors.LightSeaGreen,
            Colors.LightSkyBlue,
            Colors.LightSlateGray,
            Colors.LightSteelBlue,
            Colors.LightYellow,
            Colors.Lime,
            Colors.LimeGreen,
            Colors.Linen,
            Colors.Magenta,
            Colors.Maroon,
            Colors.MediumAquamarine,
            Colors.MediumBlue,
            Colors.MediumOrchid,
            Colors.MediumPurple,
            Colors.MediumSeaGreen,
            Colors.MediumSlateBlue,
            Colors.MediumSpringGreen,
            Colors.MediumTurquoise,
            Colors.MediumVioletRed,
            Colors.MidnightBlue,
            Colors.MintCream,
            Colors.MistyRose,
            Colors.Moccasin,
            Colors.NavajoWhite,
            Colors.Navy,
            Colors.OldLace,
            Colors.Olive,
            Colors.OliveDrab,
            Colors.OrangeRed,
            Colors.Orchid,
            Colors.PaleGoldenrod,
            Colors.PaleGreen,
            Colors.PaleTurquoise,
            Colors.PaleVioletRed,
            Colors.PapayaWhip,
            Colors.PeachPuff,
            Colors.Peru,
            Colors.Pink,
            Colors.Plum,
            Colors.PowderBlue,
            Colors.RosyBrown,
            Colors.RoyalBlue,
            Colors.SaddleBrown,
            Colors.Salmon,
            Colors.SandyBrown,
            Colors.SeaGreen,
            Colors.SeaShell,
            Colors.Sienna,
            Colors.Silver,
            Colors.SkyBlue,
            Colors.SlateBlue,
            Colors.SlateGray,
            Colors.Snow,
            Colors.SpringGreen,
            Colors.SteelBlue,
            Colors.Tan,
            Colors.Teal,
            Colors.Thistle,
            Colors.Tomato,
            Colors.Transparent,
            Colors.Turquoise,
            Colors.Violet,
            Colors.Wheat,
            Colors.WhiteSmoke,
            Colors.YellowGreen
        };
        protected static List<double> sizes = new List<double> {
            1.0,3.0,5.0,10.0,15.0,25.0,50.0,75.0,100.0
        };
        protected PenAttributes attributes;
        public PenCustomizationDialog(PenAttributes _attributes)
        {
            InitializeComponent();
            attributes = _attributes;
            originalColorPreview.Background = new SolidColorBrush(attributes.color);
            originalColorPreview.CornerRadius = new CornerRadius(attributes.width);
            originalColorPreview.Height = attributes.width;
            originalColorPreview.Width = attributes.width;
            DataContext = _attributes;
            aSlider.Value = attributes.color.A;
            rSlider.Value = attributes.color.R;
            gSlider.Value = attributes.color.G;
            bSlider.Value = attributes.color.B;
            widthSlider.Value = attributes.width;
            colorSwatches.ItemsSource = colors.Select(c => new SolidColorBrush(c));
            sizeSwatches.ItemsSource = sizes;
            isHighlighter.IsChecked = attributes.isHighlighter;
        }

        private void ApplyColor(object sender, RoutedEventArgs e)
        {
            var color = ((SolidColorBrush)colorPreview.Background).Color;
            var width = widthSlider.Value;
            attributes.color = color;
            attributes.width = width;
            attributes.isHighlighter = (bool)isHighlighter.IsChecked;
            Commands.ReplacePenAttributes.Execute(attributes);
            this.Close();
        }

        private void SelectColorSwatch(object sender, RoutedEventArgs e)
        {
            var color = ((SolidColorBrush)((FrameworkElement)sender).DataContext).Color;
            //aSlider.Value = color.A; //all swatches are just colours, with no swatches which have pre-set alpha
            rSlider.Value = color.R;
            gSlider.Value = color.G;
            bSlider.Value = color.B;
        }
        private void SelectSizeSwatch(object sender, RoutedEventArgs e)
        {
            var color = ((double)((FrameworkElement)sender).DataContext);
            widthSlider.Value = color;
        }
        private void Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void isHighlighter_Checked(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
