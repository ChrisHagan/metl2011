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
//using Ninject;

namespace MeTLLib.Providers.Connection
{
    public interface IProviderMonitor
    {
        void HealthCheck(Action healthyBehaviour);
    }
    public class ProductionProviderMonitor : IProviderMonitor
    {
        public MetlConfiguration metlServerAddress { get; protected set; }
        public ITimerFactory timerFactory { get; protected set; }
        public IReceiveEvents receiveEvents { get; protected set; }
        public IWebClient client { get; protected set; }
        public IAuditor auditor { get; protected set; }
        public ProductionProviderMonitor(
            MetlConfiguration _metlServerAddress,
            ITimerFactory _timerFactory,
            IReceiveEvents _receiveEvents,
            IWebClient _webCleint,
            IAuditor _auditor
            )
        {
            auditor = _auditor;
            metlServerAddress = _metlServerAddress;
            timerFactory = _timerFactory;
            client = _webCleint;
        }
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
                    auditor.trace("CRASH: MeTLLib::ProviderMonitor::HealthCheck managed to get a null healthyBehaviour.  This is NOT healthy behaviour.");
                    return;
                }
                var uri = metlServerAddress.serverStatus;
                if (client.downloadString(uri).Trim().ToLower() == "ok")
                {
                    healthyBehaviour();
                }
                else
                {
                    auditor.trace(String.Format("CRASH: (Fixed)MeTLLib::ProviderMonitor::HealthCheck could not ping {0}", uri));
                    if (attempts >= maximum)
                    {
                        receiveEvents.serversDown(uri);
                    }
                    else
                    {
                        HealthCheck(healthyBehaviour, attempts++);
                    }
                }
            }
            catch (Exception e) { 
                auditor.error("HealthCheck", "ProductionProviderMonitor", e);
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