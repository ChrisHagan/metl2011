using Microsoft.Practices.Composite.Presentation.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
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
    public class DiagnosticDisplay : DependencyObject
    {
        public DiagnosticDisplay()
        {
        }
        public readonly ObservableCollection<DiagnosticGauge> gauges = new ObservableCollection<DiagnosticGauge>();
        public readonly ObservableCollection<DiagnosticMessage> messages = new ObservableCollection<DiagnosticMessage>();
        public void addMessage(DiagnosticMessage message)
        {
            Dispatcher.adopt(delegate
            {
                messages.Add(message);
            });
        }
        public void updateGauge(DiagnosticGauge gauge)
        {
            Commands.DiagnosticGaugeUpdated.ExecuteAsync(gauge);
            Dispatcher.adopt(delegate
            {
                var old = gauges.FirstOrDefault(g => g.equals(gauge));
                if (old != default(DiagnosticGauge))
                {
                    old.update(gauge);
                    gauges.Remove(old);
                    gauges.Add(old);
                }
                else
                {
                    gauges.Add(gauge);

                }
            });
        }
    }

    public partial class DiagnosticWindow : Window
    {
        protected double refreshInterval = 5 * 1000;
        public DiagnosticWindow()
        {
            InitializeComponent();
            var thisProc = Process.GetCurrentProcess();
            gauges.ItemsSource = App.dd.gauges;
            messages.ItemsSource = App.dd.messages;
            var timer = new Timer(refreshInterval);
            this.Closing += (sender, args) =>
            {
                timer.Stop();
                timer.Close();
                timer.Dispose();
            };
            timer.Elapsed += (s, a) =>
            {
                Dispatcher.adopt(delegate
                {
                    nics.ItemsSource = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces().Where(nic => {
                        return nic.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up;
                    }).Select(nic => {
                        return nic.GetIPv4Statistics();
                        });
                    procs.ItemsSource = Process.GetProcessesByName(thisProc.ProcessName).Union(Process.GetProcessesByName(App.proc.ProcessName));
                    errors.Text = App.errorWriter.ToString();
                    console.Text = App.outputWriter.ToString();
                });
            };
            timer.Start();
        }
    }
}
