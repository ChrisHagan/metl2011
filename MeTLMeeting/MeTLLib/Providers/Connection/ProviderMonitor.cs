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
        private List<ServerStatus> internalServers;
        private List<ServerStatus> SERVERS
        {
            get
            {
                if (internalServers == null)
                    internalServers = 
                        new List<ServerStatus>(){
                    new ServerStatus{
                        label="Resource", 
                        CheckStatus=(server)=>server.Ping(metlServerAddress.uri)},
                    new ServerStatus{
                        label="Messaging", 
                        CheckStatus=(server)=>{
                            var conn= new XmppClientConnection(metlServerAddress.host);
                            conn.AutoAgents = false;
                            conn.OnXmppConnectionStateChanged += (_sender, state) => 
                            {
                                if (state == XmppConnectionState.Connected)
                                {    
                                    server.ok = true;
                                    Trace.TraceWarning("ProviderMonitor: "+server.label+" up"); 
                                }
                            };
                            conn.Open("NOT_AUTHORIZED", "aCriminal");
                        }},
                    new ServerStatus{
                        label="Authentication", 
                        CheckStatus=(server)=>server.Ping(new System.Uri("http://my.monash.edu.au", UriKind.Absolute))}
                    };
                return internalServers;
            }
        }
        public void HealthCheck(Action healthyBehaviour)
        {
            if (healthyBehaviour == null) throw new ArgumentNullException("healthyBehaviour", "Argument cannot be null.");
            //var currentStack = new System.Diagnostics.StackTrace();
            try
            {
                foreach (var server in SERVERS)
                    server.ok = false;
                checkServers();
                int attempts = 0;
                const int MILIS_BETWEEN_TRIES = 1000;
                ITimer timer = null;
                Action action = () =>
                    {
                        var brokenServers = SERVERS.Where(s => !s.ok);
                        attempts++;
                        if (brokenServers.Count() == 0)
                        {
                            timer.Stop();
                            timer.Dispose();
                            healthyBehaviour();
                        }
                        else
                        {
                            Commands.ServersDown.Execute(brokenServers);
                        }
                    };
                timer = this.timerFactory.getTimer(MILIS_BETWEEN_TRIES, action);
                timer.Start();
                action.Invoke();
            }
            catch (Exception e)
            {
                Trace.TraceError("Sorry, might not be able to throw the current callstack");
                throw new Exception("ProviderMonitor unable to ensure connection to servers", e);
            }
        }
        private void checkServers()
        {
            foreach (var server in SERVERS)
                server.CheckStatus(server);
        }
    }
    class ServerStatus
    {
        public string label { get; set; }
        public bool ok { get; set; }
        public Action<ServerStatus> CheckStatus;
        private bool alreadyRetried = false;
        public void Ping(System.Uri uri)
        {
            var ping = new System.Net.NetworkInformation.Ping();
            ping.PingCompleted += (_sender, pingArgs) =>
            {
                if (pingArgs.Reply != null && pingArgs.Reply.Status == IPStatus.Success)
                {
                    ok = true;
                }
                else
                {
                    ok = false;
                    if (!alreadyRetried)
                    {
                        ping.SendAsync(uri.ToString(), null);//Try again
                        alreadyRetried = true;
                    }
                }
            };
            ping.SendAsync(uri.Host, null);
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