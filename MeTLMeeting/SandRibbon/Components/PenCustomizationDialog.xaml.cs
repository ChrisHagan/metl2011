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
using System.Windows.Ink;
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
    public static class ColorHelpers
    {
        public static string describe(Color color)
        {
            return colors.Select(c =>
            {
                var rDiff = Math.Abs(color.R - c.Value.R);
                var gDiff = Math.Abs(color.G - c.Value.G);
                var bDiff = Math.Abs(color.B - c.Value.B);
                return new KeyValuePair<KeyValuePair<string, Color>, int>(c, rDiff + gDiff + bDiff);
            }).OrderBy(c => c.Value).First().Key.Key;
        }
        public static string describeDetail(PenAttributes attributes)
        {
            return describeDetail(attributes.mode, attributes.color, attributes.width, attributes.isHighlighter);
        }
        public static string describeDetail(InkCanvasEditingMode mode, Color color, double width, bool isHighlighter)
        {
            if (mode == InkCanvasEditingMode.EraseByPoint || mode == InkCanvasEditingMode.EraseByStroke)
            {
                return "eraser";
            }
            return String.Format("{0}px {1} ({2}) {3}", Convert.ToInt32(width), describe(color), color.ToString(), isHighlighter ? "highlighter" : "pen");
        }
        public static string describe(PenAttributes attributes)
        {
            return describe(attributes.mode,attributes.color, attributes.width);
        }
        public static string describe(InkCanvasEditingMode mode, Color color, double width)
        {
            if (mode == InkCanvasEditingMode.EraseByPoint || mode == InkCanvasEditingMode.EraseByStroke)
            {
                return "eraser";
            }
            return String.Format("{0}px {1}", Convert.ToInt32(width), describe(color));
        }
        public static List<KeyValuePair<string,Color>> colors = new List<KeyValuePair<string,Color>> {
            new KeyValuePair<string,Color>("Black",Colors.Black),
            new KeyValuePair<string,Color>("White",Colors.White),
            new KeyValuePair<string,Color>("Blue",Colors.Blue),
            new KeyValuePair<string,Color>("Green",Colors.Green),
            new KeyValuePair<string,Color>("Red",Colors.Red),
            new KeyValuePair<string,Color>("Yellow",Colors.Yellow),
            new KeyValuePair<string,Color>("Orange",Colors.Orange),
            new KeyValuePair<string,Color>("Cyan",Colors.Cyan),
            new KeyValuePair<string,Color>("Brown",Colors.Brown),
            new KeyValuePair<string,Color>("AliceBlue",Colors.AliceBlue),
            new KeyValuePair<string,Color>("AntiqueWhite",Colors.AntiqueWhite),
            new KeyValuePair<string,Color>("Aqua",Colors.Aqua),
            new KeyValuePair<string,Color>("Aquamarine",Colors.Aquamarine),
            new KeyValuePair<string,Color>("Azure",Colors.Azure),
            new KeyValuePair<string,Color>("Beige",Colors.Beige),
            new KeyValuePair<string,Color>("Bisque",Colors.Bisque),
            new KeyValuePair<string,Color>("BlanchedAlmond",Colors.BlanchedAlmond),
            new KeyValuePair<string,Color>("BlueViolet",Colors.BlueViolet),
            new KeyValuePair<string,Color>("BurlyWood",Colors.BurlyWood),
            new KeyValuePair<string,Color>("CadetBlue",Colors.CadetBlue),
            new KeyValuePair<string,Color>("Chartreuse",Colors.Chartreuse),
            new KeyValuePair<string,Color>("Chocolate",Colors.Chocolate),
            new KeyValuePair<string,Color>("Coral",Colors.Coral),
            new KeyValuePair<string,Color>("CornflowerBlue",Colors.CornflowerBlue),
            new KeyValuePair<string,Color>("Cornsilk",Colors.Cornsilk),
            new KeyValuePair<string,Color>("Crimson",Colors.Crimson),
            new KeyValuePair<string,Color>("DarkBlue",Colors.DarkBlue),
            new KeyValuePair<string,Color>("DarkCyan",Colors.DarkCyan),
            new KeyValuePair<string,Color>("DarkGoldenrod",Colors.DarkGoldenrod),
            new KeyValuePair<string,Color>("DarkGray",Colors.DarkGray),
            new KeyValuePair<string,Color>("DarkGreen",Colors.DarkGreen),
            new KeyValuePair<string,Color>("DarkKhaki",Colors.DarkKhaki),
            new KeyValuePair<string,Color>("DarkMagenta",Colors.DarkMagenta),
            new KeyValuePair<string,Color>("DarkOliveGreen",Colors.DarkOliveGreen),
            new KeyValuePair<string,Color>("DarkOrange",Colors.DarkOrange),
            new KeyValuePair<string,Color>("DarkOrchid",Colors.DarkOrchid),
            new KeyValuePair<string,Color>("DarkRed",Colors.DarkRed),
            new KeyValuePair<string,Color>("DarkSalmon",Colors.DarkSalmon),
            new KeyValuePair<string,Color>("DarkSeaGreen",Colors.DarkSeaGreen),
            new KeyValuePair<string,Color>("DarkSlateBlue",Colors.DarkSlateBlue),
            new KeyValuePair<string,Color>("DarkSlateGray",Colors.DarkSlateGray),
            new KeyValuePair<string,Color>("DarkTurquoise",Colors.DarkTurquoise),
            new KeyValuePair<string,Color>("DarkViolet",Colors.DarkViolet),
            new KeyValuePair<string,Color>("DeepPink",Colors.DeepPink),
            new KeyValuePair<string,Color>("DeepSkyBlue",Colors.DeepSkyBlue),
            new KeyValuePair<string,Color>("DimGray",Colors.DimGray),
            new KeyValuePair<string,Color>("DodgerBlue",Colors.DodgerBlue),
            new KeyValuePair<string,Color>("Firebrick",Colors.Firebrick),
            new KeyValuePair<string,Color>("FloralWhite",Colors.FloralWhite),
            new KeyValuePair<string,Color>("ForestGreen",Colors.ForestGreen),
            new KeyValuePair<string,Color>("Fuchsia",Colors.Fuchsia),
            new KeyValuePair<string,Color>("Gainsboro",Colors.Gainsboro),
            new KeyValuePair<string,Color>("GhostWhite",Colors.GhostWhite),
            new KeyValuePair<string,Color>("Gold",Colors.Gold),
            new KeyValuePair<string,Color>("Goldenrod",Colors.Goldenrod),
            new KeyValuePair<string,Color>("Gray",Colors.Gray),
            new KeyValuePair<string,Color>("GreenYellow",Colors.GreenYellow),
            new KeyValuePair<string,Color>("Honeydew",Colors.Honeydew),
            new KeyValuePair<string,Color>("HotPink",Colors.HotPink),
            new KeyValuePair<string,Color>("IndianRed",Colors.IndianRed),
            new KeyValuePair<string,Color>("Indigo",Colors.Indigo),
            new KeyValuePair<string,Color>("Ivory",Colors.Ivory),
            new KeyValuePair<string,Color>("Khaki",Colors.Khaki),
            new KeyValuePair<string,Color>("Lavender",Colors.Lavender),
            new KeyValuePair<string,Color>("LavenderBlush",Colors.LavenderBlush),
            new KeyValuePair<string,Color>("LawnGreen",Colors.LawnGreen),
            new KeyValuePair<string,Color>("LemonChiffon",Colors.LemonChiffon),
            new KeyValuePair<string,Color>("LightBlue",Colors.LightBlue),
            new KeyValuePair<string,Color>("LightCoral",Colors.LightCoral),
            new KeyValuePair<string,Color>("LightCyan",Colors.LightCyan),
            new KeyValuePair<string,Color>("LightGoldenrodYellow",Colors.LightGoldenrodYellow),
            new KeyValuePair<string,Color>("LightGray",Colors.LightGray),
            new KeyValuePair<string,Color>("LightGreen",Colors.LightGreen),
            new KeyValuePair<string,Color>("LightPink",Colors.LightPink),
            new KeyValuePair<string,Color>("LightSalmon",Colors.LightSalmon),
            new KeyValuePair<string,Color>("LightSeaGreen",Colors.LightSeaGreen),
            new KeyValuePair<string,Color>("LightSkyBlue",Colors.LightSkyBlue),
            new KeyValuePair<string,Color>("LightSlateGray",Colors.LightSlateGray),
            new KeyValuePair<string,Color>("LightSteelBlue",Colors.LightSteelBlue),
            new KeyValuePair<string,Color>("LightYellow",Colors.LightYellow),
            new KeyValuePair<string,Color>("Lime",Colors.Lime),
            new KeyValuePair<string,Color>("LimeGreen",Colors.LimeGreen),
            new KeyValuePair<string,Color>("Linen",Colors.Linen),
            new KeyValuePair<string,Color>("Magenta",Colors.Magenta),
            new KeyValuePair<string,Color>("Maroon",Colors.Maroon),
            new KeyValuePair<string,Color>("MediumAquamarine",Colors.MediumAquamarine),
            new KeyValuePair<string,Color>("MediumBlue",Colors.MediumBlue),
            new KeyValuePair<string,Color>("MediumOrchid",Colors.MediumOrchid),
            new KeyValuePair<string,Color>("MediumPurple",Colors.MediumPurple),
            new KeyValuePair<string,Color>("MediumSeaGreen",Colors.MediumSeaGreen),
            new KeyValuePair<string,Color>("MediumSlateBlue",Colors.MediumSlateBlue),
            new KeyValuePair<string,Color>("MediumSpringGreen",Colors.MediumSpringGreen),
            new KeyValuePair<string,Color>("MediumTurquoise",Colors.MediumTurquoise),
            new KeyValuePair<string,Color>("MediumVioletRed",Colors.MediumVioletRed),
            new KeyValuePair<string,Color>("MidnightBlue",Colors.MidnightBlue),
            new KeyValuePair<string,Color>("MintCream",Colors.MintCream),
            new KeyValuePair<string,Color>("MistyRose",Colors.MistyRose),
            new KeyValuePair<string,Color>("Moccasin",Colors.Moccasin),
            new KeyValuePair<string,Color>("NavajoWhite",Colors.NavajoWhite),
            new KeyValuePair<string,Color>("Navy",Colors.Navy),
            new KeyValuePair<string,Color>("OldLace",Colors.OldLace),
            new KeyValuePair<string,Color>("Olive",Colors.Olive),
            new KeyValuePair<string,Color>("OliveDrab",Colors.OliveDrab),
            new KeyValuePair<string,Color>("OrangeRed",Colors.OrangeRed),
            new KeyValuePair<string,Color>("Orchid",Colors.Orchid),
            new KeyValuePair<string,Color>("PaleGoldenrod",Colors.PaleGoldenrod),
            new KeyValuePair<string,Color>("PaleGreen",Colors.PaleGreen),
            new KeyValuePair<string,Color>("PaleTurquoise",Colors.PaleTurquoise),
            new KeyValuePair<string,Color>("PaleVioletRed",Colors.PaleVioletRed),
            new KeyValuePair<string,Color>("PapayaWhip",Colors.PapayaWhip),
            new KeyValuePair<string,Color>("PeachPuff",Colors.PeachPuff),
            new KeyValuePair<string,Color>("Peru",Colors.Peru),
            new KeyValuePair<string,Color>("Pink",Colors.Pink),
            new KeyValuePair<string,Color>("Plum",Colors.Plum),
            new KeyValuePair<string,Color>("PowderBlue",Colors.PowderBlue),
            new KeyValuePair<string,Color>("RosyBrown",Colors.RosyBrown),
            new KeyValuePair<string,Color>("RoyalBlue",Colors.RoyalBlue),
            new KeyValuePair<string,Color>("SaddleBrown",Colors.SaddleBrown),
            new KeyValuePair<string,Color>("Salmon",Colors.Salmon),
            new KeyValuePair<string,Color>("SandyBrown",Colors.SandyBrown),
            new KeyValuePair<string,Color>("SeaGreen",Colors.SeaGreen),
            new KeyValuePair<string,Color>("SeaShell",Colors.SeaShell),
            new KeyValuePair<string,Color>("Sienna",Colors.Sienna),
            new KeyValuePair<string,Color>("Silver",Colors.Silver),
            new KeyValuePair<string,Color>("SkyBlue",Colors.SkyBlue),
            new KeyValuePair<string,Color>("SlateBlue",Colors.SlateBlue),
            new KeyValuePair<string,Color>("SlateGray",Colors.SlateGray),
            new KeyValuePair<string,Color>("Snow",Colors.Snow),
            new KeyValuePair<string,Color>("SpringGreen",Colors.SpringGreen),
            new KeyValuePair<string,Color>("SteelBlue",Colors.SteelBlue),
            new KeyValuePair<string,Color>("Tan",Colors.Tan),
            new KeyValuePair<string,Color>("Teal",Colors.Teal),
            new KeyValuePair<string,Color>("Thistle",Colors.Thistle),
            new KeyValuePair<string,Color>("Tomato",Colors.Tomato),
            new KeyValuePair<string,Color>("Transparent",Colors.Transparent),
            new KeyValuePair<string,Color>("Turquoise",Colors.Turquoise),
            new KeyValuePair<string,Color>("Violet",Colors.Violet),
            new KeyValuePair<string,Color>("Wheat",Colors.Wheat),
            new KeyValuePair<string,Color>("WhiteSmoke",Colors.WhiteSmoke),
            new KeyValuePair<string,Color>("YellowGreen",Colors.YellowGreen)
        };
    }
    public partial class PenCustomizationDialog : Window
    {
        
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
            var beforeDetails = ColorHelpers.describeDetail(attributes);
            if (attributes.mode != InkCanvasEditingMode.EraseByPoint && attributes.mode != InkCanvasEditingMode.EraseByStroke)
            {
                var parts = beforeDetails.Split(new char[] { '(' });
                beforeDescription.Content = parts[0];
                beforeDescription2.Content = "(" + parts[1];
            }
            DataContext = _attributes;
            aSlider.Value = attributes.color.A;
            rSlider.Value = attributes.color.R;
            gSlider.Value = attributes.color.G;
            bSlider.Value = attributes.color.B;
            widthSlider.Value = attributes.width;
            colorSwatches.ItemsSource = ColorHelpers.colors.Select(c => new KeyValuePair<string,SolidColorBrush>(c.Key,new SolidColorBrush(c.Value)));
            sizeSwatches.ItemsSource = sizes;
            isHighlighter.IsChecked = attributes.isHighlighter;
            ready = true;
            updateDescription();
        }
        protected bool ready = false;
        protected void updateDescription()
        {
            if (ready)
            {
                var afterDetails = ColorHelpers.describeDetail(InkCanvasEditingMode.Ink, new Color { A = (byte)aSlider.Value, B = (byte)bSlider.Value, G = (byte)gSlider.Value, R = (byte)rSlider.Value }, widthSlider.Value, (bool)isHighlighter.IsChecked);
                if (attributes.mode != InkCanvasEditingMode.EraseByPoint && attributes.mode != InkCanvasEditingMode.EraseByStroke)
                {
                    var parts = afterDetails.Split('(');
                    afterDescription.Content = parts[0];
                    afterDescription2.Content = "(" + parts[1];
                }
            }
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
            var color = ((KeyValuePair<string,SolidColorBrush>)((FrameworkElement)sender).DataContext).Value.Color;
            //aSlider.Value = color.A; //all swatches are just colours, with no swatches which have pre-set alpha
            rSlider.Value = color.R;
            gSlider.Value = color.G;
            bSlider.Value = color.B;
            updateDescription();
        }
        private void SelectSizeSwatch(object sender, RoutedEventArgs e)
        {
            var color = ((double)((FrameworkElement)sender).DataContext);
            widthSlider.Value = color;
            updateDescription();
        }
        private void Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void isHighlighter_Checked(object sender, RoutedEventArgs e)
        {
            updateDescription();
        }

        private void penSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            updateDescription();
        }
    }
}
