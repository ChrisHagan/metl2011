using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace MeTLLib
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
        public double duration
        {
            get
            {
                var diff = finished.Subtract(started);
                return diff.TotalSeconds;// + "." + diff.TotalMilliseconds;
            }
        }
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
                if (status != GaugeStatus.Failed || status != GaugeStatus.Completed)
                {
                    if (other.status == GaugeStatus.Failed || other.status == GaugeStatus.Completed)
                    {
                        finished = DateTime.Now;
                    }
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

    public interface IAuditor
    {
        T wrapTask<T>(Func<Action<GaugeStatus>, T> action, string name, string category);
        T note<T>(Func<T> action, string name, string category);
    }
    public class NoAuditor : IAuditor
    {
        public T wrapTask<T>(Func<Action<GaugeStatus>, T> action, string name, string category) { return action((gs) => { }); }
        public T note<T>(Func<T> action, string name, string category) { return action(); }

    }
    public class FuncAuditor : IAuditor
    {
        protected Action<DiagnosticGauge> gpf;
        protected Action<DiagnosticMessage> mf;
        public FuncAuditor(Action<DiagnosticGauge> gaugeProgressFunc, Action<DiagnosticMessage> messageFunc)
        {
            gpf = gaugeProgressFunc;
            mf = messageFunc;
        }
        public T wrapTask<T>(Func<Action<GaugeStatus>, T> action, string name, string category)
        {
            var gauge = new DiagnosticGauge(name, category, DateTime.Now);
            gpf(gauge);
            T result = default(T);
            try
            {
                result = action((gs) =>
                {
                    gauge.update(gs);
                    gpf(gauge);
                });
                gauge.update(GaugeStatus.Completed);
                gpf(gauge);
                return result;
            }
            catch (Exception e)
            {
                gauge.update(GaugeStatus.Failed);
                gpf(gauge);
                throw e;
            }
        }
        public T note<T>(Func<T> action, string name, string category)
        {
            mf(new DiagnosticMessage(name, category, DateTime.Now));
            return action();
        }
    }
}
