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
        private Guid id = Guid.NewGuid();
        public Guid guid { get { return id; } }
        public string GaugeName { get; private set; }
        public string GaugeCategory { get; private set; }

        public DiagnosticGauge(string _name, string _category, DateTime _started, DateTime _ended, int _value, GaugeStatus _status, Guid _guid)
        {
            GaugeName = _name;
            GaugeCategory = _category;
            started = _started;
            finished = _ended;
            status = _status;
            value = _value;
            id = _guid;
        }
        public DiagnosticGauge(string _name, string _category, DateTime _started)
        {
            GaugeName = _name;
            GaugeCategory = _category;
            started = _started;
            status = GaugeStatus.Started;
            value = 0;
        }
        public DateTime started { get; private set; }
        public double soFar
        {
            get
            {
                return status != GaugeStatus.Completed && status != GaugeStatus.Failed ? DateTime.Now.Subtract(started).TotalSeconds : duration;
            }
        }
        public DateTime finished { get; private set; }
        public GaugeStatus status { get; private set; }
        public int value { get; private set; }
        public double duration
        {
            get
            {
                var diff = finished.Subtract(started);
                return diff.TotalSeconds;
            }
        }
        public DiagnosticGauge update(GaugeStatus _status, int progress)
        {
            if (_status == GaugeStatus.Completed || _status == GaugeStatus.Failed)
            {

                return new DiagnosticGauge(GaugeCategory, GaugeName, started, DateTime.Now, progress, _status, guid);
            }
            else
            {
                return new DiagnosticGauge(GaugeCategory, GaugeName, started, finished, progress, _status, guid);
            }
        }
        public DiagnosticGauge update(DiagnosticGauge other)
        {
            if (other.GaugeName == GaugeName && other.GaugeCategory == GaugeCategory)
            {
                return other;
            }
            else
            {
                return new DiagnosticGauge(GaugeName, GaugeCategory, started, other.finished, other.value, other.status, guid);
            }
        }
        public override bool Equals(object o)
        {
            if (o is DiagnosticGauge)
            {
                var other = o as DiagnosticGauge;
                return other.guid == id;// other.GaugeName == GaugeName && other.GaugeCategory == GaugeCategory && other.started.Ticks == started.Ticks;
            }
            else return false;
        }
        public override int GetHashCode()
        {
            return id.GetHashCode();// (GaugeName.GetHashCode() / 3) + (GaugeCategory.GetHashCode() / 3) + (started.GetHashCode() / 3);
        }
    }
    public class DiagnosticMessage
    {
        public string message { get; private set; }
        public string category { get; private set; }
        public DateTime when { get; private set; }
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
        void updateGauge(DiagnosticGauge gauge);
    }
    public class NoAuditor : IAuditor
    {
        public T wrapFunction<T>(Func<Action<GaugeStatus, int>, T> action, string name, string category) { return action((gs, v) => { }); }
        public void wrapAction(Action<Action<GaugeStatus, int>> action, string name, string category) { action((gs, v) => { }); }
        public T note<T>(Func<T> action, string name, string category) { return action(); }
        public void updateGauge(DiagnosticGauge gauge) { }
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
                    gpf(gauge.update(gs, v));
                });
                gpf(gauge.update(GaugeStatus.Completed, 100));
            }
            catch (Exception e)
            {
                gpf(gauge.update(GaugeStatus.Failed, 100));
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
                    gpf(gauge.update(gs, v));
                });
                gpf(gauge.update(GaugeStatus.Completed, 100));
                return result;
            }
            catch (Exception e)
            {
                gpf(gauge.update(GaugeStatus.Failed, 100));
                throw e;
            }
        }
        public T note<T>(Func<T> action, string name, string category)
        {
            mf(new DiagnosticMessage(name, category, DateTime.Now));
            return action();
        }
        public void updateGauge(DiagnosticGauge gauge)
        {
            gpf(gauge);
        }
    }
}
