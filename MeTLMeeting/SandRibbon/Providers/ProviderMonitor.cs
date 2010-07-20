using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using agsXMPP;
using SandRibbon.Utils.Connection;
using System.Timers;
using System.Collections.Generic;

namespace SandRibbon.Components
{
    public partial class ProviderMonitor
    {
        static ProviderMonitor()
        {
            JabberWire.LookupServer();
        }
        private static List<ServerStatus> SERVERS =
                new List<ServerStatus>(){
                    new ServerStatus{
                        label="Resource", 
                        CheckStatus=(server)=>server.Ping(Constants.JabberWire.SERVER)},
                    new ServerStatus{
                        label="Messaging", 
                        CheckStatus=(server)=>{
                            var conn= new XmppClientConnection(Constants.JabberWire.SERVER);
                            conn.AutoAgents = false;
                            conn.OnReadXml += (_sender,xml)=>{
                                if(Application.Current != null)
                                    Application.Current.Dispatcher.adoptAsync((Action)delegate
                                    {
                                        server.ok=true;
                                    });
                            };
                            conn.Open("NOT_AUTHORIZED", "aCriminal");
                        }},
                    new ServerStatus{
                        label="Authentication", 
                        CheckStatus=(server)=>server.Ping("my.monash.edu.au")}
                };
        public static void HealthCheck(Action healthyBehaviour)
        {
            //var currentStack = new System.Diagnostics.StackTrace();
            try
            {
                foreach (var server in SERVERS)
                    server.ok = false;
                checkServers();
                int attempts = 0;
                const int MILIS_BETWEEN_TRIES = 1000;
                var timer = new Timer(MILIS_BETWEEN_TRIES);
                timer.Elapsed += 
                (sender, args) =>
                {
                    var brokenServers = SERVERS.Where(s => !s.ok);
                    attempts++;
                    if (brokenServers.Count() == 0)
                    {
                        timer.Stop();
                        timer.Dispose();
                        Application.Current.Dispatcher.adopt((Action)delegate
                        {
                            healthyBehaviour();
                        });
                    }
                    else
                    {
                        Commands.ServersDown.Execute(brokenServers);
                    }
                };
                timer.Start();
            }
            catch (Exception e)
            {
                App.Now("Sorry, might not be able to throw the current callstack");
                //  throw new Exception(currentStack.ToString(), e);
            }
        }
        private static void checkServers()
        {
            foreach (var server in SERVERS)
                server.CheckStatus(server);
        }
    }
    public class ServerStatus
    {
        public string label{get;set;}
        public bool ok { get; set; }
        public Action<ServerStatus> CheckStatus;
        private bool alreadyRetried = false;
        public void Ping(string uri)
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
                        ping.SendAsync(uri, null);//Try again
                        alreadyRetried = true;
                    }
                }
            };
            ping.SendAsync(uri, null);
        }
    }
}