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
using System.Windows.Shapes;
using SandRibbon.Connection;
using System.Windows.Ink;
using System.Collections.ObjectModel;

namespace Sandbox
{
public class Comparison
{
    public double compression{get{
        return (compressed / (double)uncompressed);
    }}
    public int compressed { get; set; }
    public int uncompressed { get; set; }
}
public partial class HandwritingBenchmarker : Window
{
    public ObservableCollection<Comparison> data = new ObservableCollection<Comparison>();
    public HandwritingBenchmarker()
    {
        InitializeComponent();
        dataViewer.ItemsSource = data;
    }
    private void canvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
    {
        var roundedStroke = Translator.shortStrokeToString(e.Stroke, Rounder.places);
        data.Add(new Comparison{
            compressed = Translator.stringToBytes(roundedStroke).Count(),
            uncompressed = Translator.strokeToBytes(e.Stroke).Count()});
        rounded.Strokes.Add(Translator.shortStringToStroke(roundedStroke));
        compression.Text = Math.Round(data.Average(c => c.compression), 3).ToString();
    }
}
public class Rounder : IValueConverter
{
    public static int places = 1;
    public static Rounder instance;
    static Rounder(){
        instance = new Rounder();
    }
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        var strokes = (StrokeCollection)value;
        foreach (var stroke in strokes)
            for(int i = 0;i<stroke.StylusPoints.Count();i++){
                var p = stroke.StylusPoints[i];
                p.X = Math.Round(p.X, places);
                p.Y = Math.Round(p.Y, places);
                p.PressureFactor = (float)Math.Round((double)p.PressureFactor, places);
            }
        /*
        var result = new StrokeCollection(strokes.Select(s => new Stroke(new StylusPointCollection(s.StylusPoints.Select(p => new StylusPoint {
            PressureFactor = (float)Math.Round((double)p.PressureFactor, places),
            X = Math.Round(p.X, places),
            Y = Math.Round(p.Y, places)})))));
        strokes.Clear();
        foreach (var stroke in result)
            strokes.Add(stroke);
         */
        return strokes;
    }
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
}
