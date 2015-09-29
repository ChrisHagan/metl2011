using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Threading;

namespace SandRibbon.Pages.Collaboration.Palettes
{
    /*This class exists to mark the root beneath which the control is not operable and can be manipulated in bar configurations*/
    public class Appearance : ContentControl
    {
        internal Appearance Clone()
        {
            object clone;
            using (var stream = new MemoryStream())
            {
                XamlWriter.Save(this, stream);
                stream.Seek(0, SeekOrigin.Begin);
                clone = XamlReader.Load(stream);
            }
            return clone as Appearance;
        }
    }
    public partial class Macros
    {
        public void WormLoaded(object sender, RoutedEventArgs e)
        {
            var worm = sender as OxyPlot.Wpf.PlotView;
            var window = 100;
            var wormModel = new PlotModel();
            wormModel.Axes.Add(new LinearAxis {
                Position=AxisPosition.Bottom,
                IsAxisVisible=false
            });
            wormModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                IsAxisVisible = false
            });
            var wormSerie = new LineSeries { Color=OxyColors.White, Smooth=true};
            var points = wormSerie.Points;
            for (var i = 0; i < window; i++)
            {
                points.Add(new DataPoint(i, 0));
            }
            wormModel.Series.Add(wormSerie);
            worm.Model = wormModel;
            var activityCount = 0;
            Commands.ReceiveStrokes.RegisterCommand(new DelegateCommand<List<TargettedStroke>>(ts => activityCount += ts.Count));
            var dispatcherTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 500) };
            dispatcherTimer.Tick += delegate
            {
                var shift = points.Take(window-1).Select((p, i) => new DataPoint(i + 1, p.Y)).ToList();
                points.Clear();
                points.Add(new DataPoint(0, activityCount));
                foreach (var p in shift)
                {
                    points.Add(p);
                }
                worm.InvalidatePlot();
                activityCount = 0;
            };
            dispatcherTimer.Start();
        }
    }
}
