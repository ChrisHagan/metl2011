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
        const int population = 80;
        Random random = new Random();
        const int percentageLikelihoodOfPublishing = 10;
        //const string SERVER = "drawkward.adm.monash.edu";
        //const string SERVER = "civic.adm.monash.edu";
        const string SERVER = "racecar.adm.monash.edu";
        const string MUC = "conference." + SERVER;
        string STROKE = Enumerable.Range(0, 1500).Aggregate("<STROKE>", (acc, item) => acc += "a")+"Z";
        Jid room = new Jid("loggableLoggingImpl@" + MUC);
        Timer timer;
        long messageCount = 0;
        long previousCount = 0;
        
        public Xmpp()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<XmppClientConnection, bool> status = new Dictionary<XmppClientConnection, bool>();
            status = Enumerable.Range(0,population).Select(i=>new Jid(i.ToString()+"@"+SERVER)).Aggregate(status,
                (acc, jid)=>{
                    Console.WriteLine(jid.User);
                    var conn = new XmppClientConnection(jid.Server);
                    conn.OnAuthError += (_sender, error) => MessageBox.Show(error.ToString());
                    conn.OnLogin += (o) =>
                    {
                        var manager = new MucManager(conn);
                        manager.JoinRoom(room, conn.Username, true);
                        manager.JoinRoom(new Jid(conn.Username+"@"+MUC), conn.Username, true);
                    };
                    conn.OnPresence += (_sender, presence) => status[conn] = true;
                    conn.OnMessage += (_sender, msg) => Interlocked.Increment(ref messageCount);
                    conn.OnRegistered += (registration) => Console.WriteLine("Registered " + registration);
                    conn.OnRegisterError += (_sender, error) => Console.WriteLine("Already registered "+error);
                    conn.OnError += (_sender, error) => MessageBox.Show("Error" + error);
                    conn.OnReadXml += (_sender, xml) =>
                    {
                        //Console.WriteLine(xml);
                    };
                    //conn.RegisterAccount = true;
                    conn.Open(jid.User, "examplePassword");
                    acc.Add(conn, false);
                    return acc; 
                });
            DateTime start = new DateTime(0l);
            var period = 250.0;
            timer = new Timer((_state) =>
            {
                if (status.Values.ToList().All(v => v == true))
                {
                    var current = Interlocked.Read(ref messageCount);
                    var previous = Interlocked.Read(ref previousCount);
                    var count = current - previous;
                    if (count % population < population)
                    {
                        if (start.Ticks == 0)
                            start = DateTime.Now;
                        else
                            Console.WriteLine(count*(1000/period));
                        foreach (var conn in status.Keys.ToList())
                        {
                            if (random.Next(100) < percentageLikelihoodOfPublishing)
                                conn.Send(new Message(room, conn.MyJID, MessageType.groupchat, STROKE));
                            else{
                                var jid = new Jid(conn.Username+"@"+MUC);
                                conn.Send(new Message(jid, conn.MyJID, MessageType.groupchat, STROKE));
                            }
                        }
                    }
                    Interlocked.Exchange(ref previousCount, current);
                }
                else
                {
                    Console.WriteLine("Currently logged in:"+status.Values.ToList().Where(v=>v==true).Count());
                }
            }, null, 8000, (int)period);
        }
    }
}
