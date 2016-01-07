using System;
using agsXMPP;

namespace MeTLLib.Providers.Connection
{
    public class MeTLXmppClientConnection : XmppClientConnection
    {
        public IAuditor auditor { get; protected set; }
        public MeTLXmppClientConnection(string domain, string server,IAuditor _auditor) : base()
        {
            auditor = _auditor;
            this.Server = domain;
            this.ConnectServer = server;
            this.SocketConnectionType = agsXMPP.Net.SocketConnectionType.Direct;
            this.UseStartTLS = true;
            this.UseSSL = false; // this should be set to false when UseStartTLS is set to true.  UseStartTLS should deprecate useSSL.
            this.AutoAgents = false;
            this.AutoResolveConnectServer = false;
            this.UseCompression = false;
            ClientSocket.OnError += (s, e) =>
            {
                auditor.error("ClientSocket Error","MeTLXmppClientConnection",e);
            };
        }
        public string description
        {
            get
            {
                var description = "";
                var cs = ClientSocket as agsXMPP.Net.ClientSocket;
                description = String.Format("Connection: Compressed[{0}], Connected[{1}], SSL[{2}], StartTLS[{3}]", cs.Compressed, cs.Connected, cs.SSL, cs.SupportsStartTls);
                return description;
            }
        }
    }
}