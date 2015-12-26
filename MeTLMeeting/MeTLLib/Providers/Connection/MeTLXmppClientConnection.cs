using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using agsXMPP.protocol.extensions.bosh;

using agsXMPP.Xml;
using agsXMPP.Xml.Dom;
using agsXMPP.Net;
using agsXMPP.Idn;
using agsXMPP;
using System.IO;
using agsXMPP.IO.Compression;
using System.Net.Sockets;
using System.Collections;
using agsXMPP.protocol.client;
using agsXMPP.Net.Dns;
using agsXMPP.protocol.Base;
using agsXMPP.protocol.iq.agent;
using agsXMPP.protocol.iq.roster;
using agsXMPP.protocol.extensions.caps;
using agsXMPP.protocol.iq.disco;
using agsXMPP.Sasl;
using agsXMPP.protocol.iq.register;
using System.Net;
using agsXMPP.protocol.iq.auth;
using agsXMPP.protocol.extensions.compression;
using agsXMPP.Exceptions;
using agsXMPP.protocol.tls;
using System.Security.Authentication;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using agsXMPP.protocol.stream;
using agsXMPP.protocol.sasl;
using agsXMPP.protocol.iq.bind;
using agsXMPP.protocol.iq.session;
using agsXMPP.Sasl;

namespace MeTLLib.Providers.Connection
{
    public class MeTLXmppClientConnection : XmppClientConnection
    {
        public MeTLXmppClientConnection(string domain, string server) : base()
        {
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
                Console.WriteLine("ClientSocket Error: " + e.Message);
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