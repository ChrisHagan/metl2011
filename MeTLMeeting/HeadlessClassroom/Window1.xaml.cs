using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Authentication;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.protocol.x.muc;
using SandRibbon.Providers;

namespace HeadlessClassroom
{
    public partial class Window1 : Window
    {
        public static string SERVER = "reifier.adm.monash.edu";
        public static string MUC = "conference." + SERVER;
        public static string ROOM = "auto";
        private static int interval = 1000;
        private int population = 10;
        public ObservableCollection<HeadlessConn> studentList = new ObservableCollection<HeadlessConn>();
        private Timer timer;
        private static Window1 instance;
        public Window1()
        {
            InitializeComponent();
            instance = this;
            Loaded += (sender, args) =>
            {
                loadStudents();
            };
        }
        private void loadStudents()
        {
            var mac = getMacAddress();
            foreach (var i in Enumerable.Range(0, population))
                studentList.Add(new HeadlessConn(string.Format("{1}_student{0}", i, mac)));
        }
        public static void rate(string rate)
        {
            instance.Dispatcher.Invoke((Action)delegate
            {
                instance.rateDisplay.Text = rate;
            });
        }
        private string getMacAddress()
        {
            var mac = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(n => n.OperationalStatus == OperationalStatus.Up);
            return (mac == null) ? "unknown" : mac.Id;
        }
        public class HeadlessConn : XmppClientConnection
        {
            private static long shoutsReceived = 0;
            private Jid MUC = new Jid(ROOM + "@" + Window1.MUC);
            private Jid ME;
            private static string MESSAGE;
            private static string TEACHER = "Student0";
            public string name { get; set; }
            private Timer publishingTimer;
            private Timer privateTimer;
            static HeadlessConn()
            {
                MESSAGE = "/PUBLIC_MESSAGE ";
                for (int i = 0; i < 4500; i++)
                    MESSAGE += "B";
            }
            public HeadlessConn(string name)
                : base(Window1.SERVER)
            {
                this.name = name;
                log(name);
                ME = new Jid(name + "@conference." +Window1.SERVER);
                AutoAgents = false;
                OnAuthError += (_sender, error) =>
                {
                    if (error.TagName == "failure")
                    {
                        RegisterAccount = true;
                        Open(name, "examplePassword");
                    }
                    else
                    {
                        throw new AuthenticationException(error.ToString());
                    }
                };
                OnLogin += (o) =>
                {
                    var manager = new MucManager(this);
                    manager.JoinRoom(MUC, name);
                    manager.JoinRoom(ME, name);
                    log(name + " logged in");
                };
                OnPresence += (_sender, presence) =>
                {
                    //log(presence.ToString()); 
                };
                OnMessage += (_sender, msg) =>
                {
                    increment(ref shoutsReceived);
                };
                OnRegistered += (registration) => { };
                OnRegisterError += (_sender, error) => { throw new Exception(error.ToString()); };
                OnError += (_sender, error) => { throw error; };
                OnReadXml += (_sender, xml) =>
                {
                    //log("In size:"+xml.ToString().Length);
                };
                OnWriteXml += (_sender, xml) =>
                {
                    //log("Out size:"+xml.ToString().Length);
                };
                Open(name, "examplePassword");
            }
            public void shout()
            {
                Send(new Message(MUC, MessageType.groupchat, MESSAGE));
            }
            public void note()
            {
                Send(new Message(ME, MessageType.groupchat, MESSAGE));
            }
            private void beginHistoryRetrieval()
            {
                try
                {
                    var resource = "http://" + Window1.SERVER + ":1749/" + Window1.ROOM.ToLower() + "@" + Window1.MUC;
                    HttpResourceProvider.secureGetString(resource);
                    Thread.Sleep(10000);
                    beginHistoryRetrieval();
                }
                finally 
                {
                    Console.WriteLine("History retrieval broke");
                }
            }
            private static int reportingInterval = 100;
            private void increment(ref long counter)
            {
                var value = Interlocked.Read(ref counter);
                Interlocked.Increment(ref counter);
                if (value % reportingInterval == 0)
                    report(value);
            }
            private void log(string message)
            {
                Console.WriteLine(message);
            }
            static long previousTicks = 0;
            private void report(long currentReceived)
            {
                var then = Interlocked.Read(ref previousTicks);
                var now = DateTime.Now.Ticks;
                var message = string.Format("{0} at {1:#.###} tps", currentReceived, reportingInterval / (new TimeSpan(now - then).TotalSeconds));
                log(message);
                rate(message);
                Interlocked.Exchange(ref previousTicks, now);
            }
        }
        private void takeSomeNotes(object sender, RoutedEventArgs e)
        {
            doTakeSomeNotes();
        }
        private void doTakeSomeNotes()
        {
            foreach (var student in studentList)
                student.note();
        }
        private void talkOutLoud(object sender, RoutedEventArgs e)
        {
            doTalkOutLoud();
        }
        private void doTalkOutLoud()
        {
            foreach (var student in studentList)
                student.shout();
        }
        private void RemoveOptionsLeavingOnlyThisAndThisIsDisabled(object sender)
        {
            foreach (var option in participation.Children)
            {
                ((Button)option).IsEnabled = false;
                ((Button)option).Visibility = Visibility.Collapsed;
            }
            ((Button)sender).Visibility = Visibility.Visible;
        }
        private void Author(object sender, RoutedEventArgs e)
        {
            var author = studentList.First();
            timer = new Timer((_state) => author.shout(), null, 10000, interval);
            RemoveOptionsLeavingOnlyThisAndThisIsDisabled(sender);
        }
        private void Authors(object sender, RoutedEventArgs e)
        {
            timer = new Timer((_state) => doTalkOutLoud(), null, 10000, interval);
            RemoveOptionsLeavingOnlyThisAndThisIsDisabled(sender);
        }
        private void Students(object sender, RoutedEventArgs e)
        {
            timer = new Timer((_state) => doTakeSomeNotes(), null, 10000, interval);
            RemoveOptionsLeavingOnlyThisAndThisIsDisabled(sender);
        }
    }
}