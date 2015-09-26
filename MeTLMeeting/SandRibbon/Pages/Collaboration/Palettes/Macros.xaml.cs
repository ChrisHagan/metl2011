using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace SandRibbon.Pages.Collaboration.Palettes
{
    public partial class Macros
    {
        public void WormLoaded(object sender, RoutedEventArgs e)
        {
            var worm = sender as OxyPlot.Wpf.PlotView;
            var window = 100;
            var points = new ObservableCollection<DataPoint>();
            for (var i = 0; i < window; i++)
            {
                points.Add(new DataPoint(i, 0));
            }
            var wormModel = new PlotModel();
            var wormSerie = new LineSeries { Color=OxyColors.White, Smooth=true};
            wormModel.Series.Add(wormSerie);
            worm.Model = wormModel;
            var activityCount = 0;
            Commands.ReceiveStrokes.RegisterCommand(new DelegateCommand<List<TargettedStroke>>(ts => activityCount += ts.Count));
            var dispatcherTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1) };
            dispatcherTimer.Tick += delegate
            {
                var shift = points.Select((p, i) => new DataPoint(i + 1, p.Y)).ToList();
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
