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
using System.Collections.Concurrent;

namespace SandRibbon.Components
{
    public class DiagnosticModel
    {
        protected Stack<DiagnosticGauge> gauges = new Stack<DiagnosticGauge>();
        protected List<DiagnosticMessage> messages = new List<DiagnosticMessage>();
        protected readonly object gaugeLocker = new object();
        protected readonly object messageLocker = new object();
        public void addMessage(DiagnosticMessage message)
        {
            Commands.DiagnosticMessage.Execute(message);
            lock (messageLocker)
            {
                messages.Add(message);
                Commands.DiagnosticMessagesUpdated.Execute(messages.ToList());
            }
        }
        public void updateGauge(DiagnosticGauge gauge)
        {
            Commands.DiagnosticGaugeUpdated.Execute(gauge);
            lock (gaugeLocker)
            {
                if (gauge.status == GaugeStatus.Started) {
                    gauges.Push(gauge);
                }
                else
                {
                    Stack<DiagnosticGauge> hand = new Stack<DiagnosticGauge>();
                    bool found = false;
                    while (!found && gauges.Count > 0)
                    {
                        var candidate = gauges.Pop();
                        if (candidate.equals(gauge))
                        {
                            candidate.update(gauge);
                            gauges.Push(candidate);
                            found = true;
                        }
                        else
                        {
                            hand.Push(candidate);
                        }
                    }
                    while (hand.Count > 0)
                    {
                        gauges.Push(hand.Pop());
                    }
                    if (!found)
                    {
                        gauges.Push(gauge);
                    }
                }
                Commands.DiagnosticGaugesUpdated.Execute(gauges.ToList());
            }
        }
    }
    /*
    public class DiagnosticDisplay : DependencyObject
    {
        public DiagnosticDisplay()
        {
        }
        public readonly ObservableCollection<DiagnosticGauge> gauges = new ObservableCollection<DiagnosticGauge>();
        public readonly ObservableCollection<DiagnosticMessage> messages = new ObservableCollection<DiagnosticMessage>();
        public void addMessage(DiagnosticMessage message)
        {
            this.Dispatcher.adoptAsync(delegate {
                messages.Add(message);
            });
        }
        public void updateGauge(DiagnosticGauge gauge)
        {
            this.Dispatcher.adoptAsync(delegate
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
    */
    public partial class DiagnosticWindow : Window
    {
        protected double refreshInterval = 5 * 1000;
       // protected DiagnosticDisplay dd = new DiagnosticDisplay();
        public DiagnosticWindow()
        {
            InitializeComponent();
            Commands.DiagnosticGaugesUpdated.RegisterCommand(new DelegateCommand<List<DiagnosticGauge>>(gs => {
                Dispatcher.adoptAsync(delegate
                {
                    gauges.ItemsSource = gs.Where(g => g.status == GaugeStatus.Completed || g.status == GaugeStatus.Failed);
                });
            }));
            var messageListSize = 20;
            Commands.DiagnosticMessagesUpdated.RegisterCommand(new DelegateCommand<List<DiagnosticMessage>>(ms => {
                Dispatcher.adoptAsync(delegate
                {
                    var orig = ms.ToList();
                    var origCnt = orig.Count;
                    orig.Reverse();
                    
                    messages.ItemsSource = orig.GetRange(0, Math.Min(origCnt,messageListSize)); //orig.GetRange(Math.Max(0,ms.Count - messageListSize),ms.Count);
                });
            }));
            /*
            Dispatcher.adopt(delegate
            {
                foreach (var g in App.dd.gauges)
                {
                    dd.updateGauge(g);
                }
            });
            

            Commands.DiagnosticGaugeUpdated.RegisterCommand(new DelegateCommand<DiagnosticGauge>(g => {
                Dispatcher.adopt(delegate {
                    gauges.Items.Clear();
                    foreach (var e in App.dd.gauges.ToList())
                    {
                        gauges.Items.Add(e);
                    }
                });
            }));
            Dispatcher.adopt(delegate
            {
                foreach (var m in App.dd.messages.ToList())
                {
                    messages.Items.Add(m);
                }
            });
            Commands.DiagnosticMessage.RegisterCommand(new DelegateCommand<DiagnosticMessage>(m => {
                Dispatcher.adopt(delegate
                {
                    messages.Items.Add(m);
                });
            }));
            */
            var thisProc = Process.GetCurrentProcess();
            //gauges.ItemsSource = App.dd.gauges;
            //messages.ItemsSource = App.dd.messages;
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
                    nics.ItemsSource = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces().Where(nic =>
                    {
                        return nic.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up;
                    }).Select(nic =>
                    {
                        return nic.GetIPv4Statistics();
                    });
                    procs.ItemsSource = Process.GetProcessesByName(thisProc.ProcessName).Union(Process.GetProcessesByName(App.proc.ProcessName));
                    console.Text = App.outputWriter.ToString();
                });
            };
            timer.Start();
        }
    }
}
