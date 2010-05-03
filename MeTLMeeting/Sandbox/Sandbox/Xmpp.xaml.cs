using System;
using System.Collections.Generic;
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
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.protocol.x.muc;
using agsXMPP.Xml.Dom;
using System.Threading;

namespace Sandbox
{
    public partial class Xmpp : Window
    {
        const int population = 10;
        const string AWKWARD = "drawkward.adm.monash.edu";
        const string RADAR = "radar.adm.monash.edu";
        Jid ROOM = new Jid("4@" + AWKWARD);
        string STROKE = Enumerable.Range(0, 3000).Aggregate("<STROKE>", (acc, item) => acc += "a");
        Timer timer;
        long messageCount = 0;
        public Xmpp()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<XmppClientConnection, bool> status = new Dictionary<XmppClientConnection, bool>();
            foreach (var server in new[] { AWKWARD, RADAR})
            {
                status = Enumerable.Range(0, population).Select(i => new Jid(i.ToString() + "@" + server)).Aggregate(status,
                    (acc, jid) =>
                    {
                        Console.WriteLine(jid.User);
                        var conn = new XmppClientConnection(jid.Server);
                        conn.OnAuthError += (_sender, error) => MessageBox.Show(error.ToString());
                        conn.OnLogin += (o) =>{
                            Console.WriteLine("Received login ack from " + jid.Bare);
                            new MucManager(conn).JoinRoom(ROOM, conn.Username, true);
                        };
                        conn.OnPresence += (_sender, presence) =>
                        {
                            Console.WriteLine("Presence ack from " + jid.Bare);
                            status[conn] = true;
                        } ;
                        conn.OnMessage += (_sender, msg) => Interlocked.Increment(ref messageCount);
                        conn.OnRegistered += (registration) => Console.WriteLine("Registered " + registration);
                        conn.OnRegisterError += (_sender, error) => Console.WriteLine("Already registered " + error);
                        //conn.RegisterAccount = true;
                        conn.Open(jid.User, "examplePassword");
                        acc.Add(conn, false);
                        return acc;
                    });
                DateTime start = new DateTime(0l);
                timer = new Timer((_state) =>
                {
                    if (status.Values.ToList().All(v => v == true))
                    {
                        var count = Interlocked.Read(ref messageCount);
                        if (count % population < 50)
                        {
                            if (start.Ticks == 0)
                                start = DateTime.Now;
                            else
                            {
                                var sps = count / (DateTime.Now - start).TotalSeconds;
                                Console.WriteLine(count + " (" + sps + ")");
                            }
                            foreach (var conn in status.Keys.ToList())
                            {
                                conn.Send(new Message(ROOM, conn.MyJID, MessageType.groupchat, STROKE));
                            }
                        }
                    }
                }, null, 8000, 1000);
            }
        }
    }
}
