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
using agsXMPP.protocol.x.muc;
using System.Threading;
using agsXMPP.protocol.client;
using agsXMPP.protocol.extensions.pubsub;
using agsXMPP.Xml.Dom;

namespace Sandbox
{
    public partial class XmppPubSub : Window
    {
        const int population = 5;
        //const string SERVER = "radar.adm.monash.edu";
        const string SERVER = "civic.adm.monash.edu";
        //const string SERVER = "racecar.adm.monash.edu";
        Jid PUBSUB = new Jid("pubsub."+SERVER);
        string STROKE = Enumerable.Range(0, 1500).Aggregate("<STROKE>", (acc, item) => acc += "a");
        Timer timer;
        long messageCount = 0;
        long previousCount = 0;
        long requests = 0;
        public XmppPubSub()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<XmppClientConnection, List<string>> status = new Dictionary<XmppClientConnection, List<string>>();
            status = Enumerable.Range(0, population).Select(i=>new Jid(i.ToString()+"@"+SERVER)).Aggregate(status,
                (acc, jid)=>{
                    var conn = new XmppClientConnection(jid.Server);
                    conn.OnAuthError += (_sender, error) => MessageBox.Show(error.ToString());
                    IqCB createResult = (_sender, iq, data)=>{
                        Console.WriteLine(iq);
                    };
                    conn.OnLogin += (o) => 
                    {
                        var manager = new PubSubManager(conn);
                        foreach(var peer in Enumerable.Range(0,population))
                        {
                            new PubSubManager(conn).Subscribe(PUBSUB, jid, "mr"+peer);
                        };
                    };
                    conn.OnIq += (_sender, msg) =>
                    {
                        var iq = msg.ToString();
                        if (iq.Contains("subscription"))
                        {
                            status[conn].Add(iq);
                            Console.WriteLine(conn.Username + " has subscribed to " + status[conn].Count() + " peers");
                        }
                    };
                    conn.OnMessage += (_sender, msg) =>
                    {
                        Interlocked.Increment(ref messageCount);
                    };
                    conn.OnRegistered += (registration) => Console.WriteLine("Registered " + registration);
                    conn.OnRegisterError += (_sender, error) => Console.WriteLine("Already registered "+error);
                    conn.OnError += (_sender, error) => MessageBox.Show("Error" + error);
                    conn.OnReadXml += (_sender, xml) =>{
                        //Console.WriteLine("IN "+xml);
                    };
                    conn.OnWriteXml += (_sender, xml) =>
                    {
                        //Console.WriteLine("OUT" + xml);
                    };
                    //conn.RegisterAccount = true;
                    conn.Open(jid.User, "examplePassword");
                    conn.AutoAgents = false;
                    acc.Add(conn, new List<string>());
                    return acc; 
                });
            DateTime start = new DateTime(0l);
            timer = new Timer((_state) =>
            {
                if (status.Values.ToList().All(l => l.Count() == population))
                {
                    var current = Interlocked.Read(ref messageCount);
                    var previous = Interlocked.Read(ref previousCount);
                    var count = current - previous;
                    if (start.Ticks == 0)
                        start = DateTime.Now;
                    foreach (var conn in status.Keys.ToList())
                    {
                        var pubsub = new PubSubManager(conn);
                        var item = new agsXMPP.protocol.extensions.pubsub.Item();
                        item.ChildNodes.Add(new Element("entry", "CONTENT", "http://www.w3.org/2005/Atom"));
                        pubsub.PublishItem(PUBSUB, "mr" + conn.Username, item);
                        Interlocked.Increment(ref requests);
                    }
                    Console.WriteLine("" + current + "-" + previous + "=" + count + "(" + current + "/" + Interlocked.Read(ref requests) + ")");
                    Interlocked.Exchange(ref previousCount, current);
                }
                else
                {
                    Console.WriteLine("Current subscriptions: "+status.Values.Aggregate(0, (acc, subscriptions)=>acc+subscriptions.Count()));
                }
            }, null, 5000, 1000);
        }
    }
}
