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

            var inProgressSource = new ObservableCollection<MeTLLib.DiagnosticGauge>();
            inProgress.ItemsSource = inProgressSource;
            
            Commands.DiagnosticGaugeUpdated.RegisterCommandToDispatcher(new DelegateCommand<MeTLLib.DiagnosticGauge>((m) =>
            {
                switch (m.status)
                {
                    case GaugeStatus.Started:
                        inProgressSource.Add(m);
                        break;
                    case GaugeStatus.InProgress:
                        inProgressSource.Select(o =>
                        {
                            if (o.equals(m))
                            {
                                o.update(m);
                                return o;
                            }
                            else
                            {
                                return o;
                            }
                        });
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
            }));
        }
    }
}
