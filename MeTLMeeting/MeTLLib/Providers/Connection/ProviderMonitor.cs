using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using agsXMPP;
using System.Timers;
using System.Collections.Generic;
using System.Diagnostics;
using Ninject;

namespace MeTLLib.Providers.Connection
{
    public interface IProviderMonitor
    {
        void HealthCheck(Action healthyBehaviour);
    }
    public partial class ProductionProviderMonitor : IProviderMonitor
    {
        [Inject]
        public MeTLServerAddress metlServerAddress { private get; set; }
        [Inject]
        public ITimerFactory timerFactory { private get; set; }
        public void HealthCheck (Action healthyBehaviour){
            HealthCheck(healthyBehaviour,0);
        }
        private void HealthCheck(Action healthyBehaviour,int attempts)
        {
            var maximum = 3;
            try
            {
                if (healthyBehaviour == null)
                {
                    Trace.TraceError("CRASH: MeTLLib::ProviderMonitor::HealthCheck managed to get a null healthyBehaviour.  This is NOT healthy behaviour.");
                    return;
                }
                var uri = metlServerAddress.uri;
                var ping = new System.Net.NetworkInformation.Ping();
                var reply = ping.Send(uri.Host,2000);
                if (reply != null && reply.Status == IPStatus.Success)
                {
                    healthyBehaviour();
                }
                else
                {
                    Trace.TraceError("CRASH: (Fixed)MeTLLib::ProviderMonitor::HealthCheck could not ping {0}", uri);
                    if (attempts >= maximum)
                    {
                        Commands.ServersDown.Execute(uri.Host);
                    }
                    else
                    {
                        HealthCheck(healthyBehaviour, attempts++);
                    }
                }
            }
            catch (Exception e) { 
                Trace.TraceError("CRASH: MeTLLib::ProviderMonitor::HealthCheck threw {0}", e.Message);
            }
        }
    }
    public interface ITimerFactory
    {
        ITimer getTimer(int time, Action elapsed);
    }
    public class ProductionTimerFactory : ITimerFactory
    {
        public ITimer getTimer(int time, Action elapsed)
        {
            return new ProductionTimer(time, elapsed);
        }
    }
    public interface ITimer
    {
        void Stop();
        void Start();
        void Dispose();
    }
    class ProductionTimer : ITimer
    {
        private Timer internalClock;
        public ProductionTimer(int time, Action elapsed)
        {
            internalClock = new Timer(time);
            internalClock.Elapsed += (sender, args) => elapsed();
        }
        public void Stop()
        {
            if (internalClock != null)
            {
                internalClock.Stop();
            }
        }
        public void Start()
        {
            if (internalClock != null)
                internalClock.Start();
        }
        public void Dispose()
        {
            internalClock.Dispose();
        }
    }
}