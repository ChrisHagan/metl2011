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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MeTLLib;
using System.Diagnostics;
using MeTLLib.DataTypes;
using System.Windows.Threading;
using MeTLLib.Providers.Connection;
using Ninject.Modules;
using MeTLLib.Providers;
using System.IO;
using agsXMPP.Xml;
using agsXMPP.Xml.Dom;
using SandRibbon.Utils;

namespace MeTLSimulator
{
    class LiveHttpHistoryProvider : HttpHistoryProvider {
        public List<Element> elements = new List<Element>();
        protected override void parseHistoryItem(MemoryStream stream, JabberWire wire)
        {
            var parser = new StreamParser();
            parser.OnStreamElement += ((_sender, node) => { 
                                           elements.Add((Element)node);
                                       });
            parser.Push(stream.GetBuffer(), 0, (int)stream.Length);
            parser.Push(closeTag, 0, closeTag.Length);
        }
    }
    public partial class MainWindow : Window
    {
        ClientConnection conn;
        public MainWindow()
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
            InitializeComponent();
            onExtendedDesktop();
            ClientFactory.kernel.Unbind<HttpHistoryProvider>();
            ClientFactory.kernel.Bind<HttpHistoryProvider>().To<LiveHttpHistoryProvider>().InSingletonScope();
            conn = ClientFactory.Connection(MeTLServerAddress.serverMode.PRODUCTION);
            conn.events.StatusChanged += new MeTLLibEventHandlers.StatusChangedEventHandler(events_StatusChanged);
            conn.Connect("chagan", "vidcajun2");
        }
        void onExtendedDesktop() {
            var screens =  System.Windows.Forms.Screen.AllScreens;
            if (screens.Length < 2) return;
            var extendedDesktop = screens[1];
            WindowState = System.Windows.WindowState.Normal;
            this.Left = extendedDesktop.WorkingArea.X;
            this.Top = extendedDesktop.WorkingArea.Top;
            this.Width = extendedDesktop.WorkingArea.Width;
            this.Height = extendedDesktop.WorkingArea.Height;
        }
        DispatcherTimer startTimer = new DispatcherTimer();
        void events_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            startTimer.Interval = TimeSpan.FromMilliseconds(5000);
            startTimer.Tick += (_s1, _a1) =>
            {
                startTimer.Stop();
                //var slide = 1431411;//Mic's slide
                //var slide = 1598402;//The big pedal one
                var slide = 1629401; //Gordon's essence of mammal
                if (e.isConnected)
                    conn.getHistoryProvider().Retrieve<PreParser>(null, null, _parser =>
                    {
                        var elements = ((LiveHttpHistoryProvider)conn.getHistoryProvider()).elements;
                        var iterator = elements.GetEnumerator();
                        timer = new DispatcherTimer();
                        timer.Tick += (_s, _a) =>
                        {
                            iterator.MoveNext();
                            var message = iterator.Current;
                            if (message == null) return;
                            Logger.Log(message.ToString());
                            foreach (var ink in message.SelectElements<MeTLStanzas.Ink>(true))
                                simulationField.Strokes.Add(ink.Stroke.stroke);
                            foreach (var text in message.SelectElements<MeTLStanzas.TextBox>(true))
                                simulationField.Children.Add(text.Box.box);
                            foreach (var dirtyInk in message.SelectElements<MeTLStanzas.DirtyInk>(true))
                            {
                                var existingStrokes = simulationField.Strokes.Where(s =>
                                    s.sum().checksum.ToString().Equals(dirtyInk.element.identifier));
                                foreach (var stroke in existingStrokes.ToList())
                                    simulationField.Strokes.Remove(stroke);
                            }
                        };
                        timer.Interval = TimeSpan.FromMilliseconds(100);
                        timer.Start();
                    }, slide.ToString());
            };
            startTimer.Start();
        }
        DispatcherTimer timer;
    }
}
