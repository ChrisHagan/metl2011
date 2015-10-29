using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace MeTLLib
{
    public enum GaugeStatus { Started, InProgress, Completed, Failed }
    public class DiagnosticGauge
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
        public DateTime started { get; protected set; }
        public double soFar { get
            {
                return status != GaugeStatus.Completed && status != GaugeStatus.Failed ? DateTime.Now.Subtract(started).TotalSeconds : duration;
            } }
        public DateTime finished { get; protected set; }
        public GaugeStatus status { get; protected set; }
        public int value { get; protected set; }
        public double duration
        {
            get
            {
                var diff = finished.Subtract(started);
                return diff.TotalSeconds;
            }
        }
        public void update(GaugeStatus _status, int progress)
        {
            status = _status;
            value = progress;
        }
        public void update(DiagnosticGauge other)
        {
            if (other.GaugeName == GaugeName && other.GaugeCategory == GaugeCategory)
            {
                if (status != GaugeStatus.Failed && status != GaugeStatus.Completed)
                {
                    value = other.value;
                }
                else
                {
                    value = 100;
                    finished = DateTime.Now;
                }
                status = other.status;
            }
        }
        public bool equals(DiagnosticGauge other)
        {
            return other.GaugeName == GaugeName && other.GaugeCategory == GaugeCategory && other.started == started;
        }
    }
    public class DiagnosticMessage
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
        T wrapFunction<T>(Func<Action<GaugeStatus, int>, T> function, string name, string category);
        void wrapAction(Action<Action<GaugeStatus, int>> action, string name, string category);
        T note<T>(Func<T> action, string name, string category);
    }
    public class NoAuditor : IAuditor
    {
        public T wrapFunction<T>(Func<Action<GaugeStatus, int>, T> action, string name, string category) { return action((gs, v) => { }); }
        public void wrapAction(Action<Action<GaugeStatus, int>> action, string name, string category) { action((gs, v) => { }); }
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
        public void wrapAction(Action<Action<GaugeStatus, int>> action, string name, string category)
        {
            var gauge = new DiagnosticGauge(name, category, DateTime.Now);
            gpf(gauge);
            try
            {
                action((gs, v) =>
                {
                    gauge.update(gs, v);
                    gpf(gauge);
                });
                gauge.update(GaugeStatus.Completed, 100);
                gpf(gauge);
            }
            catch (Exception e)
            {
                gauge.update(GaugeStatus.Failed, 100);
                gpf(gauge);
                throw e;
            }
        }
        public T wrapFunction<T>(Func<Action<GaugeStatus, int>, T> action, string name, string category)
        {
            var gauge = new DiagnosticGauge(name, category, DateTime.Now);
            gpf(gauge);
            T result = default(T);
            try
            {
                result = action((gs, v) =>
                {
                    gauge.update(gs, v);
                    gpf(gauge);
                });
                gauge.update(GaugeStatus.Completed, 100);
                gpf(gauge);
                return result;
            }
            catch (Exception e)
            {
                gauge.update(GaugeStatus.Failed, 100);
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
