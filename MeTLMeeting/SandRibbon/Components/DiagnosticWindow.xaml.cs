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
using Akka.Actor;

namespace SandRibbon.Components
{
    public class DiagnosticModel
    {
        protected Stack<DiagnosticGauge> gauges = new Stack<DiagnosticGauge>();
        protected List<DiagnosticMessage> messages = new List<DiagnosticMessage>();
        protected List<ErrorMessage> errors = new List<ErrorMessage>();
        protected readonly object gaugeLocker = new object();
        protected readonly object errorLocker = new object();
        protected readonly object messageLocker = new object();

        public List<DiagnosticGauge> getGauges()
        {
            return gauges.ToList();
        }
        public List<DiagnosticMessage> getMessages()
        {
            return messages.ToList();
        }
        public List<ErrorMessage> getErrors()
        {
            return errors.ToList();
        }
        public void addMessage(DiagnosticMessage message)
        {
            lock (messageLocker)
            {
                messages.Add(message);
            }
        }
        public void AddError(ErrorMessage e)
        {
            lock (errorLocker)
            {
                errors.Add(e);
            }
        }
        public void updateGauge(DiagnosticGauge gauge)
        {
            lock (gaugeLocker)
            {
                if (gauge.status == GaugeStatus.Started)
                {
                    gauges.Push(gauge);
                }
                else
                {
                    Stack<DiagnosticGauge> hand = new Stack<DiagnosticGauge>();
                    bool found = false;
                    while (!found && gauges.Count > 0)
                    {
                        var candidate = gauges.Pop();
                        if (candidate == gauge)
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
            }
        }
    }
    public class DiagnosticsCollector : ReceiveActor
    {
        public static string REQUESTHISTORY = "requestHistory";
        protected IActorRef target = App.actorSystem.ActorOf<DiagnosticWindowReceiver>("diagnosticWindowReceiver");
        public DiagnosticModel store = new DiagnosticModel();
        public DiagnosticsCollector()
        {
            Receive<string>(s =>
            {
                if (s == REQUESTHISTORY)
                {
                    target.Tell(store);
                }
            });
            Receive<DiagnosticMessage>(m =>
            {
                target.Tell(m);
                store.addMessage(m);
            });
            Receive<DiagnosticGauge>(g =>
            {
                target.Tell(g);
                store.updateGauge(g);
            });
            Receive<ErrorMessage>(e => {
                target.Tell(e);
                store.AddError(e);
            });
        }
    }
    public class DiagnosticWindowReceiver : ReceiveActor
    {
        public DiagnosticWindowReceiver()
        {
            Receive<DiagnosticModel>(dm => {
                if (App.diagnosticWindow != null)
                {
                    App.diagnosticWindow.Dispatcher.adopt(delegate
                    {
                        App.diagnosticWindow.diagnosticModel = dm;
                    });
                }
            });
            Receive<ErrorMessage>(e => {
                if (App.diagnosticWindow != null)
                {
                    App.diagnosticWindow.Dispatcher.adopt(delegate
                    {
                        App.diagnosticWindow.errorSource.Add(e);
                    });
                }
            });
            Receive<DiagnosticMessage>(m =>
            {
                if (App.diagnosticWindow != null)
                {
                    App.diagnosticWindow.Dispatcher.adopt(delegate
                    {
                        App.diagnosticWindow.messageSource.Add(m);
                    });

                }
            });
            Receive<DiagnosticGauge>(g =>
            {
                if (App.diagnosticWindow != null)
                {
                    App.diagnosticWindow.Dispatcher.adopt(delegate
                    {
                        var oldFromTotal = App.diagnosticWindow.gaugeSource.FirstOrDefault(eg => eg.Equals(g));
                        if (oldFromTotal != default(DiagnosticGauge))
                            App.diagnosticWindow.gaugeSource.Remove(oldFromTotal);
                        App.diagnosticWindow.gaugeSource.Add(g);
                        var old = App.diagnosticWindow.inProgressSource.FirstOrDefault(eg => eg.Equals(g));
                        switch (g.status)
                        {
                            case GaugeStatus.Started:
                                App.diagnosticWindow.inProgressSource.Add(g);
                                break;
                            case GaugeStatus.InProgress:
                                if (old != default(DiagnosticGauge))
                                    App.diagnosticWindow.inProgressSource.Remove(old);
                                App.diagnosticWindow.inProgressSource.Add(g);
                                break;
                            case GaugeStatus.Completed:
                                if (old != default(DiagnosticGauge))
                                    App.diagnosticWindow.inProgressSource.Remove(old);
                                break;
                            case GaugeStatus.Failed:
                                if (old != default(DiagnosticGauge))
                                    App.diagnosticWindow.inProgressSource.Remove(old);
                                break;
                        }

                    });
                }
            });
        }
    }
    public partial class DiagnosticWindow : Window
    {
        public ObservableCollection<DiagnosticGauge> gaugeSource = new ObservableCollection<DiagnosticGauge>();
        public ObservableCollection<DiagnosticGauge> inProgressSource = new ObservableCollection<DiagnosticGauge>();
        public ObservableCollection<DiagnosticMessage> messageSource = new ObservableCollection<DiagnosticMessage>();
        public ObservableCollection<ErrorMessage> errorSource = new ObservableCollection<ErrorMessage>();
        protected double refreshInterval = 5 * 1000;
        protected DiagnosticModel dmModel;
        public DiagnosticModel diagnosticModel { get {
                return dmModel;
            } set {
                dmModel = value;
                dmModel.getErrors().ForEach(e => errorSource.Add(e));
                dmModel.getMessages().ForEach(m => messageSource.Add(m));
                dmModel.getGauges().ForEach(g => gaugeSource.Add(g));
            }
        }
        public DiagnosticWindow()
        {
            InitializeComponent();
            this.Loaded += (s, e) => {
                App.diagnosticModelActor.Tell(DiagnosticsCollector.REQUESTHISTORY);
            };
            gauges.ItemsSource = gaugeSource;
            messages.ItemsSource = messageSource;
            inProgress.ItemsSource = inProgressSource;
            errors.ItemsSource = errorSource;
            var thisProc = Process.GetCurrentProcess();
            var timer = new Timer(refreshInterval);
            this.Closing += (sender, args) =>
            {
                timer.Stop();
                timer.Close();
                timer.Dispose();
            };
            timer.Elapsed += (s, a) =>
            {
                App.diagnosticWindow.Dispatcher.adopt(delegate
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
