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

namespace SandRibbon.Components
{
    public partial class DiagnosticWindow : Window
    {
        protected double refreshInterval = 5 * 1000;
        public DiagnosticWindow()
        {
            InitializeComponent();
            var thisProc = Process.GetCurrentProcess();
            gauges.ItemsSource = App.dd.gauges;
            messages.ItemsSource = App.dd.messages;
            Commands.DiagnosticMessage.RegisterCommand(new DelegateCommand<MeTLLib.DiagnosticMessage>((m) =>
            {
                Dispatcher.adopt(delegate
                {
                    App.dd.addMessage(m);
                });
            }));
            Commands.DiagnosticGaugeUpdated.RegisterCommand(new DelegateCommand<MeTLLib.DiagnosticGauge>((m) =>
            {
                Dispatcher.adopt(delegate
                {
                    App.dd.updateGauge(m);
                });
            }));
            var timer = new Timer(refreshInterval);
            this.Closing += (sender, args) =>
            {
                App.diagnosticWindow = new DiagnosticWindow();
                timer.Stop();
            };
            timer.Elapsed += (s, a) =>
            {
                Dispatcher.adopt(delegate
                {
                    nics.ItemsSource = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
                    procs.ItemsSource = Process.GetProcessesByName(thisProc.ProcessName).Union(Process.GetProcessesByName(App.proc.ProcessName));
                    errors.Text = App.errorWriter.ToString();
                    console.Text = App.outputWriter.ToString();
                });
            };
            timer.Start();
        }
    }
}
