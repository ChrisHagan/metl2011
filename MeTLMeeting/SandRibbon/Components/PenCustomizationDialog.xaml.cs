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
        protected PenAttributes attributes;
        public PenCustomizationDialog(PenAttributes _attributes)
        {
            InitializeComponent();
            attributes = _attributes;
            DataContext = _attributes;
            aSlider.Value = attributes.color.A;
            rSlider.Value = attributes.color.R;
            gSlider.Value = attributes.color.G;
            bSlider.Value = attributes.color.B;
            widthSlider.Value = attributes.width;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var color = ((SolidColorBrush)colorPreview.Background).Color;
            var width = widthSlider.Value;
            attributes.color = color;
            attributes.width = width;
            Commands.ReplacePenAttributes.Execute(attributes);
            this.Close();
        }
    }
}
