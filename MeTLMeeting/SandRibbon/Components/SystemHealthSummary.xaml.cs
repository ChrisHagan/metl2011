using Microsoft.Practices.Composite.Presentation.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using MeTLLib;

namespace SandRibbon.Components
{
    /// <summary>
    /// Interaction logic for SystemHealthSummary.xaml
    /// </summary>
    public partial class SystemHealthSummary : Grid
    {
        protected int failedCount = 0;
        public SystemHealthSummary()
        {
            InitializeComponent();

            //var inProgressSource = new ObservableCollection<MeTLLib.DiagnosticGauge>();
            //inProgress.ItemsSource = inProgressSource;
            Commands.DiagnosticGaugesUpdated.RegisterCommand(new DelegateCommand<List<DiagnosticGauge>>(gs => {
                Dispatcher.adoptAsync(delegate
                {
                    inProgress.ItemsSource = gs.Where(g => g.status != GaugeStatus.Completed);
                });
            }));
            /*
            Commands.DiagnosticGaugeUpdated.RegisterCommand(new DelegateCommand<MeTLLib.DiagnosticGauge>((m) =>
            {
                this.Dispatcher.adoptAsync(delegate
                {
                    switch (m.status)
                    {
                        case GaugeStatus.Started:
                            inProgressSource.Add(m);
                            break;
                        case GaugeStatus.InProgress:
                            var old = inProgressSource.FirstOrDefault(g => g.equals(m));
                            if (old != default(DiagnosticGauge))
                            {
                                old.update(m);
                                inProgressSource.Remove(old);
                                inProgressSource.Add(old);
                            }
                            else
                            {
                                inProgressSource.Add(m);
                            }
                            break;
                        case GaugeStatus.Completed:
                            inProgressSource.Remove(m);
                            break;
                        case GaugeStatus.Failed:
                            inProgressSource.Remove(m);
                            failedCount += 1;
                            failureCount.Content = failedCount;
                            break;
                    }
                });
            }));
            */
        }
    }
}
