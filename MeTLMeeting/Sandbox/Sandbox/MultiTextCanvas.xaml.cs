using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace Sandbox
{
    public partial class MultiTextCanvas : UserControl
    {
        public MultiTextCanvas()
        {
            InitializeComponent();
            Loaded += (a, b) => {
                MouseUp += (c, args) =>
                {
                    placeCursor(container, (MouseButtonEventArgs)args);
                };
            };
        }
        private void placeCursor(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(container);
            var source = (InkCanvas)sender;
            TextBox box = new TextBox();
            box.TextChanged += stuffHappens;
            box.LostFocus += (_sender, _args) =>
            {
                if(box.Text.Length == 0)
                    container.Children.Remove(box);
            };
            container.Children.Add(box);
            InkCanvas.SetLeft(box, pos.X);
            InkCanvas.SetTop(box, pos.Y);
            box.Focus();
        }

        private void stuffHappens(object sender, TextChangedEventArgs e)
        {
            var output = XamlWriter.Save((TextBox) sender);
            var input = XamlReader.Parse(output);
        }
    }
    public static class TextBoxExtensions
    {
        public static bool IsUnder(this TextBox box, Point point)
        {
            var boxOrigin = new Point(InkCanvas.GetLeft(box), InkCanvas.GetTop(box));
            var boxSize = new Size(box.ActualWidth, box.ActualHeight);
            var result = new Rect(boxOrigin,boxSize).Contains(point);
            Console.WriteLine(String.Format("{0},{1} is within {2},{3} * {4}*{5}?  {6}",
                point.X, point.Y, boxOrigin.X, boxOrigin.Y, boxSize.Width, boxSize.Height, result));
            Console.WriteLine(result.ToString());
            return result;
        }
    }
}
