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

namespace SandRibbon.Components
{
    public enum GaugeStatus { Started, InProgress, Completed, Failed }
    public class DiagnosticGauge : DependencyObject
    {

        public string GaugeName { get; protected set; }
        public string GaugeCategory { get; protected set; }

        public DiagnosticGauge(string _name, string _category, DateTime _started)
        {
            GaugeName = _name;
            GaugeCategory = _category;
            started = _started;
            status = GaugeStatus.Started;
            value = 0;
        }
        public DiagnosticGauge(string _name, string _category, DateTime _started, int _target) : this(_name, _category, _started)
        {
            target = _target;
        }
        public int target { get; protected set; }
        public DateTime started { get; protected set; }
        public DateTime finished { get; protected set; }
        public GaugeStatus status { get; protected set; }
        public int value { get; protected set; }
        public double duration { get
            {
                var diff = finished.Subtract(started);
                return diff.TotalSeconds;// + "." + diff.TotalMilliseconds;
            } }
        public void update(GaugeStatus _status)
        {
            status = _status;
            value += 1;
        }
        public void update(DiagnosticGauge other)
        {
            if (other.GaugeName == GaugeName && other.GaugeCategory == GaugeCategory)
            {
                status = other.status;
                value = other.value;
                if (other.status == GaugeStatus.Failed || other.status == GaugeStatus.Completed)
                {
                    finished = DateTime.Now;
                }
            }
        }
        public bool equals(DiagnosticGauge other)
        {
            return other.GaugeName == GaugeName && other.GaugeCategory == GaugeCategory && other.started == started;
        }
    }
    public class DiagnosticMessage : DependencyObject
    {
        public string message { get; protected set; }
        public string category { get; protected set; }
        public DateTime when { get; protected set; }
        public DiagnosticMessage(string _message, string _category, DateTime _when)
        {
            message = _message;
            category = _category;
            when = _when;
        }
    }
    public class DiagnosticDisplay : DependencyObject
    {
        public readonly ObservableCollection<DiagnosticGauge> gauges = new ObservableCollection<DiagnosticGauge>();
        public readonly ObservableCollection<DiagnosticMessage> messages = new ObservableCollection<DiagnosticMessage>();
        public void addMessage(DiagnosticMessage message)
        {
            messages.Add(message);
        }
        public void updateGauge(DiagnosticGauge gauge)
        {
            try
            {
                var old = gauges.First(g => g.equals(gauge));
                old.update(gauge);
                gauges.Remove(old);
                gauges.Add(old);
            }
            catch
            {
                gauges.Add(gauge);
            }

        }
    }
    public partial class DiagnosticWindow : Window
    {
        protected DiagnosticDisplay dd = new DiagnosticDisplay();
        public DiagnosticWindow()
        {
            InitializeComponent();
            //DataContext = dd;
            gauges.ItemsSource = dd.gauges;
            messages.ItemsSource = dd.messages;
            Commands.DiagnosticMessage.RegisterCommand(new DelegateCommand<DiagnosticMessage>((m) =>
            {
                dd.addMessage(m);
            }));
            Commands.DiagnosticGaugeUpdated.RegisterCommand(new DelegateCommand<DiagnosticGauge>((m) =>
            {
                dd.updateGauge(m);
            }));
        }
    }
}
